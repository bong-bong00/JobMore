using System.Windows;
using System.Windows.Input;

namespace JobMore.Views
{
    public partial class InputDialog : Window
    {
        private bool _ok;

        public InputDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => { InputBox.Focus(); };
            InputBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) { _ok = true; Close(); } };
        }

        /// <summary>입력창. 확인 시 입력값, 취소 시 null 반환.</summary>
        public static string Show(string title, string hint = "")
        {
            var d = new InputDialog();
            d.TitleText.Text = title;
            d.MsgText.Text = hint;
            if (string.IsNullOrEmpty(hint)) d.MsgText.Visibility = Visibility.Collapsed;
            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsLoaded) d.Owner = owner;
            else d.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            d.ShowDialog();
            if (!d._ok) return null;
            var text = (d.InputBox.Text ?? "").Trim();
            return text.Length == 0 ? null : text;
        }

        private void Yes_Click(object sender, RoutedEventArgs e) { _ok = true; Close(); }
        private void No_Click(object sender, RoutedEventArgs e) { _ok = false; Close(); }
    }
}
