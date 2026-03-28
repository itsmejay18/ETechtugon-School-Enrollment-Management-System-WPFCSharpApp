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
