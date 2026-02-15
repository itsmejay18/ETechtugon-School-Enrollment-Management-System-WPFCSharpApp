using System;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.Presentation.Helpers;

namespace School_Management_System.Presentation.Theming
{
    public static class ThemeManager
    {
        public static void ApplyBaseForm(Form form)
        {
            if (form == null) return;

            form.Font = ThemeFonts.Label;
            form.BackColor = ThemeColors.Background;
            form.ForeColor = ThemeColors.Text;
        }

        public static void StyleCardPanel(Panel panel)
        {
            if (panel == null) return;

            panel.BackColor = ThemeColors.CardBackground;
            panel.Padding = new Padding(18);
            UiHelper.ApplyRoundedCorners(panel, 6);
            UiHelper.EnableDoubleBuffering(panel);

            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using (var pen = new Pen(ThemeColors.Border))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
        }

        public static void StyleInput(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Font = ThemeFonts.Input;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void StyleComboBox(ComboBox comboBox)
        {
            if (comboBox == null) return;
            comboBox.Font = ThemeFonts.Input;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public static void StyleButtonPrimary(Button button)
        {
            StyleButton(button, ThemeColors.Secondary, Darken(ThemeColors.Secondary, 0.08f), Color.White);
        }

        public static void StyleButtonDanger(Button button)
        {
            StyleButton(button, ThemeColors.AccentDanger, Darken(ThemeColors.AccentDanger, 0.08f), Color.White);
        }

        public static void StyleButtonNeutral(Button button)
        {
            StyleButton(button, Color.White, ColorTranslator.FromHtml("#F3F4F6"), ThemeColors.Text);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = ThemeColors.Border;
        }

        public static void StyleSidebarButton(Button button)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = ThemeColors.SidebarBackground;
            button.ForeColor = Color.White;
            button.Font = ThemeFonts.Sidebar;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 12, 0);
            button.Height = 42;
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (s, e) => button.BackColor = ThemeColors.SidebarHover;
            button.MouseLeave += (s, e) => button.BackColor = ThemeColors.SidebarBackground;
        }

        public static void StyleDataGrid(DataGridView grid)
        {
            if (grid == null) return;

            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = ThemeFonts.SubHeader;
            grid.ColumnHeadersHeight = 36;
            grid.RowTemplate.Height = 32;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = ThemeColors.Text;
            grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#D6EAF8");
            grid.DefaultCellStyle.SelectionForeColor = ThemeColors.Text;
        }

        private static void StyleButton(Button button, Color backColor, Color hoverBackColor, Color foreColor)
        {
            if (button == null) return;

            button.Font = ThemeFonts.Button;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Height = 34;
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (s, e) => button.BackColor = hoverBackColor;
            button.MouseLeave += (s, e) => button.BackColor = backColor;
        }

        private static Color Darken(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            var r = (int)(color.R * (1f - amount));
            var g = (int)(color.G * (1f - amount));
            var b = (int)(color.B * (1f - amount));
            return Color.FromArgb(color.A, r, g, b);
        }
    }
}

