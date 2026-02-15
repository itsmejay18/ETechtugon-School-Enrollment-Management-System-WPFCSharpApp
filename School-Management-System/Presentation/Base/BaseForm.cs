using System;
using System.Windows.Forms;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Base
{
    public class BaseForm : Form
    {
        protected BaseForm()
        {
            ThemeManager.ApplyBaseForm(this);
            StartPosition = FormStartPosition.CenterScreen;
        }

        protected void ShowInfo(string message, string title = null)
        {
            ThemedMessageBox.ShowInfo(this, message, title);
        }

        protected void ShowError(string message, string title = null)
        {
            ThemedMessageBox.ShowError(this, message, title);
        }

        protected bool Confirm(string message, string title = null)
        {
            return ThemedMessageBox.ShowConfirm(this, message, title) == DialogResult.OK;
        }
    }
}

