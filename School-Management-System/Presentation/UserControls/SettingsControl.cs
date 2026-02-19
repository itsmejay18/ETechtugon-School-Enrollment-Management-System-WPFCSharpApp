using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Forms;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.UserControls
{
    public sealed class SettingsControl : BaseUserControl
    {
        private readonly SystemSettingService _settingsService;
        private readonly LookupService _lookupService;

        private ComboBox _cmbAcademicYear;
        private ComboBox _cmbSemester;
        private Button _btnSave;

        public SettingsControl()
            : this(null, null)
        {
        }

        public SettingsControl(SystemSettingService settingsService, LookupService lookupService)
        {
            _settingsService = settingsService;
            _lookupService = lookupService;

            InitializeComponent();
            LoadLookups();
            LoadCurrent();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;

            var gb = new GroupBox
            {
                Text = "Global Settings",
                Dock = DockStyle.Top,
                Height = 200,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(gb);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(8, 6, 8, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            _cmbAcademicYear = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbAcademicYear);

            _cmbSemester = new ComboBox { Dock = DockStyle.Fill, Font = ThemeFonts.Input };
            ThemeManager.StyleComboBox(_cmbSemester);

            _btnSave = new Button { Text = "Save Active Term", Width = 160, Height = 32, Anchor = AnchorStyles.Left };
            ThemeManager.StyleButtonPrimary(_btnSave);
            _btnSave.Click += (s, e) => SaveSettings();

            layout.Controls.Add(MakeLabel("Current School Year"), 0, 0);
            layout.Controls.Add(_cmbAcademicYear, 1, 0);
            layout.Controls.Add(MakeLabel("Current Semester"), 0, 1);
            layout.Controls.Add(_cmbSemester, 1, 1);
            layout.Controls.Add(_btnSave, 1, 2);

            gb.Controls.Add(layout);

            Controls.Add(gb);
        }

        private static Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void LoadLookups()
        {
            if (_lookupService == null) return;
            try
            {
                _cmbAcademicYear.DisplayMember = "Name";
                _cmbAcademicYear.ValueMember = "AcademicYearId";
                _cmbAcademicYear.DataSource = _lookupService.GetAcademicYears();

                _cmbSemester.DisplayMember = "Name";
                _cmbSemester.ValueMember = "SemesterId";
                _cmbSemester.DataSource = _lookupService.GetSemesters();
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadLookups", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }

        private void LoadCurrent()
        {
            if (_settingsService == null) return;
            try
            {
                var term = _settingsService.GetActiveTerm();
                if (term.AcademicYearId.HasValue)
                {
                    _cmbAcademicYear.SelectedValue = term.AcademicYearId.Value;
                }

                if (term.SemesterId.HasValue)
                {
                    _cmbSemester.SelectedValue = term.SemesterId.Value;
                }
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.LoadCurrent", ex);
            }
        }

        private void SaveSettings()
        {
            if (_settingsService == null) return;
            try
            {
                var ayId = _cmbAcademicYear.SelectedValue == null ? 0 : Convert.ToInt32(_cmbAcademicYear.SelectedValue);
                var semId = _cmbSemester.SelectedValue == null ? 0 : Convert.ToInt32(_cmbSemester.SelectedValue);

                if (ayId <= 0 || semId <= 0)
                {
                    ThemedMessageBox.ShowError(this, "Please select both School Year and Semester.", "Settings");
                    return;
                }

                _settingsService.SetActiveTerm(ayId, semId);
                ThemedMessageBox.ShowInfo(this, "Active term updated. New enrollments will default to these values.", "Settings");
            }
            catch (Exception ex)
            {
                School_Management_System.DataLayer.Logging.FileLogger.LogError("SettingsControl.SaveSettings", ex);
                ThemedMessageBox.ShowError(this, Messages.UnexpectedError);
            }
        }
    }
}
