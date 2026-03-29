using System;
using System.ComponentModel;
using System.Drawing;
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
            AutoScaleMode = AutoScaleMode.None;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (IsInDesigner) return;
            ThemeManager.ApplyPaletteToControlTree(this);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e == null || e.Control == null || IsInDesigner) return;
            ThemeManager.ApplyPaletteToControlTree(e.Control);
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

        protected static bool HasSelectedDataRow(DataGridView grid, string keyColumnName = null)
        {
            if (grid == null || grid.SelectedRows.Count == 0)
            {
                return false;
            }

            var row = grid.SelectedRows[0];
            if (row == null || row.IsNewRow)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(keyColumnName))
            {
                return true;
            }

            if (row.DataGridView == null || !row.DataGridView.Columns.Contains(keyColumnName))
            {
                return false;
            }

            var value = row.Cells[keyColumnName].Value;
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        protected static bool HasSelectedListItem(ListView listView)
        {
            return listView != null && listView.SelectedItems.Count > 0 && listView.SelectedItems[0] != null;
        }

        protected static void ApplyCrudButtonState(
            bool active,
            bool hasSelection,
            Button addButton,
            Button editButton,
            Button deleteButton,
            Button saveButton,
            Button cancelButton,
            params Button[] extraSelectionButtons)
        {
            if (saveButton != null) saveButton.Enabled = active;
            if (cancelButton != null) cancelButton.Enabled = active;
            if (addButton != null) addButton.Enabled = !active;
            if (editButton != null) editButton.Enabled = !active && hasSelection;
            if (deleteButton != null) deleteButton.Enabled = !active && hasSelection;

            if (extraSelectionButtons == null)
            {
                return;
            }

            foreach (var button in extraSelectionButtons)
            {
                if (button != null)
                {
                    button.Enabled = !active && hasSelection;
                }
            }
        }

        protected static void LockSplitEditorPanel(SplitContainer split, int editorPanelWidth, int minGridWidth = 200)
        {
            if (split == null) return;

            if (editorPanelWidth < 240) editorPanelWidth = 240;
            if (minGridWidth < 120) minGridWidth = 120;

            split.FixedPanel = FixedPanel.Panel2;
            split.IsSplitterFixed = true;

            EventHandler apply = (s, e) =>
            {
                var usableWidth = split.ClientSize.Width - split.SplitterWidth;
                if (usableWidth <= 0) return;

                // Keep a stable right panel width while ensuring SplitterDistance stays valid.
                var rightWidth = editorPanelWidth;
                var maxRightWidth = usableWidth - minGridWidth;
                if (maxRightWidth < 180) maxRightWidth = Math.Max(0, usableWidth);
                if (rightWidth > maxRightWidth) rightWidth = maxRightWidth;
                if (rightWidth < 180) rightWidth = Math.Min(180, usableWidth);

                var target = usableWidth - rightWidth;
                if (target < 0) target = 0;

                var safePanel1Min = Math.Min(minGridWidth, target);
                var safePanel2Min = Math.Min(rightWidth, usableWidth - safePanel1Min);

                try
                {
                    // Reset mins first; setting min sizes can throw if current splitter is out of range.
                    split.Panel1MinSize = 0;
                    split.Panel2MinSize = 0;
                    split.SplitterDistance = target;
                    split.Panel1MinSize = safePanel1Min;
                    split.Panel2MinSize = safePanel2Min;
                }
                catch
                {
                    // Ignore transient layout exceptions during handle creation.
                }
            };

            split.SizeChanged += apply;
            split.HandleCreated += apply;
            apply(split, EventArgs.Empty);
        }
    }
}
