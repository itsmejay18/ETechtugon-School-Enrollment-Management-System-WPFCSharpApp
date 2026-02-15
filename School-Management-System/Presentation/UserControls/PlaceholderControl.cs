using System.Drawing;
using System.Windows.Forms;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class PlaceholderControl : BaseUserControl
    {
        public PlaceholderControl(string message)
        {
            BackColor = ThemeColors.Background;

            var card = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = ThemeColors.CardBackground, Padding = new Padding(18), Margin = new Padding(0, 0, 0, 12) };
            ThemeManager.StyleCardPanel(card);

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Text = message ?? string.Empty,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(lbl);
            Controls.Add(card);
        }
    }
}

