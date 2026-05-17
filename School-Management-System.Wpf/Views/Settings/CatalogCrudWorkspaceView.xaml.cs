using System.Windows.Controls;
using System.Windows.Input;
using School_Management_System.Wpf.ViewModels.Settings;

namespace School_Management_System.Wpf.Views.Settings
{
    public partial class CatalogCrudWorkspaceView : UserControl
    {
        public CatalogCrudWorkspaceView()
        {
            InitializeComponent();
        }

        private void RecordsGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as CatalogCrudWorkspaceViewModel;
            if (vm != null && vm.OpenDetailsCommand.CanExecute(null))
            {
                vm.OpenDetailsCommand.Execute(null);
            }
        }
    }
}
