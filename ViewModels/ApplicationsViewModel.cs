using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JobMore.Models;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>지원현황 화면 — 목록/검색/필터/대시보드(퍼널)/단계이동/바로가기.</summary>
    public class ApplicationsViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;

        public ObservableCollection<ApplicationViewModel> Applications { get; } = new();
        public ICollectionView ApplicationsView { get; }

        public IEnumerable<Stage> StageOptions { get; } =
            Enum.GetValues(typeof(Stage)).Cast<Stage>();
        /// <summary>인라인 단계 콤보용 — 진행 단계만(결과 단계는 전용 버튼으로).</summary>
        public IEnumerable<Stage> ProgressStageOptions { get; } =
            Enum.GetValues(typeof(Stage)).Cast<Stage>()
                .Where(s => s is not (Stage.Offer or Stage.Rejected or Stage.Withdrawn));
        public IEnumerable<EmploymentType> EmploymentTypeOptions { get; } =
            Enum.GetValues(typeof(EmploymentType)).Cast<EmploymentType>();
        public IEnumerable<Channel> ChannelOptions { get; } =
            Enum.GetValues(typeof(Channel)).Cast<Channel>();
        public IEnumerable<Priority> PriorityOptions { get; } =
            Enum.GetValues(typeof(Priority)).Cast<Priority>();
        public IEnumerable<object> StageFilterOptions { get; }

        // 직무 검색(상세 패널) — 검색해서 선택 + 직접입력 둘 다 가능
        public ObservableCollection<JobResult> JobResults { get; } = new();
        private string _jobSearch = string.Empty;
        public string JobSearchText
        {
            get => _jobSearch;
            set { if (SetProperty(ref _jobSearch, value)) RunJobSearch(); }
        }
        private void RunJobSearch()
        {
            JobResults.Clear();
            if (string.IsNullOrWhiteSpace(JobSearchText)) return;
            foreach (var (name, cat) in JobCatalog.Search(JobSearchText))
                JobResults.Add(new JobResult { Name = name, Category = cat });
        }
        public RelayCommand SelectJobCommand { get; private set; }
        private void SelectJob(JobResult r)
        {
            if (r == null || SelectedApplication == null) return;
            SelectedApplication.Position = r.Name;
            JobSearchText = string.Empty;
        }

        // 회사 색 팔레트 (캘린더에 반영)
        public IEnumerable<string> ColorPalette { get; } = new[]
        {
            "#7C5CFC", "#E25C5C", "#2BB673", "#F0922B", "#2C9BD6", "#D659B0", "#5A6BD8", "#7A8290"
        };
        public RelayCommand SetColorCommand { get; private set; }
        private void SetColor(string hex)
        {
            if (SelectedApplication == null || string.IsNullOrWhiteSpace(hex)) return;
            SelectedApplication.ColorHex = hex;
            _data.Save();
        }

        public ApplicationsViewModel()
        {
            foreach (var a in _data.GetAll())
                Applications.Add(new ApplicationViewModel(a));

            ApplicationsView = CollectionViewSource.GetDefaultView(Applications);
            ApplicationsView.Filter = FilterPredicate;

            var opts = new List<object> { "전체 단계" };
            opts.AddRange(StageOptions.Cast<object>());
            StageFilterOptions = opts;
            _stageFilter = "전체 단계";

            AddCommand     = new RelayCommand(_ => AddApplication());
            DeleteCommand  = new RelayCommand(_ => DeleteSelected(), _ => SelectedApplication != null);
            SaveCommand    = new RelayCommand(_ => Save());
            AdvanceCommand = new RelayCommand(_ => MoveStage(+1), _ => SelectedApplication != null && !SelectedApplication.IsClosed);
            RetreatCommand = new RelayCommand(_ => MoveStage(-1), _ => SelectedApplication != null && !SelectedApplication.IsClosed);
            SelectStepCommand = new RelayCommand(p => SelectStep(p as StepItem), _ => SelectedApplication != null && !SelectedApplication.IsClosed);
            ClearStepDateCommand = new RelayCommand(p => ClearStepDate(p as StepItem), _ => SelectedApplication != null && !SelectedApplication.IsClosed);
            OpenUrlCommand = new RelayCommand(_ => OpenUrl(),
                _ => SelectedApplication != null && !string.IsNullOrWhiteSpace(SelectedApplication.JobUrl));
            OpenRowUrlCommand = new RelayCommand(p => OpenUrlFor(p as ApplicationViewModel));
            SelectJobCommand = new RelayCommand(p => SelectJob(p as JobResult));
            SetColorCommand = new RelayCommand(p => SetColor(p as string));
            OfferResultCommand    = new RelayCommand(_ => EnterResult(Stage.Offer),    _ => SelectedApplication != null);
            RejectResultCommand   = new RelayCommand(_ => EnterResult(Stage.Rejected), _ => SelectedApplication != null);
            WithdrawResultCommand = new RelayCommand(_ => EnterResult(Stage.Withdrawn),_ => SelectedApplication != null);

            LoadEventTypes();
            RefreshStats();
        }

        private ApplicationViewModel _selected;
        public ApplicationViewModel SelectedApplication
        {
            get => _selected;
            set { if (SetProperty(ref _selected, value)) BuildSteps(); }
        }

        // ───── 진행 단계 stepper (관심→…→처우협의) ─────
        private static readonly Stage[] Flow =
        {
            Stage.Interested, Stage.Applied, Stage.DocumentPassed,
            Stage.FirstInterview, Stage.SecondInterview, Stage.Negotiation
        };

        public ObservableCollection<StepItem> Steps { get; } = new();

        // ───── 다음 일정 종류 드롭다운 ─────
        public const string AddNewEventType = "+ 새 항목 추가…";
        public ObservableCollection<string> EventTypeOptions { get; } = new();

        private void LoadEventTypes()
        {
            EventTypeOptions.Clear();
            foreach (var et in _data.GetEventTypes()) EventTypeOptions.Add(et.Name);
            EventTypeOptions.Add(AddNewEventType);
        }

        /// <summary>새 일정 종류 등록(중복 방지) 후 목록 갱신. 등록된 이름 반환.</summary>
        public string AddEventType(string name)
        {
            var et = _data.AddEventType(name);
            if (et == null) return null;
            if (!EventTypeOptions.Contains(et.Name))
                EventTypeOptions.Insert(EventTypeOptions.Count - 1, et.Name); // 센티넬 앞에 삽입
            return et.Name;
        }

        private bool _isInProgressStage;
        public bool IsInProgressStage { get => _isInProgressStage; private set => SetProperty(ref _isInProgressStage, value); }
        private bool _isAtLastStage;
        public bool IsAtLastStage { get => _isAtLastStage; private set => SetProperty(ref _isAtLastStage, value); }

        private void BuildSteps()
        {
            Steps.Clear();
            var vm = SelectedApplication;
            if (vm == null) { IsInProgressStage = false; IsAtLastStage = false; return; }

            int cur = Array.IndexOf(Flow, vm.Stage);   // -1 이면 종료 단계(Offer/Rejected/Withdrawn)
            for (int i = 0; i < Flow.Length; i++)
            {
                var st = Flow[i];
                var log = vm.Model.StageLogs
                    .Where(l => l.Stage == st)
                    .OrderByDescending(l => l.ChangedAt)
                    .FirstOrDefault();

                var step = new StepItem
                {
                    Stage = st,
                    Label = EnumHelper.GetDescription(st),
                    IsCurrent = (i == cur),
                    IsDone = (cur >= 0 && i < cur),
                    IsLast = (i == Flow.Length - 1),
                    PassedDate = log?.ChangedAt          // 주입 전이라 콜백 안 울림
                };
                step.DateChanged = OnStepDateChanged;
                Steps.Add(step);
            }
            IsInProgressStage = cur >= 0;
            IsAtLastStage = (cur == Flow.Length - 1);
        }

        /// <summary>단계(알약) 클릭 → 그 단계의 날짜 캘린더만 토글(이동·자동기록 안 함).</summary>
        public RelayCommand SelectStepCommand { get; private set; }
        private void SelectStep(StepItem step)
        {
            if (step == null) return;
            bool open = !step.IsDateEditing;
            foreach (var s in Steps) s.IsDateEditing = false;  // 하나만 열기
            step.IsDateEditing = open;
        }

        /// <summary>그 단계의 날짜 기록 삭제 (✕).</summary>
        public RelayCommand ClearStepDateCommand { get; private set; }
        private void ClearStepDate(StepItem step)
        {
            var vm = SelectedApplication;
            if (vm == null || step == null) return;
            _data.RemoveStageLogs(vm.Model, step.Stage);
            step.PassedDate = null;       // 콜백은 null이면 무시되므로 안전
            step.IsDateEditing = false;
            RefreshStats();
            ApplicationsView.Refresh();
        }

        /// <summary>stepper에서 날짜를 직접 고르면 해당 단계 로그 날짜를 갱신.</summary>
        private void OnStepDateChanged(StepItem step, DateTime? date)
        {
            var vm = SelectedApplication;
            if (vm == null || date == null) return;

            var log = vm.Model.StageLogs
                .Where(l => l.Stage == step.Stage)
                .OrderByDescending(l => l.ChangedAt)
                .FirstOrDefault();
            if (log == null) { _data.LogStage(vm.Model, step.Stage); log = vm.Model.StageLogs.Last(l => l.Stage == step.Stage); }
            log.ChangedAt = date.Value;
            _data.Save();
            RefreshStats();
            ApplicationsView.Refresh();
        }

        // ── 검색/필터 ──
        private string _search = string.Empty;
        public string SearchText
        {
            get => _search;
            set { if (SetProperty(ref _search, value)) ApplicationsView.Refresh(); }
        }

        private object _stageFilter;
        public object StageFilter
        {
            get => _stageFilter;
            set { if (SetProperty(ref _stageFilter, value)) ApplicationsView.Refresh(); }
        }

        private bool FilterPredicate(object item)
        {
            if (item is not ApplicationViewModel vm) return false;

            // 홈(지원현황)은 진행중만 — 최종합격·불합격·중도포기는 기록 탭에서
            if (!vm.IsInProgress) return false;

            if (!string.IsNullOrWhiteSpace(_search))
            {
                string q = _search.Trim();
                bool hit = (vm.Company?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (vm.Position?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
                if (!hit) return false;
            }
            if (_stageFilter is Stage s && vm.Stage != s) return false;
            return true;
        }

        // ── 통계(퍼널) ──
        private int _total, _active, _interviewing, _offers, _imminent, _thisWeek;
        public int TotalCount        { get => _total;        private set => SetProperty(ref _total, value); }
        public int ActiveCount       { get => _active;       private set => SetProperty(ref _active, value); }
        public int InterviewingCount { get => _interviewing; private set => SetProperty(ref _interviewing, value); }
        public int OfferCount        { get => _offers;       private set => SetProperty(ref _offers, value); }
        /// <summary>마감 임박(3일 이내) 진행중 건 수.</summary>
        public int ImminentCount     { get => _imminent;     private set => SetProperty(ref _imminent, value); }
        /// <summary>이번 주(앞으로 7일 이내) 서류 마감 건 수.</summary>
        public int ThisWeekDeadlineCount { get => _thisWeek; private set => SetProperty(ref _thisWeek, value); }

        private string _docPassRate, _offerRate;
        public string DocumentPassRate { get => _docPassRate; private set => SetProperty(ref _docPassRate, value); }
        public string OfferRate        { get => _offerRate;  private set => SetProperty(ref _offerRate, value); }

        private int ReachedCount(params Stage[] stages)
        {
            var set = new HashSet<Stage>(stages);
            return Applications.Count(vm =>
                set.Contains(vm.Stage) || vm.Model.StageLogs.Any(l => set.Contains(l.Stage)));
        }

        public void RefreshStats()
        {
            TotalCount        = Applications.Count;
            ActiveCount       = Applications.Count(a => a.IsInProgress);
            InterviewingCount = Applications.Count(a => a.Stage is Stage.FirstInterview or Stage.SecondInterview);
            OfferCount        = Applications.Count(a => a.Stage == Stage.Offer);
            ImminentCount     = Applications.Count(a => a.IsInProgress && a.Dday is int d && d >= 0 && d <= 3);
            ThisWeekDeadlineCount = Applications.Count(a => a.IsInProgress
                && a.Deadline is DateTime dl
                && (dl.Date - DateTime.Today).TotalDays >= 0
                && (dl.Date - DateTime.Today).TotalDays <= 7);

            int applied = ReachedCount(Stage.Applied, Stage.DocumentPassed,
                Stage.FirstInterview, Stage.SecondInterview, Stage.Negotiation, Stage.Offer);
            int docPassed = ReachedCount(Stage.DocumentPassed,
                Stage.FirstInterview, Stage.SecondInterview, Stage.Negotiation, Stage.Offer);
            int offered = ReachedCount(Stage.Offer);

            DocumentPassRate = applied == 0 ? "—" : $"{docPassed * 100.0 / applied:0}%";
            OfferRate        = applied == 0 ? "—" : $"{offered  * 100.0 / applied:0}%";
        }

        // ── 커맨드 ──
        public RelayCommand AddCommand     { get; }
        public RelayCommand DeleteCommand  { get; }
        public RelayCommand SaveCommand    { get; }
        public RelayCommand AdvanceCommand { get; }
        public RelayCommand RetreatCommand { get; }
        public RelayCommand OpenUrlCommand { get; }
        /// <summary>목록 행의 🔗 버튼 — 해당 건의 공고 URL을 연다.</summary>
        public RelayCommand OpenRowUrlCommand { get; }
        // 최종 결과 입력 (확인 후 기록으로 이동)
        public RelayCommand OfferResultCommand { get; }
        public RelayCommand RejectResultCommand { get; }
        public RelayCommand WithdrawResultCommand { get; }

        private void AddApplication()
        {
            var model = new Models.Application
            {
                Company = "새 지원", Position = "직무",
                Stage = Stage.Interested, AddedDate = DateTime.Today
            };
            _data.Add(model);
            var vm = new ApplicationViewModel(model);
            Applications.Insert(0, vm);
            SelectedApplication = vm;
            RefreshStats();
        }

        private void DeleteSelected()
        {
            if (SelectedApplication == null) return;
            _data.Delete(SelectedApplication.Model);
            Applications.Remove(SelectedApplication);
            SelectedApplication = null;
            RefreshStats();
        }

        /// <summary>단계 이동(+1: 다음, -1: 이전) + 이력 로그.</summary>
        private void MoveStage(int dir)
        {
            var vm = SelectedApplication;
            if (vm == null || vm.IsClosed) return;

            int idx = Array.IndexOf(Flow, vm.Stage);
            if (idx < 0) return;
            int target = idx + dir;
            if (target < 0 || target >= Flow.Length) return;   // 처우협의 이후는 '다음 단계' 없음 → 결과 버튼 사용

            Stage next = Flow[target];
            vm.Stage = next;
            _data.Save();          // 날짜는 자동 기록하지 않음 (단계 클릭→캘린더로만 입력)
            vm.RefreshDday();
            RefreshStats();
            ApplicationsView.Refresh();
            BuildSteps();
        }

        /// <summary>최종 결과(최종합격/불합격/중도포기) 입력 — 확인 후 기록으로 이동.</summary>
        private void EnterResult(Stage result)
        {
            var vm = SelectedApplication;
            if (vm == null) return;

            if (!Views.ConfirmDialog.Show(
                    "최종 결과를 입력하면 기록 화면으로 이동됩니다.\n입력하시겠습니까?",
                    "최종 결과 입력")) return;

            vm.Stage = result;
            _data.LogStage(vm.Model, result);
            _data.Save();
            RefreshStats();
            ApplicationsView.Refresh();
            SelectedApplication = null;
        }

        /// <summary>지원했던 원본 공고 사이트를 기본 브라우저로 연다.</summary>
        private void OpenUrl() => OpenUrlFor(SelectedApplication);

        private void OpenUrlFor(ApplicationViewModel vm)
        {
            var url = vm?.JobUrl;
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.StartsWith("http")) url = "https://" + url;
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch { /* 잘못된 URL은 조용히 무시 */ }
        }

        public void Save()
        {
            _data.Save();
            RefreshStats();
            ApplicationsView.Refresh();
            Views.ConfirmDialog.Info("저장되었습니다.", "저장 완료");
        }
    }

    /// <summary>진행 단계 stepper 한 칸 (날짜·편집 상태 포함).</summary>
    public class StepItem : ViewModelBase
    {
        public Stage Stage { get; set; }
        public string Label { get; set; }
        public bool IsLast { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsDone { get; set; }

        public System.Action<StepItem, System.DateTime?> DateChanged;   // 부모가 주입

        private System.DateTime? _passedDate;
        public System.DateTime? PassedDate
        {
            get => _passedDate;
            set { if (SetProperty(ref _passedDate, value)) DateChanged?.Invoke(this, value); }
        }

        private bool _isDateEditing;
        public bool IsDateEditing { get => _isDateEditing; set => SetProperty(ref _isDateEditing, value); }
    }
}
