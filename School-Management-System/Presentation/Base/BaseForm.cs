using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Base
{
    public class BaseForm : Form
    {
        protected BaseForm()
        {
            ThemeManager.ApplyBaseForm(this);
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.CenterScreen;

            try
            {
                var appIcon = BrandAssets.CreateAppIcon();
                if (appIcon != null)
                {
                    Icon = appIcon;
                }
            }
            catch
            {
                // Branding assets are optional at runtime.
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e == null || e.Control == null || IsInDesigner) return;
            ThemeManager.ApplyPaletteToControlTree(e.Control);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (IsInDesigner) return;
            ThemeManager.ApplyPaletteToControlTree(this);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if (IsInDesigner)
            {
                base.ScaleControl(factor, specified);
            }
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
