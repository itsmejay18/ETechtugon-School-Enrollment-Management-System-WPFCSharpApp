using System.ComponentModel;
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

        protected bool IsInDesigner
        {
            get
            {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                       DesignMode ||
                       (Site != null && Site.DesignMode);
            }
        }
    }
}
