using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.ViewModels.Shared
{
    public class DataTableWorkspaceViewModel : ViewModelBase
    {
        private readonly Func<string, DataTable> _loader;
        private string _searchText;
        private string _statusMessage;
        private DataView _records;
        private DataRowView _selectedRecord;

        public DataTableWorkspaceViewModel(string title, string description, string searchHint, Func<string, DataTable> loader)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            SearchHint = searchHint ?? "Search records";
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));

            DetailFields = new ObservableCollection<DetailFieldViewModel>();
            SearchCommand = new RelayCommand(Refresh);
            RefreshCommand = new RelayCommand(Refresh);
        }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string SearchHint { get; private set; }
        public ObservableCollection<DetailFieldViewModel> DetailFields { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

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
                    OnSelectedRecordChanged();
                }
            }
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

        public virtual void Refresh()
        {
            try
            {
                var table = _loader(SearchText ?? string.Empty) ?? new DataTable();
                Records = table.DefaultView;
                SelectedRecord = Records.Count > 0 ? Records[0] : null;
                StatusMessage = Records.Count > 0
                    ? "Loaded live records from the current school database."
                    : "No records matched the current search.";
            }
            catch (Exception ex)
            {
                Records = new DataView(new DataTable());
                SelectedRecord = null;
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
    }
}
