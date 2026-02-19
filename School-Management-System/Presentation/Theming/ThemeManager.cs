using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            UiHelper.EnableDoubleBuffering(form);
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
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var shadowPen = new Pen(Color.FromArgb(20, 44, 62, 80)))
                {
                    var shadowRect = rect;
                    shadowRect.Offset(0, 1);
                    e.Graphics.DrawRectangle(shadowPen, shadowRect);
                }

                using (var pen = new Pen(ThemeColors.Border))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
        }

        public static void StyleGroupBox(GroupBox groupBox)
        {
            if (groupBox == null) return;

            groupBox.Font = ThemeFonts.SubHeader;
            groupBox.ForeColor = ThemeColors.Text;
            groupBox.BackColor = ThemeColors.CardBackground;
        }

        public static void StyleInput(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Font = ThemeFonts.Input;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.ForeColor = ThemeColors.Text;
        }

        public static void StyleInput(NumericUpDown numericUpDown)
        {
            if (numericUpDown == null) return;
            numericUpDown.Font = ThemeFonts.Input;
            numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            numericUpDown.BackColor = Color.White;
            numericUpDown.ForeColor = ThemeColors.Text;
            numericUpDown.TextAlign = HorizontalAlignment.Right;
        }

        public static void StyleComboBox(ComboBox comboBox)
        {
            if (comboBox == null) return;
            comboBox.Font = ThemeFonts.Input;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = ThemeColors.Text;
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
            StyleButton(button, Color.White, ThemeColors.SurfaceAlt, ThemeColors.Text);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = ThemeColors.Border;
        }

        public static void StyleButtonGhost(Button button)
        {
            if (button == null) return;

            button.Font = ThemeFonts.Button;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.SurfaceAlt;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.SurfaceAlt;
            button.BackColor = Color.Transparent;
            button.ForeColor = ThemeColors.Secondary;
            button.Height = 34;
            button.Cursor = Cursors.Hand;
        }

        public static void StyleSidebarButton(Button button)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.Transparent;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.SidebarHover;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.SidebarActive;
            button.BackColor = ThemeColors.SidebarBackground;
            button.ForeColor = ThemeColors.SidebarTextMuted;
            button.Font = ThemeFonts.Sidebar;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.Padding = new Padding(12, 0, 12, 0);
            button.Height = 46;
            button.AutoEllipsis = true;
            button.Cursor = Cursors.Hand;
            UiHelper.ApplyRoundedCorners(button, 6);
        }

        public static void SetSidebarButtonState(Button button, bool active)
        {
            if (button == null) return;
            button.BackColor = active ? ThemeColors.SidebarActive : ThemeColors.SidebarBackground;
            button.ForeColor = active ? Color.White : ThemeColors.SidebarTextMuted;
            button.FlatAppearance.BorderColor = active
                ? Color.FromArgb(130, ThemeColors.Secondary)
                : Color.Transparent;
        }

        public static void StyleDataGrid(DataGridView grid)
        {
            if (grid == null) return;

            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = ThemeColors.Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = ThemeFonts.SubHeader;
            grid.ColumnHeadersHeight = 36;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
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
            grid.AlternatingRowsDefaultCellStyle.BackColor = ThemeColors.Surface;
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
            button.FlatAppearance.MouseOverBackColor = hoverBackColor;
            button.FlatAppearance.MouseDownBackColor = Darken(backColor, 0.12f);
            UiHelper.ApplyRoundedCorners(button, 4);
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
