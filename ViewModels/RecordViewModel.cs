using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JobMore.Models;
using JobMore.Services;
using JobMore.Views;

namespace JobMore.ViewModels
{
    /// <summary>기록 화면 — 종료 포함 전체 히스토리 + 통계 + 자소서·자격증 + 결과 되돌리기.</summary>
    public class RecordViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;

        public ObservableCollection<ApplicationRecord> History { get; } = new();
        public ObservableCollection<CoverLetter> CoverLetters { get; } = new();
        public ObservableCollection<Certificate> Certificates { get; } = new();

        /// <summary>단계/결과 필터가 적용되는 화면용 뷰.</summary>
        public ICollectionView HistoryView { get; }

        public IEnumerable<object> StageFilterOptions { get; }

        // ── 서브탭 ([목록] / [통계·차트]) ──
        private bool _showStats;
        public bool ShowStats { get => _showStats; set { if (SetProperty(ref _showStats, value)) OnPropertyChanged(nameof(ShowList)); } }
        public bool ShowList => !_showStats;
        public RelayCommand ShowListCommand { get; }
        public RelayCommand ShowStatsCommand { get; }

        // ── 합격 퍼널 ──
        public ObservableCollection<FunnelStage> Funnel { get; } = new();

        // ── 도넛 차트 ──
        public ObservableCollection<DonutSlice> JobDonut { get; } = new();   // 직무별 지원
        public ObservableCollection<DonutSlice> EmpDonut { get; } = new();   // 고용형태별 지원
        private string _jobDonutEmpty, _empDonutEmpty;
        public string JobDonutEmpty { get => _jobDonutEmpty; private set => SetProperty(ref _jobDonutEmpty, value); }
        public string EmpDonutEmpty { get => _empDonutEmpty; private set => SetProperty(ref _empDonutEmpty, value); }

        public RecordViewModel()
        {
            RestoreCommand = new RelayCommand(_ => Restore(),
                _ => SelectedRecord != null && SelectedRecord.IsTerminal);
            OpenUrlCommand = new RelayCommand(_ => OpenUrl(),
                _ => SelectedRecord != null && !string.IsNullOrWhiteSpace(SelectedRecord.App.JobUrl));
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedRecord != null);
            ShowListCommand = new RelayCommand(_ => ShowStats = false);
            ShowStatsCommand = new RelayCommand(_ => ShowStats = true);

            LoadHistory();

            HistoryView = CollectionViewSource.GetDefaultView(History);
            HistoryView.Filter = FilterPredicate;

            var opts = new List<object> { "전체", Stage.Offer, Stage.Rejected, Stage.Withdrawn };
            StageFilterOptions = opts;
            _stageFilter = "전체";

            foreach (var c in _data.GetCoverLetters()) CoverLetters.Add(c);
            foreach (var c in _data.GetCertificates()) Certificates.Add(c);
            LetterCount = CoverLetters.Count;
            CertCount = Certificates.Count;
        }

        private object _stageFilter;
        public object StageFilter
        {
            get => _stageFilter;
            set { if (SetProperty(ref _stageFilter, value)) HistoryView?.Refresh(); }
        }

        private bool FilterPredicate(object item)
        {
            if (item is not ApplicationRecord r) return false;
            if (_stageFilter is Stage s) return r.StageEnum == s;
            return true; // "전체"
        }

        private void LoadHistory()
        {
            History.Clear();
            var apps = _data.GetAll().ToList();
            // 기록 탭은 '종료된 건'만 — 최종합격/불합격/중도포기
            foreach (var a in apps
                         .Where(a => a.Stage is Stage.Offer or Stage.Rejected or Stage.Withdrawn)
                         .OrderByDescending(a => a.AddedDate))
                History.Add(new ApplicationRecord(a));
            RefreshStats(apps);
        }

        private void RefreshStats(List<Models.Application> apps)
        {
            TotalCount     = apps.Count;
            OfferCount     = apps.Count(a => a.Stage == Stage.Offer);
            RejectedCount  = apps.Count(a => a.Stage == Stage.Rejected);
            WithdrawnCount = apps.Count(a => a.Stage == Stage.Withdrawn);

            int Reached(params Stage[] stages)
            {
                var set = new HashSet<Stage>(stages);
                return apps.Count(a => set.Contains(a.Stage) || a.StageLogs.Any(l => set.Contains(l.Stage)));
            }
            int applied = Reached(Stage.Applied, Stage.DocumentPassed, Stage.FirstInterview,
                Stage.SecondInterview, Stage.Negotiation, Stage.Offer);
            int offered = Reached(Stage.Offer);
            OfferRate = applied == 0 ? "—" : $"{offered * 100.0 / applied:0}%";

            // 합격 퍼널 (단계별 도달 수)
            int docPassed = Reached(Stage.DocumentPassed, Stage.FirstInterview,
                Stage.SecondInterview, Stage.Negotiation, Stage.Offer);
            int interviewed = Reached(Stage.FirstInterview, Stage.SecondInterview, Stage.Negotiation, Stage.Offer);

            Funnel.Clear();
            int top = applied == 0 ? 1 : applied;
            Funnel.Add(new FunnelStage("지원",     applied,     applied,     top, "#7C5CFC"));
            Funnel.Add(new FunnelStage("서류합격", docPassed,   applied,     top, "#5AA9F8"));
            Funnel.Add(new FunnelStage("면접",     interviewed, applied,     top, "#F0922B"));
            Funnel.Add(new FunnelStage("최종합격", offered,     applied,     top, "#1F9D57"));

            // 직무별 지원 도넛
            var jobGroups = apps
                .Select(a => string.IsNullOrWhiteSpace(a.Position) ? "미입력" : a.Position.Trim())
                .GroupBy(p => p)
                .ToDictionary(g => g.Key, g => g.Count());
            JobDonut.Clear();
            foreach (var s in DonutChart.Build(jobGroups)) JobDonut.Add(s);
            JobDonutEmpty = JobDonut.Count == 0 ? "데이터 없음" : null;

            // 고용형태별 지원 도넛
            var empGroups = apps
                .GroupBy(a => EnumHelper.GetDescription(a.EmploymentType))
                .ToDictionary(g => g.Key, g => g.Count());
            EmpDonut.Clear();
            foreach (var s in DonutChart.Build(empGroups)) EmpDonut.Add(s);
            EmpDonutEmpty = EmpDonut.Count == 0 ? "데이터 없음" : null;

            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(OfferCount));
            OnPropertyChanged(nameof(RejectedCount));
            OnPropertyChanged(nameof(WithdrawnCount));
            OnPropertyChanged(nameof(OfferRate));
        }

        private ApplicationRecord _selected;
        public ApplicationRecord SelectedRecord
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public RelayCommand RestoreCommand { get; }
        public RelayCommand OpenUrlCommand { get; }
        public RelayCommand DeleteCommand { get; }

        private void Delete()
        {
            var rec = SelectedRecord;
            if (rec == null) return;
            if (!ConfirmDialog.Show("이 지원 기록을 삭제할까요?\n삭제하면 되돌릴 수 없습니다.", "기록 삭제"))
                return;
            _data.Delete(rec.App);
            SelectedRecord = null;
            LoadHistory();
        }

        private void OpenUrl()
        {
            var url = SelectedRecord?.App.JobUrl;
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.StartsWith("http")) url = "https://" + url;
            try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
            catch { }
        }

        /// <summary>종료된 지원을 진행 단계로 되돌린다 → 홈(지원현황)에 다시 나타남.</summary>
        private void Restore()
        {
            var rec = SelectedRecord;
            if (rec == null || !rec.IsTerminal) return;

            if (!ConfirmDialog.Show(
                    "이 지원을 다시 '진행중'으로 되돌립니다.\n홈(지원현황) 화면에서 다시 확인할 수 있어요. 계속할까요?",
                    "결과 되돌리기"))
                return;

            var app = rec.App;

            // 종료 전 마지막 진행 단계로 복원 (이력에서 찾되, 없으면 '지원')
            Stage restoreTo = app.StageLogs
                .Where(l => l.Stage is not (Stage.Offer or Stage.Rejected or Stage.Withdrawn))
                .OrderByDescending(l => l.ChangedAt)
                .Select(l => (Stage?)l.Stage)
                .FirstOrDefault() ?? Stage.Applied;

            app.Stage = restoreTo;
            _data.LogStage(app, restoreTo);
            _data.Save();

            LoadHistory();
            SelectedRecord = null;

            ConfirmDialog.Info("진행중으로 되돌렸습니다. 홈(지원현황) 탭에서 확인하세요.", "완료");
        }

        public int TotalCount     { get; private set; }
        public int OfferCount     { get; private set; }
        public int RejectedCount  { get; private set; }
        public int WithdrawnCount { get; private set; }
        public string OfferRate   { get; private set; }
        public int LetterCount    { get; }
        public int CertCount      { get; }
    }

    /// <summary>합격 퍼널 한 단계.</summary>
    public class FunnelStage
    {
        public FunnelStage(string name, int count, int applied, int top, string colorHex)
        {
            Name = name; Count = count; ColorHex = colorHex;
            Percent = applied == 0 ? "0%" : $"{count * 100.0 / applied:0}%";
            FillStars = count;
            RestStars = System.Math.Max(top - count, 0);
        }
        public string Name { get; }
        public int Count { get; }
        public string Percent { get; }
        public string ColorHex { get; }
        public double FillStars { get; }   // 막대 채움 비율(star)
        public double RestStars { get; }   // 막대 빈공간(star)
    }

    /// <summary>기록 탭에 보여줄 지원 1건.</summary>
    public class ApplicationRecord
    {
        public ApplicationRecord(Models.Application a)
        {
            App = a;
            Company = a.Company;
            Position = a.Position;
            AppliedDate = a.AppliedDate?.ToString("yyyy-MM-dd") ?? a.AddedDate.ToString("yyyy-MM-dd");
            Stage = EnumHelper.GetDescription(a.Stage);
            StageEnum = a.Stage;
        }
        public Models.Application App { get; }
        public string Company { get; }
        public string Position { get; }
        public string AppliedDate { get; }
        public string Stage { get; }
        public Models.Stage StageEnum { get; }

        public bool IsTerminal =>
            App.Stage is Models.Stage.Offer or Models.Stage.Rejected or Models.Stage.Withdrawn;
    }
}
