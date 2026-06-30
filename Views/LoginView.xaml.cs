using System.Windows;
using System.Windows.Controls;
using JobMore.ViewModels;

namespace JobMore.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // PasswordBox는 보안상 바인딩이 막혀 있어, 변경 시 ViewModel로 직접 전달
        private void PwBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = PwBox.Password;
        }
    }
}
