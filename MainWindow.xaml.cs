using System.Windows;
using JobMore.Services;
using JobMore.Views;

namespace JobMore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 저장하지 않은 변경이 있으면 종료 전에 확인
            if (!DataService.Instance.HasPendingChanges()) return;

            bool save = ConfirmDialog.Show(
                "저장하지 않은 변경이 있습니다.\n저장하고 종료할까요?",
                "저장 확인");
            if (save)
                DataService.Instance.Save();
            // '아니오'면 저장하지 않고 그대로 종료
        }
    }
}
