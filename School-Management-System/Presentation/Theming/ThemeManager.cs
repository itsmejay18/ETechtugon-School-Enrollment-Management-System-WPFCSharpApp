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

            var appIcon = BrandAssets.CreateAppIcon();
            if (appIcon != null)
            {
                form.Icon = appIcon;
            }
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

        public static void StyleCheckBox(CheckBox checkBox)
        {
            if (checkBox == null) return;

            checkBox.Font = ThemeFonts.Label;
            checkBox.ForeColor = ThemeColors.Text;
            checkBox.BackColor = Color.Transparent;
            checkBox.FlatStyle = FlatStyle.Standard;
        }

        public static void StyleLinkLabel(LinkLabel linkLabel)
        {
            if (linkLabel == null) return;

            linkLabel.Font = ThemeFonts.Label;
            linkLabel.LinkColor = ThemeColors.Secondary;
            linkLabel.ActiveLinkColor = Darken(ThemeColors.Secondary, 0.1f);
            linkLabel.VisitedLinkColor = ThemeColors.Secondary;
        }

        public static void StyleDatePicker(DateTimePicker dateTimePicker)
        {
            if (dateTimePicker == null) return;

            dateTimePicker.Font = ThemeFonts.Input;
            dateTimePicker.CalendarForeColor = ThemeColors.Text;
            dateTimePicker.CalendarMonthBackground = Color.White;
        }

        public static void StyleSidebarButton(Button button)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.SidebarHover;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.SidebarActive;
            button.BackColor = ThemeColors.SidebarBackground;
            button.UseVisualStyleBackColor = false;
            button.ForeColor = ThemeColors.SidebarTextMuted;
            button.Font = ThemeFonts.Sidebar;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.Padding = new Padding(12, 0, 12, 0);
            button.Height = 46;
            button.AutoEllipsis = true;
            button.Cursor = Cursors.Hand;
            button.TabStop = false;
            UiHelper.ApplyRoundedCorners(button, 6);
        }

        public static void SetSidebarButtonState(Button button, bool active)
        {
            if (button == null) return;
            button.BackColor = active ? ThemeColors.SidebarActive : ThemeColors.SidebarBackground;
            button.ForeColor = active ? Color.White : ThemeColors.SidebarTextMuted;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = active ? ThemeColors.SidebarActive : ThemeColors.SidebarHover;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.SidebarActive;
        }

        public static void ApplyPaletteToControlTree(Control root)
        {
            if (root == null) return;

            ApplyPaletteToControl(root);
            foreach (Control child in root.Controls)
            {
                ApplyPaletteToControlTree(child);
            }
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

        private static void ApplyPaletteToControl(Control control)
        {
            if (control == null) return;

            var textBox = control as TextBox;
            if (textBox != null)
            {
                StyleInput(textBox);
                return;
            }

            var numericUpDown = control as NumericUpDown;
            if (numericUpDown != null)
            {
                StyleInput(numericUpDown);
                return;
            }

            var comboBox = control as ComboBox;
            if (comboBox != null)
            {
                StyleComboBox(comboBox);
                return;
            }

            var checkBox = control as CheckBox;
            if (checkBox != null)
            {
                StyleCheckBox(checkBox);
                return;
            }

            var linkLabel = control as LinkLabel;
            if (linkLabel != null)
            {
                StyleLinkLabel(linkLabel);
                return;
            }

            var datePicker = control as DateTimePicker;
            if (datePicker != null)
            {
                StyleDatePicker(datePicker);
                return;
            }

            var groupBox = control as GroupBox;
            if (groupBox != null)
            {
                StyleGroupBox(groupBox);
                return;
            }

            var dataGrid = control as DataGridView;
            if (dataGrid != null)
            {
                StyleDataGrid(dataGrid);
                return;
            }

            var button = control as Button;
            if (button != null)
            {
                if (button.FlatStyle == FlatStyle.Standard || button.FlatStyle == FlatStyle.System)
                {
                    StyleButtonNeutral(button);
                }
                return;
            }

            var label = control as Label;
            if (label != null && IsDefaultLikeFont(label.Font))
            {
                label.Font = ThemeFonts.Label;
            }
        }

        private static bool IsDefaultLikeFont(Font font)
        {
            if (font == null) return true;

            var defaultFont = Control.DefaultFont;
            var sizeDiff = Math.Abs(font.SizeInPoints - defaultFont.SizeInPoints);
            return string.Equals(font.FontFamily.Name, defaultFont.FontFamily.Name, StringComparison.OrdinalIgnoreCase) &&
                   sizeDiff < 0.2f &&
                   font.Style == defaultFont.Style;
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
