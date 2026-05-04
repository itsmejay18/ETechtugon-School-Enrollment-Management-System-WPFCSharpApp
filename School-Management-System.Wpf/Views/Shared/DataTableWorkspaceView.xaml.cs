using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.Views.Shared
{
    public partial class DataTableWorkspaceView : UserControl
    {
        public DataTableWorkspaceView()
        {
            InitializeComponent();
        }

        private void RecordsGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = WpfUiDataHelper.ToFriendlyLabel(e.PropertyName);
        }

        private void RecordsGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FindAncestor<DataGridRow>(e.OriginalSource as System.Windows.DependencyObject) == null)
            {
                return;
            }

            OpenSelectedRecord();
        }

        private void OpenSelectedRecord()
        {
            var viewModel = DataContext as DataTableWorkspaceViewModel;
            if (viewModel == null || viewModel.OpenDetailsCommand == null)
            {
                return;
            }

            if (viewModel.OpenDetailsCommand.CanExecute(null))
            {
                viewModel.OpenDetailsCommand.Execute(null);
            }
        }

        private static T FindAncestor<T>(System.Windows.DependencyObject start) where T : System.Windows.DependencyObject
        {
            var current = start;
            while (current != null)
            {
                var typed = current as T;
                if (typed != null)
                {
                    return typed;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
