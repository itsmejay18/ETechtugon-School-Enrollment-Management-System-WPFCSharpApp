using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.ViewModels.Shared
{
    public class DataTableWorkspaceViewModel : ViewModelBase, IModalStateHost
    {
        private readonly Func<string, DataTable> _loader;
        private string _searchText;
        private string _statusMessage;
        private DataView _records;
        private DataRowView _selectedRecord;
        private bool _isDetailsModalOpen;

        public DataTableWorkspaceViewModel(string title, string description, string searchHint, Func<string, DataTable> loader)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            SearchHint = searchHint ?? "Search records";
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));

            DetailFields = new ObservableCollection<DetailFieldViewModel>();
            SearchCommand = new RelayCommand(Refresh);
            RefreshCommand = new RelayCommand(Refresh);
            OpenDetailsCommand = new RelayCommand(OpenDetails, () => SelectedRecord != null);
            CloseDetailsCommand = new RelayCommand(CloseDetails);
        }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string SearchHint { get; private set; }
        public ObservableCollection<DetailFieldViewModel> DetailFields { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand OpenDetailsCommand { get; private set; }
        public RelayCommand CloseDetailsCommand { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            protected set { SetProperty(ref _statusMessage, value); }
        }

        public DataView Records
        {
            get { return _records; }
            protected set
            {
                if (SetProperty(ref _records, value))
                {
                    OnPropertyChanged(nameof(RecordCountText));
                    OnPropertyChanged(nameof(HasRecords));
                }
            }
        }

        public DataRowView SelectedRecord
        {
            get { return _selectedRecord; }
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    if (_selectedRecord == null)
                    {
                        IsDetailsModalOpen = false;
                    }

                    OnPropertyChanged(nameof(HasSelectedRecord));
                    OnPropertyChanged(nameof(DetailsModalTitle));
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                    OpenDetailsCommand.RaiseCanExecuteChanged();
                    OnSelectedRecordChanged();
                }
            }
        }

        public bool IsDetailsModalOpen
        {
            get { return _isDetailsModalOpen; }
            protected set
            {
                if (SetProperty(ref _isDetailsModalOpen, value))
                {
                    OnPropertyChanged(nameof(IsModalOpen));
                }
            }
        }

        public bool IsModalOpen
        {
            get { return IsDetailsModalOpen; }
        }

        public bool HasSelectedRecord
        {
            get { return SelectedRecord != null; }
        }

        public bool HasRecords
        {
            get { return Records != null && Records.Count > 0; }
        }

        public string RecordCountText
        {
            get { return (Records == null ? 0 : Records.Count).ToString() + " record(s) loaded"; }
        }

        protected DataRowView CurrentSelectedRecord
        {
            get { return _selectedRecord; }
        }

        public string DetailsModalTitle
        {
            get { return GetDetailsModalTitle(); }
        }

        public string DetailsModalSubtitle
        {
            get { return GetDetailsModalSubtitle(); }
        }

        public virtual void Refresh()
        {
            try
            {
                var table = _loader(SearchText ?? string.Empty) ?? new DataTable();
                Records = table.DefaultView;
                SelectedRecord = Records.Count > 0 ? Records[0] : null;
                if (Records.Count == 0)
                {
                    IsDetailsModalOpen = false;
                }

                StatusMessage = Records.Count > 0
                    ? "Loaded live records from the current school database."
                    : "No records matched the current search.";
            }
            catch (Exception ex)
            {
                Records = new DataView(new DataTable());
                SelectedRecord = null;
                IsDetailsModalOpen = false;
                StatusMessage = "Unable to load records right now.";
                MessageBox.Show(
                    ex.Message,
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        protected virtual void OnSelectedRecordChanged()
        {
            PopulateDetailFields(CurrentSelectedRecord);
        }

        protected virtual string GetDetailsModalTitle()
        {
            return BuildPreferredDisplayText(
                CurrentSelectedRecord,
                Title,
                "Name",
                "Title",
                "CourseName",
                "DepartmentName",
                "SectionName",
                "SubjectName",
                "StudentNumber",
                "FacultyCode",
                "Username",
                "SettingKey",
                "Action");
        }

        protected virtual string GetDetailsModalSubtitle()
        {
            if (CurrentSelectedRecord == null)
            {
                return Description;
            }

            var headline = GetDetailsModalTitle();
            var parts = new List<string>();
            var preferredColumns = new[]
            {
                "CourseCode",
                "DepartmentCode",
                "SubjectCode",
                "SectionCode",
                "Role",
                "Description",
                "CreatedAt",
                "UpdatedAt",
                "Status",
                "Value"
            };

            foreach (var columnName in preferredColumns)
            {
                AddDistinctPart(parts, GetColumnValue(CurrentSelectedRecord, columnName), headline);
                if (parts.Count >= 3)
                {
                    break;
                }
            }

            if (parts.Count == 0 && CurrentSelectedRecord != null)
            {
                foreach (DataColumn column in CurrentSelectedRecord.Row.Table.Columns)
                {
                    AddDistinctPart(parts, WpfUiDataHelper.FormatValue(CurrentSelectedRecord.Row[column]), headline);
                    if (parts.Count >= 3)
                    {
                        break;
                    }
                }
            }

            return parts.Count > 0
                ? string.Join(" | ", parts)
                : "Review the selected record from the live database.";
        }

        protected void PopulateDetailFields(DataRowView row, params string[] excludedColumns)
        {
            DetailFields.Clear();

            if (row == null)
            {
                return;
            }

            foreach (DataColumn column in row.Row.Table.Columns)
            {
                if (column == null)
                {
                    continue;
                }

                var isExcluded = false;
                if (excludedColumns != null)
                {
                    foreach (var excluded in excludedColumns)
                    {
                        if (string.Equals(column.ColumnName, excluded, StringComparison.OrdinalIgnoreCase))
                        {
                            isExcluded = true;
                            break;
                        }
                    }
                }

                if (isExcluded)
                {
                    continue;
                }

                DetailFields.Add(new DetailFieldViewModel
                {
                    Label = WpfUiDataHelper.ToFriendlyLabel(column.ColumnName),
                    Value = WpfUiDataHelper.FormatValue(row.Row[column])
                });
            }
        }

        protected static string BuildPreferredDisplayText(DataRowView row, string fallback, params string[] preferredColumns)
        {
            if (row != null)
            {
                foreach (var columnName in preferredColumns ?? Array.Empty<string>())
                {
                    var value = GetColumnValue(row, columnName);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }

                foreach (DataColumn column in row.Row.Table.Columns)
                {
                    var value = WpfUiDataHelper.FormatValue(row.Row[column]);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return fallback ?? string.Empty;
        }

        protected static string GetColumnValue(DataRowView row, string columnName)
        {
            if (row == null || string.IsNullOrWhiteSpace(columnName) || row.Row.Table == null || !row.Row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            return WpfUiDataHelper.FormatValue(row.Row[columnName]);
        }

        private void OpenDetails()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            IsDetailsModalOpen = true;
        }

        private void CloseDetails()
        {
            IsDetailsModalOpen = false;
        }

        private static void AddDistinctPart(ICollection<string> parts, string value, string headline)
        {
            if (parts == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (string.Equals(value, headline, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var existing in parts)
            {
                if (string.Equals(existing, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            parts.Add(value);
        }
    }
}
