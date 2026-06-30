using System.Windows;
using System.Windows.Controls;
using JobMore.ViewModels;

namespace JobMore.Views
{
    public partial class SignUpView : UserControl
    {
        public SignUpView()
        {
            InitializeComponent();
        }

        private void PwBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel vm)
                vm.Password = PwBox.Password;
        }
    }
}
