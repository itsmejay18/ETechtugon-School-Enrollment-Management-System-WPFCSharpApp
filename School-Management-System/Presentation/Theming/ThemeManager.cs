using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using School_Management_System.Presentation.Helpers;

namespace School_Management_System.Presentation.Theming
{
    public static class ThemeManager
    {
        private sealed class ButtonStyleMetadata
        {
            public Color EnabledBackColor { get; set; }
            public Color EnabledForeColor { get; set; }
            public Color EnabledBorderColor { get; set; }
            public int EnabledBorderSize { get; set; }
            public Color DisabledBackColor { get; set; }
            public Color DisabledForeColor { get; set; }
            public Color DisabledBorderColor { get; set; }
            public int DisabledBorderSize { get; set; }
        }

        private static readonly Dictionary<Button, ButtonStyleMetadata> ButtonStyles = new Dictionary<Button, ButtonStyleMetadata>();

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
            UiHelper.ApplyRoundedCorners(panel, 12);
            UiHelper.EnableDoubleBuffering(panel);
            panel.Paint -= CardPanelPaint;
            panel.Paint += CardPanelPaint;
        }

        public static void StyleGroupBox(GroupBox groupBox)
        {
            if (groupBox == null) return;

            groupBox.Font = ThemeFonts.SubHeader;
            groupBox.ForeColor = ThemeColors.Text;
            groupBox.BackColor = ThemeColors.CardBackground;
            groupBox.Padding = new Padding(
                Math.Max(groupBox.Padding.Left, 14),
                Math.Max(groupBox.Padding.Top, 30),
                Math.Max(groupBox.Padding.Right, 14),
                Math.Max(groupBox.Padding.Bottom, 14));
            groupBox.Paint -= GroupBoxPaint;
            groupBox.Paint += GroupBoxPaint;
        }

        public static void StyleInput(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Font = ThemeFonts.Input;
            textBox.AutoSize = false;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = textBox.ReadOnly ? ThemeColors.Surface : Color.White;
            textBox.ForeColor = ThemeColors.Text;
            if (!textBox.Multiline)
            {
                textBox.Height = Math.Max(textBox.Height, 30);
            }
        }

        public static void StyleInput(NumericUpDown numericUpDown)
        {
            if (numericUpDown == null) return;
            numericUpDown.Font = ThemeFonts.Input;
            numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            numericUpDown.BackColor = Color.White;
            numericUpDown.ForeColor = ThemeColors.Text;
            numericUpDown.TextAlign = HorizontalAlignment.Right;
            numericUpDown.Height = Math.Max(numericUpDown.Height, 30);
        }

        public static void StyleComboBox(ComboBox comboBox)
        {
            if (comboBox == null) return;
            comboBox.Font = ThemeFonts.Input;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.IntegralHeight = false;
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = ThemeColors.Text;
            comboBox.Height = Math.Max(comboBox.Height, 32);
        }

        public static void StyleButtonPrimary(Button button)
        {
            StyleButton(button, ThemeColors.Secondary, Darken(ThemeColors.Secondary, 0.08f), Color.White, 0, ThemeColors.Secondary);
        }

        public static void StyleButtonDanger(Button button)
        {
            StyleButton(button, ThemeColors.AccentDanger, Darken(ThemeColors.AccentDanger, 0.08f), Color.White, 0, ThemeColors.AccentDanger);
        }

        public static void StyleButtonNeutral(Button button)
        {
            StyleButton(button, Color.White, ThemeColors.SurfaceAlt, ThemeColors.Text, 1, ThemeColors.Border);
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
            dateTimePicker.Height = Math.Max(dateTimePicker.Height, 32);
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
            button.Padding = new Padding(16, 0, 16, 0);
            button.Height = 48;
            button.AutoEllipsis = true;
            button.Cursor = Cursors.Hand;
            button.TabStop = false;
            UiHelper.ApplyRoundedCorners(button, 10);
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
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = ThemeColors.Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = ThemeFonts.SubHeader;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeColors.Primary;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersHeight = 40;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 36;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = ThemeColors.Text;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#D6EAF8");
            grid.DefaultCellStyle.SelectionForeColor = ThemeColors.Text;
            grid.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F9FBFD");
        }

        public static void StyleListView(ListView listView)
        {
            if (listView == null) return;

            listView.Font = ThemeFonts.Label;
            listView.BackColor = Color.White;
            listView.ForeColor = ThemeColors.Text;
            listView.BorderStyle = BorderStyle.FixedSingle;
            listView.HideSelection = false;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        public static void StyleTabControl(TabControl tabControl)
        {
            if (tabControl == null) return;

            tabControl.Font = ThemeFonts.Label;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.Padding = new Point(18, 8);
            tabControl.ItemSize = new Size(132, 34);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.DrawItem -= TabControlDrawItem;
            tabControl.DrawItem += TabControlDrawItem;
            foreach (TabPage page in tabControl.TabPages)
            {
                page.BackColor = ThemeColors.Background;
                page.ForeColor = ThemeColors.Text;
            }
        }

        public static void StyleSplitContainer(SplitContainer splitContainer)
        {
            if (splitContainer == null) return;

            splitContainer.BackColor = ThemeColors.Border;
        }

        private static void StyleButton(Button button, Color backColor, Color hoverBackColor, Color foreColor, int borderSize, Color borderColor)
        {
            if (button == null) return;

            button.Font = ThemeFonts.Button;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = borderSize;
            button.FlatAppearance.BorderColor = borderColor;
            button.UseVisualStyleBackColor = false;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.AutoSize = false;
            button.Height = Math.Max(button.Height, 34);
            button.Cursor = Cursors.Hand;
            button.AutoEllipsis = true;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.ImageAlign = button.Image == null ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            if (button.Padding == Padding.Empty)
            {
                button.Padding = button.Image == null ? new Padding(8, 0, 8, 0) : new Padding(10, 0, 12, 0);
            }
            button.FlatAppearance.MouseOverBackColor = hoverBackColor;
            button.FlatAppearance.MouseDownBackColor = Darken(backColor, 0.12f);
            UiHelper.ApplyRoundedCorners(button, 8);

            ButtonStyles[button] = new ButtonStyleMetadata
            {
                EnabledBackColor = backColor,
                EnabledForeColor = foreColor,
                EnabledBorderColor = borderColor,
                EnabledBorderSize = borderSize,
                DisabledBackColor = ThemeColors.SurfaceAlt,
                DisabledForeColor = ThemeColors.MutedText,
                DisabledBorderColor = ThemeColors.Border,
                DisabledBorderSize = 1
            };

            button.EnabledChanged -= ButtonEnabledChanged;
            button.EnabledChanged += ButtonEnabledChanged;
            ApplyButtonState(button);
        }

        private static void ButtonEnabledChanged(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            ApplyButtonState(button);
        }

        private static void ApplyButtonState(Button button)
        {
            if (button == null)
            {
                return;
            }

            ButtonStyleMetadata metadata;
            if (!ButtonStyles.TryGetValue(button, out metadata) || metadata == null)
            {
                return;
            }

            if (button.Enabled)
            {
                button.BackColor = metadata.EnabledBackColor;
                button.ForeColor = metadata.EnabledForeColor;
                button.FlatAppearance.BorderColor = metadata.EnabledBorderColor;
                button.FlatAppearance.BorderSize = metadata.EnabledBorderSize;
                button.Cursor = Cursors.Hand;
                return;
            }

            button.BackColor = metadata.DisabledBackColor;
            button.ForeColor = metadata.DisabledForeColor;
            button.FlatAppearance.BorderColor = metadata.DisabledBorderColor;
            button.FlatAppearance.BorderSize = metadata.DisabledBorderSize;
            button.Cursor = Cursors.Default;
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

            var listView = control as ListView;
            if (listView != null)
            {
                StyleListView(listView);
                return;
            }

            var tabControl = control as TabControl;
            if (tabControl != null)
            {
                StyleTabControl(tabControl);
                return;
            }

            var splitContainer = control as SplitContainer;
            if (splitContainer != null)
            {
                StyleSplitContainer(splitContainer);
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

        private static void CardPanelPaint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            var rect = new Rectangle(0, 0, panel.ClientSize.Width - 1, panel.ClientSize.Height - 1);
            if (rect.Width <= 1 || rect.Height <= 1) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var shadowPath = UiHelper.GetRoundedRectPath(new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height), 12))
            using (var shadowPen = new Pen(Color.FromArgb(10, 44, 62, 80)))
            {
                e.Graphics.DrawPath(shadowPen, shadowPath);
            }

            using (var borderPath = UiHelper.GetRoundedRectPath(rect, 12))
            using (var borderPen = new Pen(ThemeColors.Border))
            {
                e.Graphics.DrawPath(borderPen, borderPath);
            }
        }

        private static void GroupBoxPaint(object sender, PaintEventArgs e)
        {
            var groupBox = sender as GroupBox;
            if (groupBox == null) return;

            var backgroundColor = groupBox.Parent == null ? ThemeColors.Background : groupBox.Parent.BackColor;
            var rect = new Rectangle(0, 10, groupBox.ClientSize.Width - 1, groupBox.ClientSize.Height - 11);
            if (rect.Width <= 1 || rect.Height <= 1) return;

            e.Graphics.Clear(backgroundColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var backgroundBrush = new SolidBrush(groupBox.BackColor))
            using (var borderPath = UiHelper.GetRoundedRectPath(rect, 12))
            using (var borderPen = new Pen(ThemeColors.Border))
            {
                e.Graphics.FillPath(backgroundBrush, borderPath);
                e.Graphics.DrawPath(borderPen, borderPath);
            }

            var title = string.IsNullOrWhiteSpace(groupBox.Text) ? string.Empty : groupBox.Text.Trim();
            if (title.Length == 0) return;

            var textSize = TextRenderer.MeasureText(title, groupBox.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            var textRect = new Rectangle(16, 0, textSize.Width + 18, 22);
            using (var coverBrush = new SolidBrush(groupBox.BackColor))
            using (var accentPen = new Pen(Color.FromArgb(140, ThemeColors.Secondary), 2f))
            {
                e.Graphics.FillRectangle(coverBrush, textRect);
                TextRenderer.DrawText(e.Graphics, title, groupBox.Font, new Point(22, 0), ThemeColors.Text, TextFormatFlags.NoPadding);
                e.Graphics.DrawLine(accentPen, 18, 24, 66, 24);
            }
        }

        private static void TabControlDrawItem(object sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null || e.Index < 0 || e.Index >= tabControl.TabPages.Count)
            {
                return;
            }

            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var bounds = Rectangle.Inflate(e.Bounds, -4, -3);
            var fillColor = isSelected ? ThemeColors.CardBackground : ThemeColors.Surface;
            var borderColor = isSelected ? ThemeColors.Secondary : ThemeColors.Border;
            var textColor = isSelected ? ThemeColors.Text : ThemeColors.MutedText;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var fillBrush = new SolidBrush(fillColor))
            using (var borderPen = new Pen(borderColor))
            using (var path = UiHelper.GetRoundedRectPath(bounds, 10))
            {
                e.Graphics.FillPath(fillBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            var textBounds = new Rectangle(bounds.X + 10, bounds.Y + 1, bounds.Width - 20, bounds.Height - 2);
            TextRenderer.DrawText(
                e.Graphics,
                tabControl.TabPages[e.Index].Text,
                ThemeFonts.Label,
                textBounds,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
