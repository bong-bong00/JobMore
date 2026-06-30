using System;
using System.Linq;
using System.Windows;
using JobMore.Models;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>설정 화면 — 회원 정보 수정 + 로그아웃 + 회원 탈퇴.</summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;
        private readonly User _user;

        /// <summary>로그아웃 콜백 (셸이 로그인 화면으로).</summary>
        public Action LoggedOut { get; set; }
        /// <summary>회원 탈퇴 완료 콜백 (셸이 회원가입 화면으로).</summary>
        public Action Withdrawn { get; set; }

        public SettingsViewModel()
        {
            _user = _data.GetUser() ?? new User();
            SaveCommand = new RelayCommand(_ => Save());
            LogoutCommand = new RelayCommand(_ => Logout());
            WithdrawCommand = new RelayCommand(_ => Withdraw());
            SelectJobCommand = new RelayCommand(p => SelectJob(p as JobResult));
            RunJobSearch();
        }

        // 희망직무 검색
        public System.Collections.ObjectModel.ObservableCollection<JobResult> JobResults { get; } = new();
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
        public RelayCommand SelectJobCommand { get; }
        private void SelectJob(JobResult r)
        {
            if (r == null) return;
            var parts = (_user.DesiredJob ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
            if (parts.Contains(r.Name)) return;
            parts.Add(r.Name);
            DesiredJob = string.Join(", ", parts);
            JobSearchText = string.Empty;
        }

        public string Name
        {
            get => _user.Name;
            set { _user.Name = value; OnPropertyChanged(); }
        }
        public int Age
        {
            get => _user.Age;
            set { _user.Age = value; OnPropertyChanged(); }
        }
        public string Email
        {
            get => _user.Email;
            set { _user.Email = value; OnPropertyChanged(); }
        }
        public string Phone
        {
            get => _user.Phone;
            set { _user.Phone = value; OnPropertyChanged(); }
        }
        public string DesiredJob
        {
            get => _user.DesiredJob;
            set { _user.DesiredJob = value; OnPropertyChanged(); }
        }
        public string JoinedAt => _user.CreatedAt.ToString("yyyy-MM-dd");

        // 나이 드롭다운용
        public System.Collections.Generic.IEnumerable<int> AgeOptions { get; } =
            System.Linq.Enumerable.Range(15, 56);

        public RelayCommand SaveCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand WithdrawCommand { get; }

        private void Save()
        {
            _data.Save();
            Views.ConfirmDialog.Info("회원 정보가 저장되었습니다.", "저장 완료");
        }

        /// <summary>비밀번호 변경 (현재 비번 확인 후 새 비번으로).</summary>
        public void ChangePassword(string current, string next, string confirm)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                Views.ConfirmDialog.Info("새 비밀번호를 입력하세요.", "비밀번호 변경");
                return;
            }
            if (current != _user.Password)
            {
                Views.ConfirmDialog.Info("현재 비밀번호가 일치하지 않습니다.", "비밀번호 변경");
                return;
            }
            if (next != confirm)
            {
                Views.ConfirmDialog.Info("새 비밀번호가 서로 일치하지 않습니다.", "비밀번호 변경");
                return;
            }
            _user.Password = next;
            _data.Save();
            Views.ConfirmDialog.Info("비밀번호가 변경되었습니다.", "완료");
        }

        private void Logout()
        {
            if (Views.ConfirmDialog.Show("로그아웃 하시겠습니까?", "로그아웃"))
                LoggedOut?.Invoke();
        }

        private void Withdraw()
        {
            if (Views.ConfirmDialog.Show(
                "정말 탈퇴하시겠습니까?\n회원 정보와 작성한 자소서·자격증이 모두 삭제됩니다.\n(지원 현황은 유지됩니다.)",
                "회원 탈퇴"))
            {
                _data.DeleteUserAndData();
                Withdrawn?.Invoke();
            }
        }
    }
}
