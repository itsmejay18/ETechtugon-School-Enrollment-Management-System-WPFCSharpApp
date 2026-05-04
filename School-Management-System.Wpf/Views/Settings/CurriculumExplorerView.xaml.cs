using System.Windows.Controls;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.Views.Settings
{
    public partial class CurriculumExplorerView : UserControl
    {
        public CurriculumExplorerView()
        {
            InitializeComponent();
        }

        private void CurriculumSubjectsGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = WpfUiDataHelper.ToFriendlyLabel(e.PropertyName);
        }
    }
}
