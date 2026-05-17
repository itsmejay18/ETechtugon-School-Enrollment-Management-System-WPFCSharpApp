using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public abstract class CatalogCrudWorkspaceViewModel : ViewModelBase, IModalStateHost
    {
        private string _searchText;
        private CatalogRecordViewModel _selectedRecord;
        private bool _isEditorActive;
        private bool _isEditorModalOpen;
        private string _statusMessage;
        private int _editingId;
        private string _code;
        private string _name;
        private string _extra1;
        private string _extra2;
        private string _recordDescription;
        private LookupOptionViewModel _selectedOption;

        protected CatalogCrudWorkspaceViewModel(
            string title,
            string description,
            string codeLabel,
            string nameLabel,
            string optionLabel,
            string extra1Label,
            string extra2Label,
            string descriptionLabel)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            CodeLabel = codeLabel ?? "Code";
            NameLabel = nameLabel ?? "Name";
            OptionLabel = optionLabel ?? string.Empty;
            Extra1Label = extra1Label ?? string.Empty;
            Extra2Label = extra2Label ?? string.Empty;
            DescriptionLabel = descriptionLabel ?? "Description";

            Records = new ObservableCollection<CatalogRecordViewModel>();
            Options = new ObservableCollection<LookupOptionViewModel>();

            RefreshCommand = new RelayCommand(Refresh);
            AddCommand = new RelayCommand(BeginAdd);
            OpenDetailsCommand = new RelayCommand(OpenDetails, () => SelectedRecord != null && !IsEditorActive);
            EditCommand = new RelayCommand(BeginEdit, () => SelectedRecord != null && !IsEditorActive);
            DeleteCommand = new RelayCommand(DeleteCurrent, () => SelectedRecord != null && !IsEditorActive);
            SaveCommand = new RelayCommand(Save, () => IsEditorActive);
            CancelCommand = new RelayCommand(CancelEdit, () => IsEditorActive);
            CloseModalCommand = new RelayCommand(CloseModal);
        }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string CodeLabel { get; private set; }
        public string NameLabel { get; private set; }
        public string OptionLabel { get; private set; }
        public string Extra1Label { get; private set; }
        public string Extra2Label { get; private set; }
        public string DescriptionLabel { get; private set; }
        public ObservableCollection<CatalogRecordViewModel> Records { get; private set; }
        public ObservableCollection<LookupOptionViewModel> Options { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand OpenDetailsCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand CloseModalCommand { get; private set; }

        public bool HasOption { get { return !string.IsNullOrWhiteSpace(OptionLabel); } }
        public bool HasExtra1 { get { return !string.IsNullOrWhiteSpace(Extra1Label); } }
        public bool HasExtra2 { get { return !string.IsNullOrWhiteSpace(Extra2Label); } }
        public bool HasDescription { get { return !string.IsNullOrWhiteSpace(DescriptionLabel); } }
        public bool IsPreviewMode { get { return !IsEditorActive; } }
        public bool IsEditorReadOnly { get { return !IsEditorActive; } }
        public bool IsModalOpen { get { return IsEditorModalOpen; } }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    Refresh();
                }
            }
        }

        public CatalogRecordViewModel SelectedRecord
        {
            get { return _selectedRecord; }
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    if (!IsEditorActive)
                    {
                        LoadEditorFromSelected();
                    }

                    OnPropertyChanged(nameof(EditorModalTitle));
                    OnPropertyChanged(nameof(EditorModalSubtitle));
                    RaiseCommandStates();
                }
            }
        }

        public bool IsEditorActive
        {
            get { return _isEditorActive; }
            private set
            {
                if (SetProperty(ref _isEditorActive, value))
                {
                    OnPropertyChanged(nameof(IsPreviewMode));
                    OnPropertyChanged(nameof(IsEditorReadOnly));
                    OnPropertyChanged(nameof(EditorModalTitle));
                    OnPropertyChanged(nameof(EditorModalSubtitle));
                    RaiseCommandStates();
                }
            }
        }

        public bool IsEditorModalOpen
        {
            get { return _isEditorModalOpen; }
            private set
            {
                if (SetProperty(ref _isEditorModalOpen, value))
                {
                    OnPropertyChanged(nameof(IsModalOpen));
                }
            }
        }

        public string Code { get { return _code; } set { SetProperty(ref _code, value); } }
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        public string Extra1 { get { return _extra1; } set { SetProperty(ref _extra1, value); } }
        public string Extra2 { get { return _extra2; } set { SetProperty(ref _extra2, value); } }
        public string RecordDescription { get { return _recordDescription; } set { SetProperty(ref _recordDescription, value); } }
        public LookupOptionViewModel SelectedOption { get { return _selectedOption; } set { SetProperty(ref _selectedOption, value); } }

        public string StatusMessage { get { return _statusMessage; } protected set { SetProperty(ref _statusMessage, value); } }

        public string EditorModalTitle
        {
            get
            {
                if (IsEditorActive && _editingId == 0)
                {
                    return "New " + Title.ToLowerInvariant();
                }

                return SelectedRecord == null || string.IsNullOrWhiteSpace(SelectedRecord.Name)
                    ? Title + " details"
                    : SelectedRecord.Name;
            }
        }

        public string EditorModalSubtitle
        {
            get
            {
                if (IsEditorActive)
                {
                    return _editingId == 0
                        ? "Create a new record."
                        : "Update the selected record.";
                }

                return SelectedRecord == null
                    ? "Select a record to preview."
                    : (SelectedRecord.Code ?? string.Empty) + " is open in preview mode.";
            }
        }

        public void Initialize()
        {
            RefreshOptions();
            Refresh();
        }

        protected abstract DataTable LoadRecords(string search);
        protected abstract void LoadOptionRecords(ObservableCollection<LookupOptionViewModel> options);
        protected abstract CatalogRecordViewModel BuildRecord(DataRow row);
        protected abstract ValidationResult ValidateEditor();
        protected abstract int SaveCore(int id);
        protected abstract void DeleteCore(int id);

        protected int? SelectedOptionId
        {
            get { return SelectedOption == null || SelectedOption.Id <= 0 ? (int?)null : SelectedOption.Id; }
        }

        protected static int ParseRequiredInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }

        protected static int? ParseOptionalInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int parsed;
            return int.TryParse(value, out parsed) ? (int?)parsed : null;
        }

        private void RefreshOptions()
        {
            Options.Clear();
            LoadOptionRecords(Options);
            SelectedOption = Options.FirstOrDefault();
        }

        private void Refresh()
        {
            try
            {
                var selectedId = SelectedRecord == null ? 0 : SelectedRecord.Id;
                var table = LoadRecords(SearchText ?? string.Empty) ?? new DataTable();
                Records.Clear();
                foreach (DataRow row in table.Rows)
                {
                    Records.Add(BuildRecord(row));
                }

                SelectedRecord = Records.FirstOrDefault(r => r.Id == selectedId) ?? Records.FirstOrDefault();
                StatusMessage = Records.Count > 0
                    ? "Records loaded from the current school database."
                    : "No records matched the current search.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Unable to load records right now.";
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BeginAdd()
        {
            _editingId = 0;
            SelectedRecord = null;
            Code = string.Empty;
            Name = string.Empty;
            Extra1 = string.Empty;
            Extra2 = string.Empty;
            RecordDescription = string.Empty;
            SelectedOption = Options.FirstOrDefault();
            IsEditorActive = true;
            IsEditorModalOpen = true;
        }

        private void OpenDetails()
        {
            LoadEditorFromSelected();
            IsEditorActive = false;
            IsEditorModalOpen = SelectedRecord != null;
        }

        private void BeginEdit()
        {
            if (SelectedRecord == null)
            {
                return;
            }

            LoadEditorFromSelected();
            IsEditorActive = true;
            IsEditorModalOpen = true;
        }

        private void CancelEdit()
        {
            IsEditorActive = false;
            LoadEditorFromSelected();
            IsEditorModalOpen = SelectedRecord != null;
        }

        private void CloseModal()
        {
            if (IsEditorActive)
            {
                CancelEdit();
            }

            IsEditorModalOpen = false;
        }

        private void Save()
        {
            var validation = ValidateEditor();
            if (validation != null && !validation.IsValid)
            {
                MessageBox.Show(validation.ToString(), Title + " Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var savedId = SaveCore(_editingId);
                IsEditorActive = false;
                IsEditorModalOpen = false;
                StatusMessage = "Record saved successfully.";
                RefreshOptions();
                Refresh();
                SelectedRecord = Records.FirstOrDefault(r => r.Id == savedId) ?? Records.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCurrent()
        {
            if (SelectedRecord == null)
            {
                return;
            }

            var result = MessageBox.Show("Delete the selected record?", Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                DeleteCore(SelectedRecord.Id);
                IsEditorModalOpen = false;
                StatusMessage = "Record deleted successfully.";
                RefreshOptions();
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEditorFromSelected()
        {
            if (SelectedRecord == null)
            {
                _editingId = 0;
                return;
            }

            _editingId = SelectedRecord.Id;
            Code = SelectedRecord.Code;
            Name = SelectedRecord.Name;
            Extra1 = SelectedRecord.DetailA;
            Extra2 = SelectedRecord.DetailB;
            RecordDescription = SelectedRecord.Description;
            SelectedOption = Options.FirstOrDefault(o => o.Id == (SelectedRecord.OptionId ?? 0)) ?? Options.FirstOrDefault();
        }

        private void RaiseCommandStates()
        {
            OpenDetailsCommand.RaiseCanExecuteChanged();
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
