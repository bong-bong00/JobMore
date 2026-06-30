using System.Windows;
using System.Windows.Controls;
using JobMore.ViewModels;

namespace JobMore.Views
{
    public partial class WritingView : UserControl
    {
        public WritingView()
        {
            InitializeComponent();
        }

        // 항목 글자 클릭 → 자소서 내용 커서 위치에 삽입
        private void InsertItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not WritingViewModel vm || vm.SelectedLetter == null)
            {
                ConfirmDialog.Info("먼저 자소서를 선택하거나 새로 만들어 주세요.", "안내");
                return;
            }
            if (sender is Button b && b.DataContext is SelectableItem item)
            {
                var box = ContentBox;
                int i = box.CaretIndex;
                string ins = item.InsertText ?? string.Empty;
                string cur = box.Text ?? string.Empty;
                box.Text = cur.Insert(i, ins);
                box.CaretIndex = i + ins.Length;
                box.Focus();
            }
        }
    }
}
