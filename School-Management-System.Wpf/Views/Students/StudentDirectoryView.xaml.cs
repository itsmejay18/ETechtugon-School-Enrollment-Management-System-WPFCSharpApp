using System.Windows.Controls;
using System.Windows.Input;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.Views.Students
{
    public partial class StudentDirectoryView : UserControl
    {
        public StudentDirectoryView()
        {
            InitializeComponent();
        }

        private void RecordsGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
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
    }
}
