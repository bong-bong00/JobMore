using System.Windows;
using System.Windows.Controls;
using JobMore.ViewModels;

namespace JobMore.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.ChangePassword(CurrentPw.Password, NewPw.Password, ConfirmPw.Password);
                CurrentPw.Clear();
                NewPw.Clear();
                ConfirmPw.Clear();
            }
        }
    }
}
