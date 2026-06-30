using System.Windows;
using System.Windows.Controls;

namespace JobMore.Views
{
    public partial class ApplicationsView : UserControl
    {
        public ApplicationsView()
        {
            InitializeComponent();
        }

        // 현재 단계가 화면에 보이도록 자동 스크롤
        private void Step_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ViewModels.StepItem s && s.IsCurrent)
                fe.BringIntoView();
        }

        // 클릭한 단계가 가려져 있으면 보이도록 스크롤
        private void Step_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
                fe.BringIntoView();
        }

        // 다음 일정 종류 드롭다운에서 '+ 새 항목 추가…' 선택 시 입력 팝업
        private void EventTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox cb)) return;
            if (cb.SelectedItem as string != ViewModels.ApplicationsViewModel.AddNewEventType) return;

            string prev = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as string : null;
            var owner = DataContext as ViewModels.ApplicationsViewModel;
            string name = InputDialog.Show("새 일정 종류", "추가할 일정 종류를 입력하세요");

            if (string.IsNullOrWhiteSpace(name))
            {
                if (cb.DataContext is ViewModels.ApplicationViewModel av) av.NextEventLabel = prev;
                return;
            }

            string added = owner?.AddEventType(name);
            if (cb.DataContext is ViewModels.ApplicationViewModel av2) av2.NextEventLabel = added ?? prev;
        }
    }
}
