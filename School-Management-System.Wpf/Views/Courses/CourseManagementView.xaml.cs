using System.Windows.Controls;
using System.Windows.Input;
using School_Management_System.Wpf.ViewModels.Courses;

namespace School_Management_System.Wpf.Views.Courses
{
    public partial class CourseManagementView : UserControl
    {
        public CourseManagementView()
        {
            InitializeComponent();
        }

        private void CoursesGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as CourseManagementViewModel;
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
