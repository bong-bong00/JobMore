using System.Windows;

namespace JobMore.Views
{
    public partial class ConfirmDialog : Window
    {
        private bool _result;

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        /// <summary>예/아니오 확인창. 예를 누르면 true.</summary>
        public static bool Show(string message, string title = "확인")
        {
            var d = new ConfirmDialog();
            d.TitleText.Text = title;
            d.MsgText.Text = message;
            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsLoaded) d.Owner = owner;
            else d.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            d.ShowDialog();
            return d._result;
        }

        /// <summary>확인만 있는 안내창.</summary>
        public static void Info(string message, string title = "안내")
        {
            var d = new ConfirmDialog();
            d.TitleText.Text = title;
            d.MsgText.Text = message;
            d.NoBtn.Visibility = Visibility.Collapsed;
            d.YesBtn.Content = "확인";
            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsLoaded) d.Owner = owner;
            else d.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            d.ShowDialog();
        }

        private void Yes_Click(object sender, RoutedEventArgs e) { _result = true; Close(); }
        private void No_Click(object sender, RoutedEventArgs e) { _result = false; Close(); }
    }
}
