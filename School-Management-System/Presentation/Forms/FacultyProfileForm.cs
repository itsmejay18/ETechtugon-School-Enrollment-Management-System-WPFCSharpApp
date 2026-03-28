using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using School_Management_System.Models;
using School_Management_System.Presentation.Base;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Presentation.Theming;

namespace School_Management_System.Presentation.Forms
{
    public sealed class FacultyProfileForm : BaseForm
    {
        private readonly Faculty _faculty;
        private readonly byte[] _photoData;
        private readonly DataTable _assignments;

        public FacultyProfileForm(Faculty faculty, byte[] photoData, DataTable assignments)
        {
            _faculty = faculty ?? new Faculty();
            _photoData = photoData;
            _assignments = assignments ?? new DataTable();

            Text = "Faculty Profile";
            MinimumSize = new Size(860, 560);
            Size = new Size(980, 680);

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = ThemeColors.Background;
            Padding = new Padding(18);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 228));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildHeaderCard(), 0, 0);
            root.Controls.Add(BuildAssignmentsCard(), 0, 1);

            Controls.Add(root);
        }

        private Control BuildHeaderCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.CardBackground,
                Padding = new Padding(18)
            };
            ThemeManager.StyleCardPanel(card);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            var details = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(0)
            };
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var fullName = new Label
            {
                Text = BuildPersonName(_faculty.FirstName, _faculty.MiddleName, _faculty.LastName),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            details.Controls.Add(fullName, 0, 0);
            details.SetColumnSpan(fullName, 2);

            details.Controls.Add(MakeCaption("Faculty No."), 0, 1);
            details.Controls.Add(MakeValue(_faculty.FacultyCode), 1, 1);
            details.Controls.Add(MakeCaption("Status"), 0, 2);
            details.Controls.Add(MakeValue(_faculty.IsActive ? "Active" : "Inactive"), 1, 2);
            details.Controls.Add(MakeCaption("Email"), 0, 3);
            details.Controls.Add(MakeValue(_faculty.Email), 1, 3);
            details.Controls.Add(MakeCaption("Phone"), 0, 4);
            details.Controls.Add(MakeValue(_faculty.Phone), 1, 4);
            details.Controls.Add(MakeCaption("Hire Date"), 0, 5);
            details.Controls.Add(MakeValue(_faculty.HireDate.HasValue ? _faculty.HireDate.Value.ToString("MMM dd, yyyy") : "Not set"), 1, 5);
            details.Controls.Add(MakeCaption("Address"), 0, 6);
            details.Controls.Add(MakeValue(_faculty.Address), 1, 6);

            var photoHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 0, 0, 0),
                BackColor = Color.Transparent
            };

            var photoCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 190,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(14)
            };
            ThemeManager.StyleCardPanel(photoCard);

            var photo = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            PhotoStorageHelper.ShowPhotoFromBytes(photo, _photoData);

            photoCard.Controls.Add(photo);
            photoHost.Controls.Add(photoCard);

            layout.Controls.Add(details, 0, 0);
            layout.Controls.Add(photoHost, 1, 0);

            card.Controls.Add(layout);
            return card;
        }

        private Control BuildAssignmentsCard()
        {
            var card = new GroupBox
            {
                Text = "Assigned Subjects and Schedule",
                Dock = DockStyle.Fill,
                Font = ThemeFonts.SubHeader,
                ForeColor = ThemeColors.Text,
                Padding = new Padding(12, 18, 12, 12),
                BackColor = ThemeColors.CardBackground
            };
            ThemeManager.StyleGroupBox(card);

            var list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            ThemeManager.StyleListView(list);
            list.Columns.Add("Subject", 230);
            list.Columns.Add("Section", 110);
            list.Columns.Add("Schedule", 170);
            list.Columns.Add("Room", 120);
            list.Columns.Add("Term", 170);

            foreach (DataRow row in _assignments.Rows)
            {
                var item = new ListViewItem(BuildSubjectLabel(row));
                item.SubItems.Add(Convert.ToString(row["SectionName"]));
                item.SubItems.Add(BuildScheduleText(row));
                item.SubItems.Add(Convert.ToString(row["Room"]));
                item.SubItems.Add(BuildTermText(row));
                list.Items.Add(item);
            }

            card.Controls.Add(list);
            return card;
        }

        private static Control MakeCaption(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.MutedText,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Control MakeValue(string text)
        {
            return new Label
            {
                Text = string.IsNullOrWhiteSpace(text) ? "Not provided" : text,
                Dock = DockStyle.Fill,
                Font = ThemeFonts.Label,
                ForeColor = ThemeColors.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
        }

        private static string BuildPersonName(string firstName, string middleName, string lastName)
        {
            var givenName = (firstName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                givenName = (givenName + " " + middleName.Trim().Substring(0, 1).ToUpperInvariant() + ".").Trim();
            }

            var familyName = (lastName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(givenName)) return familyName;
            if (string.IsNullOrWhiteSpace(familyName)) return givenName;
            return familyName + ", " + givenName;
        }

        private static string BuildSubjectLabel(DataRow row)
        {
            if (row == null) return string.Empty;
            var code = Convert.ToString(row["SubjectCode"]);
            var name = Convert.ToString(row["SubjectName"]);
            return ((code ?? string.Empty) + " - " + (name ?? string.Empty)).Trim(new[] { ' ', '-' });
        }

        private static string BuildScheduleText(DataRow row)
        {
            if (row == null) return string.Empty;

            var day = Convert.ToString(row["DayOfWeek"]);
            string start = null;
            string end = null;

            try
            {
                var startTime = row["StartTime"] as TimeSpan?;
                if (startTime.HasValue)
                {
                    start = startTime.Value.ToString(@"hh\:mm");
                }

                var endTime = row["EndTime"] as TimeSpan?;
                if (endTime.HasValue)
                {
                    end = endTime.Value.ToString(@"hh\:mm");
                }
            }
            catch
            {
                // Ignore invalid time formatting.
            }

            var time = string.Empty;
            if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
            {
                time = start + "-" + end;
            }

            return string.Join(" | ", Array.FindAll(new[] { day, time }, delegate(string value)
            {
                return !string.IsNullOrWhiteSpace(value);
            }));
        }

        private static string BuildTermText(DataRow row)
        {
            if (row == null) return string.Empty;

            return string.Join(" | ", Array.FindAll(
                new[]
                {
                    Convert.ToString(row["Semester"]),
                    Convert.ToString(row["AcademicYear"])
                },
                delegate(string value) { return !string.IsNullOrWhiteSpace(value); }));
        }
    }
}
