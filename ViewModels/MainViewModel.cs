using System.Collections.ObjectModel;
using System.Linq;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>
    /// 앱 셸 — 로그인/회원가입 → 메인(사이드바 멀티뷰) 전환과 내비게이션 관리.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<NavItem> NavItems { get; } = new()
        {
            new NavItem("apps",     "지원현황", "\uE8A5"),
            new NavItem("calendar", "일정",     "\uE787"),
            new NavItem("record",   "기록",     "\uE81C"),
            new NavItem("writing",  "작성",     "\uE70F"),
            new NavItem("settings", "설정",     "\uE713"),
        };

        public MainViewModel()
        {
            NavCommand = new RelayCommand(p => Navigate(p as string));

            // 항상 로그인 화면부터 시작 (회원이 없으면 로그인 화면의 '회원가입'으로 이동)
            ShowLogin();
        }

        // ── 인증 화면 전환 ──
        private void ShowLogin()
        {
            var login = new LoginViewModel
            {
                LoggedIn = EnterApp,
                GoSignUp = ShowSignUp
            };
            ShowChrome = false;
            CurrentViewModel = login;
        }

        private void ShowSignUp()
        {
            var signup = new SignUpViewModel
            {
                Completed = ShowLogin,   // 가입 완료 → 로그인 화면으로
                GoLogin = ShowLogin
            };
            ShowChrome = false;
            CurrentViewModel = signup;
        }

        /// <summary>로그인/가입 성공 → 메인 진입.</summary>
        private void EnterApp()
        {
            ShowChrome = true;
            Navigate("apps");
            OnPropertyChanged(nameof(UserName));
        }

        // ── 크롬(사이드바) 표시 ──
        private bool _showChrome;
        public bool ShowChrome { get => _showChrome; set => SetProperty(ref _showChrome, value); }

        private ViewModelBase _current;
        public ViewModelBase CurrentViewModel
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        private string _currentKey;
        public string CurrentKey
        {
            get => _currentKey;
            set
            {
                if (SetProperty(ref _currentKey, value))
                    foreach (var n in NavItems) n.IsActive = (n.Key == value);
            }
        }

        public string UserName
        {
            get
            {
                var u = DataService.Instance.GetUser();
                return u == null ? "" : u.Name;
            }
        }

        public RelayCommand NavCommand { get; }

        private void Navigate(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            switch (key)
            {
                case "apps":
                    CurrentViewModel = new ApplicationsViewModel();
                    break;
                case "calendar":
                    CurrentViewModel = new CalendarViewModel();
                    break;
                case "record":
                    CurrentViewModel = new RecordViewModel();
                    break;
                case "writing":
                    CurrentViewModel = new WritingViewModel();
                    break;
                case "settings":
                    var s = new SettingsViewModel
                    {
                        LoggedOut = ShowLogin,
                        Withdrawn = ShowSignUp
                    };
                    CurrentViewModel = s;
                    break;
                default:
                    return;
            }
            CurrentKey = key;
            OnPropertyChanged(nameof(UserName));
        }
    }

    /// <summary>사이드바 항목.</summary>
    public class NavItem : ViewModelBase
    {
        public NavItem(string key, string label, string glyph)
        {
            Key = key; Label = label; Glyph = glyph;
        }
        public string Key { get; }
        public string Label { get; }
        public string Glyph { get; }

        private bool _isActive;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    }
}
