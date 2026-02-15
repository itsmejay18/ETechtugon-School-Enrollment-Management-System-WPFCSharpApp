using System.Windows.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Base
{
    public class BaseUserControl : UserControl
    {
        protected BaseUserControl()
        {
            Font = ThemeFonts.Label;
            BackColor = ThemeColors.Background;
            ForeColor = ThemeColors.Text;
            Dock = DockStyle.Fill;
        }
    }
}

