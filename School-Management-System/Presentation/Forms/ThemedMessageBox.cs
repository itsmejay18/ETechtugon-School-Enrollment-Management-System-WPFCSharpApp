using System;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.Common;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Forms
{
    public sealed class ThemedMessageBox : Form
    {
        private readonly Label _lblTitle;
        private readonly Label _lblMessage;
        private readonly Panel _header;
        private readonly Panel _body;
        private readonly FlowLayoutPanel _buttons;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        private ThemedMessageBox(string title, string message, MessageBoxIcon icon, bool showCancel)
        {
            ThemeManager.ApplyBaseForm(this);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Width = 520;
            Height = 220;

            _header = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = ThemeColors.Primary, Padding = new Padding(16, 10, 16, 10) };
            _body = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.CardBackground, Padding = new Padding(16, 14, 16, 12) };
            _buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(16, 10, 16, 10) };

            _lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = ThemeFonts.SubHeader,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = string.IsNullOrWhiteSpace(title) ? AppConstants.AppTitle : title
            };

            _lblMessage = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = ThemeColors.Text,
                Font = ThemeFonts.Label,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = message ?? string.Empty
            };

            _btnOk = new Button { Text = "OK", Width = 96 };
            ThemeManager.StyleButtonPrimary(_btnOk);
            _btnOk.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            _btnCancel = new Button { Text = "Cancel", Width = 96, Visible = showCancel };
            ThemeManager.StyleButtonNeutral(_btnCancel);
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            _buttons.Controls.Add(_btnOk);
            _buttons.Controls.Add(_btnCancel);

            _header.Controls.Add(_lblTitle);
            _body.Controls.Add(_lblMessage);

            Controls.Add(_body);
            Controls.Add(_buttons);
            Controls.Add(_header);

            UiHelper.ApplyRoundedCorners(_body, 6);
            UiHelper.EnableDoubleBuffering(this);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        public static void ShowInfo(IWin32Window owner, string message, string title = null)
        {
            using (var f = new ThemedMessageBox(title, message, MessageBoxIcon.Information, false))
            {
                f.ShowDialog(owner);
            }
        }

        public static void ShowError(IWin32Window owner, string message, string title = null)
        {
            using (var f = new ThemedMessageBox(title, message, MessageBoxIcon.Error, false))
            {
                f.ShowDialog(owner);
            }
        }

        public static DialogResult ShowConfirm(IWin32Window owner, string message, string title = null)
        {
            using (var f = new ThemedMessageBox(title, message, MessageBoxIcon.Question, true))
            {
                f._btnOk.Text = "Yes";
                f._btnCancel.Text = "No";
                ThemeManager.StyleButtonDanger(f._btnCancel);
                ThemeManager.StyleButtonPrimary(f._btnOk);
                return f.ShowDialog(owner);
            }
        }
    }
}

