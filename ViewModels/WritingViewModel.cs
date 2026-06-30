using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JobMore.Models;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>작성 화면 — 자소서 작성/복사/AI생성 + 자격증·학력·대외활동(삽입/AI포함 토글).</summary>
    public class WritingViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;

        public ObservableCollection<CoverLetter> CoverLetters { get; } = new();
        public ObservableCollection<CertificateResult> CertResults { get; } = new();

        // 내 항목 (체크=AI 포함, 글자 클릭=자소서에 삽입)
        public ObservableCollection<SelectableItem> CertItems { get; } = new();
        public ObservableCollection<SelectableItem> EduItems { get; } = new();
        public ObservableCollection<SelectableItem> ActItems { get; } = new();
        public ObservableCollection<SelectableItem> CareerItems { get; } = new();
        public ObservableCollection<SelectableItem> DesiredJobItems { get; } = new();

        public WritingViewModel()
        {
            foreach (var c in _data.GetCoverLetters()) CoverLetters.Add(c);
            ReloadItems();

            AddLetterCommand    = new RelayCommand(_ => AddLetter());
            DeleteLetterCommand = new RelayCommand(_ => DeleteLetter(), _ => SelectedLetter != null);
            SaveLetterCommand   = new RelayCommand(_ => SaveLetter(), _ => SelectedLetter != null);
            CopyLetterCommand   = new RelayCommand(_ => CopyLetter(), _ => SelectedLetter != null);
            GenerateCommand     = new RelayCommand(_ => Generate(), _ => SelectedLetter != null && !IsGenerating);

            AddCertCommand   = new RelayCommand(p => AddCertFromResult(p as CertificateResult));
            DeleteItemCommand = new RelayCommand(p => DeleteItem(p as SelectableItem));

            AddEducationCommand = new RelayCommand(_ => AddEducation());
            AddActivityCommand  = new RelayCommand(_ => AddActivity());
            AddCareerCommand    = new RelayCommand(_ => AddCareer());
            AddJobCommand       = new RelayCommand(p => AddJobFromResult(p as JobResult));
            AddJobManualCommand = new RelayCommand(_ => AddJobManual());

            RunCertSearch();
        }

        private void ReloadItems()
        {
            CertItems.Clear();
            foreach (var c in _data.GetCertificates())
                CertItems.Add(new SelectableItem(c, c.Name,
                    string.IsNullOrWhiteSpace(c.Number) ? c.Issuer : $"{c.Issuer} · {c.Number}",
                    c.Name, $"자격증: {c.Name}"));

            EduItems.Clear();
            foreach (var e in _data.GetEducations())
                EduItems.Add(new SelectableItem(e, $"{e.School} {e.Major}", e.Period,
                    $"{e.School} {e.Major}", $"학력: {e.School} {e.Major} ({e.Period}) {e.Note}".Trim()));

            ActItems.Clear();
            foreach (var a in _data.GetActivities())
                ActItems.Add(new SelectableItem(a, a.Title,
                    string.IsNullOrWhiteSpace(a.Organization) ? a.Period : $"{a.Organization} · {a.Period}",
                    a.Title, $"대외활동: {a.Title} ({a.Organization}) - {a.Description}".Trim()));

            CareerItems.Clear();
            foreach (var c in _data.GetCareers())
                CareerItems.Add(new SelectableItem(c, $"{c.Company} {c.Role}",
                    c.Period, $"{c.Company} {c.Role}",
                    $"경력: {c.Company} {c.Role} ({c.Period}) {c.Description}".Trim()));

            DesiredJobItems.Clear();
            foreach (var d in _data.GetDesiredJobs())
                DesiredJobItems.Add(new SelectableItem(d, d.Name,
                    d.Category, d.Name, $"희망 직무: {d.Name}"));
        }

        // ───── 자소서 ─────
        private CoverLetter _selectedLetter;
        public CoverLetter SelectedLetter
        {
            get => _selectedLetter;
            set
            {
                if (SetProperty(ref _selectedLetter, value))
                {
                    OnPropertyChanged(nameof(LetterTitle));
                    OnPropertyChanged(nameof(LetterQuestion));
                    OnPropertyChanged(nameof(LetterContent));
                    OnPropertyChanged(nameof(LetterCharInfo));
                }
            }
        }

        public string LetterTitle
        {
            get => SelectedLetter?.Title ?? string.Empty;
            set { if (SelectedLetter != null) { SelectedLetter.Title = value; OnPropertyChanged(); } }
        }
        public string LetterQuestion
        {
            get => SelectedLetter?.Question ?? string.Empty;
            set { if (SelectedLetter != null) { SelectedLetter.Question = value; OnPropertyChanged(); } }
        }
        public string LetterContent
        {
            get => SelectedLetter?.Content ?? string.Empty;
            set
            {
                if (SelectedLetter != null)
                {
                    SelectedLetter.Content = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LetterCharInfo));
                }
            }
        }
        public string LetterCharInfo =>
            SelectedLetter == null ? "" : $"{SelectedLetter.CharCount}자 (공백 포함)";

        private bool _isGenerating;
        public bool IsGenerating
        {
            get => _isGenerating;
            set { if (SetProperty(ref _isGenerating, value)) OnPropertyChanged(nameof(GenerateLabel)); }
        }
        public string GenerateLabel => IsGenerating ? "생성 중…" : "🤖 AI 자소서 생성";

        public RelayCommand AddLetterCommand    { get; }
        public RelayCommand DeleteLetterCommand { get; }
        public RelayCommand SaveLetterCommand   { get; }
        public RelayCommand CopyLetterCommand   { get; }
        public RelayCommand GenerateCommand     { get; }

        private void AddLetter()
        {
            var c = new CoverLetter { Title = "새 자소서", Question = "", Content = "", UpdatedAt = DateTime.Now };
            _data.AddCoverLetter(c);
            CoverLetters.Insert(0, c);
            SelectedLetter = c;
        }

        private void DeleteLetter()
        {
            if (SelectedLetter == null) return;
            _data.DeleteCoverLetter(SelectedLetter);
            CoverLetters.Remove(SelectedLetter);
            SelectedLetter = null;
        }

        private void SaveLetter()
        {
            if (SelectedLetter == null) return;
            SelectedLetter.UpdatedAt = DateTime.Now;
            _data.Save();
            var sel = SelectedLetter;
            var ordered = CoverLetters.OrderByDescending(c => c.UpdatedAt).ToList();
            CoverLetters.Clear();
            foreach (var c in ordered) CoverLetters.Add(c);
            SelectedLetter = sel;
        }

        private void CopyLetter()
        {
            if (SelectedLetter == null) return;
            try
            {
                Clipboard.SetText(SelectedLetter.Content ?? string.Empty);
                Views.ConfirmDialog.Info("자소서 내용이 복사되었습니다.\n지원 사이트에 붙여넣기(Ctrl+V) 하세요.", "복사 완료");
            }
            catch { }
        }

        /// <summary>켜둔 항목으로 AI(또는 폴백) 자소서 생성.</summary>
        private async void Generate()
        {
            if (SelectedLetter == null) return;

            var included = CertItems.Concat(EduItems).Concat(ActItems).Concat(CareerItems).Concat(DesiredJobItems)
                .Where(i => i.IsIncluded).Select(i => i.AiText).ToList();

            IsGenerating = true;
            try
            {
                string result = await AiService.GenerateCoverLetterAsync(
                    string.Empty, LetterQuestion, included);
                LetterContent = result;
            }
            catch
            {
                Views.ConfirmDialog.Info("생성 중 오류가 발생했습니다. 잠시 후 다시 시도하세요.", "오류");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        // ───── 자격증 검색 ─────
        private string _certSearch = string.Empty;
        public string CertSearchText
        {
            get => _certSearch;
            set { if (SetProperty(ref _certSearch, value)) RunCertSearch(); }
        }

        private void RunCertSearch()
        {
            CertResults.Clear();
            foreach (var (name, issuer) in CertificateCatalog.Search(CertSearchText))
                CertResults.Add(new CertificateResult { Name = name, Issuer = issuer });
        }

        public RelayCommand AddCertCommand { get; }
        public RelayCommand DeleteItemCommand { get; }
        public RelayCommand AddEducationCommand { get; }
        public RelayCommand AddActivityCommand { get; }
        public RelayCommand AddCareerCommand { get; }

        private void AddCertFromResult(CertificateResult r)
        {
            if (r == null) return;
            if (CertItems.Any(c => c.Title == r.Name)) return;
            var cert = new Certificate { Name = r.Name, Issuer = r.Issuer, AcquiredDate = DateTime.Today };
            _data.AddCertificate(cert);
            CertItems.Insert(0, new SelectableItem(cert, cert.Name, cert.Issuer, cert.Name, $"자격증: {cert.Name}"));
        }

        // ───── 희망 직무 (검색 + 수동) ─────
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
        public RelayCommand AddJobCommand { get; private set; }
        private void AddJobFromResult(JobResult r)
        {
            if (r == null) return;
            if (DesiredJobItems.Any(j => j.Title == r.Name)) return;
            var d = new Models.DesiredJob { Name = r.Name, Category = r.Category };
            _data.AddDesiredJob(d);
            DesiredJobItems.Insert(0, new SelectableItem(d, d.Name, d.Category, d.Name, $"희망 직무: {d.Name}"));
        }

        private string _jobManual = string.Empty;
        public string JobManual { get => _jobManual; set => SetProperty(ref _jobManual, value); }
        public RelayCommand AddJobManualCommand { get; private set; }
        private void AddJobManual()
        {
            var name = (JobManual ?? "").Trim();
            if (name.Length == 0) return;
            if (DesiredJobItems.Any(j => j.Title == name)) { JobManual = ""; return; }
            var d = new Models.DesiredJob { Name = name, Category = "직접 입력" };
            _data.AddDesiredJob(d);
            DesiredJobItems.Insert(0, new SelectableItem(d, d.Name, d.Category, d.Name, $"희망 직무: {d.Name}"));
            JobManual = "";
        }

        // 학력 입력
        private string _eduSchool = "", _eduMajor = "", _eduPeriod = "";
        public string EduSchool { get => _eduSchool; set => SetProperty(ref _eduSchool, value); }
        public string EduMajor  { get => _eduMajor;  set => SetProperty(ref _eduMajor, value); }
        public string EduPeriod { get => _eduPeriod; set => SetProperty(ref _eduPeriod, value); }
        private System.DateTime? _eduStart, _eduEnd;
        public System.DateTime? EduStart { get => _eduStart; set => SetProperty(ref _eduStart, value); }
        public System.DateTime? EduEnd   { get => _eduEnd;   set => SetProperty(ref _eduEnd, value); }

        /// <summary>시작/종료 → "2023.07.01 ~ 2025.02.28" (끝 비우면 "2023.07.01 ~").</summary>
        public static string ComposePeriod(System.DateTime? start, System.DateTime? end)
        {
            if (start == null && end == null) return "";
            string s = start?.ToString("yyyy.MM.dd") ?? "";
            string e = end?.ToString("yyyy.MM.dd") ?? "";
            return $"{s} ~ {e}".Trim();
        }

        private void AddEducation()
        {
            if (string.IsNullOrWhiteSpace(EduSchool)) return;
            var period = ComposePeriod(EduStart, EduEnd);
            var e = new Education { School = EduSchool.Trim(), Major = EduMajor.Trim(), Period = period };
            _data.AddEducation(e);
            EduItems.Insert(0, new SelectableItem(e, $"{e.School} {e.Major}", e.Period,
                $"{e.School} {e.Major}", $"학력: {e.School} {e.Major} ({e.Period})".Trim()));
            EduSchool = EduMajor = EduPeriod = "";
            EduStart = EduEnd = null;
        }

        // 대외활동 입력
        private string _actTitle = "", _actOrg = "", _actPeriod = "";
        public string ActTitle  { get => _actTitle;  set => SetProperty(ref _actTitle, value); }
        public string ActOrg    { get => _actOrg;    set => SetProperty(ref _actOrg, value); }
        public string ActPeriod { get => _actPeriod; set => SetProperty(ref _actPeriod, value); }
        private System.DateTime? _actStart, _actEnd;
        public System.DateTime? ActStart { get => _actStart; set => SetProperty(ref _actStart, value); }
        public System.DateTime? ActEnd   { get => _actEnd;   set => SetProperty(ref _actEnd, value); }

        private void AddActivity()
        {
            if (string.IsNullOrWhiteSpace(ActTitle)) return;
            var period = ComposePeriod(ActStart, ActEnd);
            var a = new Activity { Title = ActTitle.Trim(), Organization = ActOrg.Trim(), Period = period };
            _data.AddActivity(a);
            ActItems.Insert(0, new SelectableItem(a, a.Title,
                string.IsNullOrWhiteSpace(a.Organization) ? a.Period : $"{a.Organization} · {a.Period}",
                a.Title, $"대외활동: {a.Title} ({a.Organization})".Trim()));
            ActTitle = ActOrg = ActPeriod = "";
            ActStart = ActEnd = null;
        }

        // 경력 입력
        private string _carCompany = "", _carRole = "", _carPeriod = "";
        public string CareerCompany { get => _carCompany; set => SetProperty(ref _carCompany, value); }
        public string CareerRole    { get => _carRole;    set => SetProperty(ref _carRole, value); }
        public string CareerPeriod  { get => _carPeriod;  set => SetProperty(ref _carPeriod, value); }
        private System.DateTime? _carStart, _carEnd;
        public System.DateTime? CareerStart { get => _carStart; set => SetProperty(ref _carStart, value); }
        public System.DateTime? CareerEnd   { get => _carEnd;   set => SetProperty(ref _carEnd, value); }

        private void AddCareer()
        {
            if (string.IsNullOrWhiteSpace(CareerCompany)) return;
            var period = ComposePeriod(CareerStart, CareerEnd);
            var c = new Career { Company = CareerCompany.Trim(), Role = CareerRole.Trim(), Period = period };
            _data.AddCareer(c);
            CareerItems.Insert(0, new SelectableItem(c, $"{c.Company} {c.Role}", c.Period,
                $"{c.Company} {c.Role}", $"경력: {c.Company} {c.Role} ({c.Period})".Trim()));
            CareerCompany = CareerRole = CareerPeriod = "";
            CareerStart = CareerEnd = null;
        }

        private void DeleteItem(SelectableItem item)
        {
            if (item == null) return;
            switch (item.Model)
            {
                case Certificate c: _data.DeleteCertificate(c); CertItems.Remove(item);   break;
                case Education e:   _data.DeleteEducation(e);    EduItems.Remove(item);    break;
                case Activity a:    _data.DeleteActivity(a);     ActItems.Remove(item);    break;
                case Career cr:     _data.DeleteCareer(cr);      CareerItems.Remove(item); break;
                case Models.DesiredJob dj: _data.DeleteDesiredJob(dj); DesiredJobItems.Remove(item); break;
            }
        }

        // ───── 섹션 접기/펴기 ─────
        private bool _careerOpen = false, _certOpen = false, _eduOpen = false, _actOpen = false, _jobOpen = false;
        public bool IsCareerOpen { get => _careerOpen; set => SetProperty(ref _careerOpen, value); }
        public bool IsCertOpen { get => _certOpen; set => SetProperty(ref _certOpen, value); }
        public bool IsEduOpen  { get => _eduOpen;  set => SetProperty(ref _eduOpen, value); }
        public bool IsActOpen  { get => _actOpen;  set => SetProperty(ref _actOpen, value); }
        public bool IsJobOpen  { get => _jobOpen;  set => SetProperty(ref _jobOpen, value); }
    }

    /// <summary>자격증 검색 결과 한 줄.</summary>
    public class CertificateResult
    {
        public string Name { get; set; }
        public string Issuer { get; set; }
    }

    /// <summary>내 항목 한 줄 — 체크(AI 포함) + 글자클릭(삽입).</summary>
    public class SelectableItem : ViewModelBase
    {
        public SelectableItem(object model, string title, string subtitle, string insertText, string aiText)
        {
            Model = model; Title = title; Subtitle = subtitle; InsertText = insertText; AiText = aiText;
        }
        public object Model { get; }
        public string Title { get; }        // 표시 메인
        public string Subtitle { get; }     // 표시 서브(기관/기간)
        public string InsertText { get; }   // 글자 클릭 시 자소서에 삽입할 텍스트
        public string AiText { get; }        // AI 프롬프트에 넣을 텍스트

        private bool _included = true;       // 체크=AI 생성 때 포함
        public bool IsIncluded { get => _included; set => SetProperty(ref _included, value); }
    }
}
