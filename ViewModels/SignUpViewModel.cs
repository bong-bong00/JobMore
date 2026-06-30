using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JobMore.Models;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>회원가입 화면. 나이는 드롭다운 선택, 희망직무는 검색해서 선택.</summary>
    public class SignUpViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;

        /// <summary>가입 완료 후 메인으로 전환하는 콜백.</summary>
        public Action Completed { get; set; }
        /// <summary>로그인 화면으로 돌아가는 콜백.</summary>
        public Action GoLogin { get; set; }

        // 나이 선택지 (15~70) — "꾹 누르면 숫자가 주루룩" = 드롭다운
        public IEnumerable<int> AgeOptions { get; } = Enumerable.Range(15, 56);

        // 희망직무 검색 결과
        public ObservableCollection<JobResult> JobResults { get; } = new();

        // 선택된 희망직무들 (칩으로 표시, 최대 4개)
        public ObservableCollection<string> SelectedJobs { get; } = new();

        public SignUpViewModel()
        {
            SignUpCommand = new RelayCommand(_ => SignUp());
            GoLoginCommand = new RelayCommand(_ => GoLogin?.Invoke());
            SelectJobCommand = new RelayCommand(p => SelectJob(p as JobResult));
            RemoveJobCommand = new RelayCommand(p => RemoveJob(p as string));
            RunJobSearch();
        }

        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private int _age = 25;
        public int Age { get => _age; set => SetProperty(ref _age, value); }

        private string _email = string.Empty;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        // 직무 검색어
        private string _jobSearch = string.Empty;
        public string JobSearchText
        {
            get => _jobSearch;
            set { if (SetProperty(ref _jobSearch, value)) RunJobSearch(); }
        }

        private void RunJobSearch()
        {
            JobResults.Clear();
            foreach (var (name, cat) in JobCatalog.Search(JobSearchText))
                JobResults.Add(new JobResult { Name = name, Category = cat });
        }

        // 선택 안내 문구 (칩이 하나도 없을 때 표시)
        public bool HasNoJob => SelectedJobs.Count == 0;

        private void SelectJob(JobResult r)
        {
            if (r == null) return;
            if (SelectedJobs.Contains(r.Name)) return;
            if (SelectedJobs.Count >= 4)
            {
                Views.ConfirmDialog.Info("희망 직무는 최대 4개까지 선택할 수 있어요.", "안내");
                return;
            }
            SelectedJobs.Add(r.Name);
            OnPropertyChanged(nameof(HasNoJob));
        }

        private void RemoveJob(string job)
        {
            if (job == null) return;
            SelectedJobs.Remove(job);
            OnPropertyChanged(nameof(HasNoJob));
        }

        public RelayCommand SignUpCommand { get; }
        public RelayCommand GoLoginCommand { get; }
        public RelayCommand SelectJobCommand { get; }
        public RelayCommand RemoveJobCommand { get; }

        private void SignUp()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
            {
                Views.ConfirmDialog.Info("이름과 아이디는 필수입니다.", "입력 확인");
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                Views.ConfirmDialog.Info("비밀번호를 입력하세요.", "입력 확인");
                return;
            }

            _data.AddUser(new User
            {
                Name = Name.Trim(),
                Age = Age,
                Email = Email.Trim(),
                Password = Password,
                CreatedAt = DateTime.Now
            });

            Views.ConfirmDialog.Info("회원가입이 완료되었습니다.\n로그인 화면에서 로그인해 주세요.", "회원가입 완료");
            Completed?.Invoke();
        }
    }

    /// <summary>희망직무 검색 결과 한 줄.</summary>
    public class JobResult
    {
        public string Name { get; set; }
        public string Category { get; set; }
    }
}
