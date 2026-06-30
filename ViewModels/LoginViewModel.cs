using System;
using System.Windows;
using JobMore.Services;

namespace JobMore.ViewModels
{
    /// <summary>로그인 화면. 성공하면 메인으로, 하단 '회원가입'으로 가입 화면 전환.</summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly DataService _data = DataService.Instance;

        /// <summary>로그인 성공 콜백 (셸이 메인으로 전환).</summary>
        public Action LoggedIn { get; set; }
        /// <summary>회원가입 화면으로 전환 콜백.</summary>
        public Action GoSignUp { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(_ => Login());
            SignUpCommand = new RelayCommand(_ => GoSignUp?.Invoke());

            // 편의: 가입된 이메일을 미리 채워줌 (단일 사용자 로컬 앱)
            var u = _data.GetUser();
            if (u != null) _email = u.Email;
        }

        private string _email = string.Empty;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public RelayCommand LoginCommand { get; }
        public RelayCommand SignUpCommand { get; }

        private void Login()
        {
            if (_data.ValidateLogin(Email, Password))
            {
                LoggedIn?.Invoke();
            }
            else
            {
                Views.ConfirmDialog.Info("아이디 또는 비밀번호가 올바르지 않습니다.", "로그인 실패");
            }
        }
    }
}
