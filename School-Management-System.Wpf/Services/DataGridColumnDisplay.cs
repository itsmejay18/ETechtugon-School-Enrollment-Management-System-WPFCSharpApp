using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.Services
{
    public static class DataGridColumnDisplay
    {
        private const string AutoProfile = "Auto";

        private static readonly IDictionary<string, string[]> ProfileColumns = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Student", new[] { "StudentNumber", "LastName", "FirstName", "MiddleName", "Gender" } },
            { "Faculty", new[] { "FacultyCode", "LastName", "FirstName", "MiddleName" } },
            { "Course", new[] { "CourseCode", "CourseName", "DepartmentName" } },
            { "Department", new[] { "DepartmentCode", "DepartmentName" } },
            { "Subject", new[] { "SubjectCode", "SubjectName", "Units", "CourseCode" } },
            { "Section", new[] { "SectionName", "CourseName", "YearLevel", "AcademicYear", "Semester", "Capacity" } },
            { "Schedule", new[] { "SubjectCode", "SubjectName", "SectionName", "FacultyName", "DayOfWeek", "StartTime", "EndTime", "Room", "Units" } },
            { "Reference", new[] { "Name", "IsCurrent", "SortOrder" } },
            { "Setting", new[] { "SettingKey", "SettingValue" } },
            { "Activity", new[] { "CreatedAt", "Username", "Action", "Entity", "Details" } },
            { "User", new[] { "Username", "DisplayName", "Role", "IsActive" } }
        };

        private static readonly ISet<string> HiddenMainColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Address",
            "Description",
            "Email",
            "EntityId",
            "HireDate",
            "MachineName",
            "Phone",
            "PhotoData",
            "PhotoPath",
            "Remarks",
            "UpdatedAt"
        };

        private static readonly DependencyProperty AutoColumnNameProperty =
            DependencyProperty.RegisterAttached(
                "AutoColumnName",
                typeof(string),
                typeof(DataGridColumnDisplay),
                new PropertyMetadata(string.Empty));

        private static readonly DependencyProperty ExpandedItemProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedItem",
                typeof(object),
                typeof(DataGridColumnDisplay),
                new PropertyMetadata(null));

        public static readonly DependencyProperty UseCompactColumnsProperty =
            DependencyProperty.RegisterAttached(
                "UseCompactColumns",
                typeof(bool),
                typeof(DataGridColumnDisplay),
                new PropertyMetadata(false, OnUseCompactColumnsChanged));

        public static readonly DependencyProperty ColumnProfileProperty =
            DependencyProperty.RegisterAttached(
                "ColumnProfile",
                typeof(string),
                typeof(DataGridColumnDisplay),
                new PropertyMetadata(AutoProfile));

        public static readonly DependencyProperty ShowSelectedRowDetailsProperty =
            DependencyProperty.RegisterAttached(
                "ShowSelectedRowDetails",
                typeof(bool),
                typeof(DataGridColumnDisplay),
                new PropertyMetadata(false, OnShowSelectedRowDetailsChanged));

        public static bool GetUseCompactColumns(DependencyObject element)
        {
            return element != null && (bool)element.GetValue(UseCompactColumnsProperty);
        }

        public static void SetUseCompactColumns(DependencyObject element, bool value)
        {
            if (element != null)
            {
                element.SetValue(UseCompactColumnsProperty, value);
            }
        }

        public static string GetColumnProfile(DependencyObject element)
        {
            return element == null ? AutoProfile : (string)element.GetValue(ColumnProfileProperty);
        }

        public static void SetColumnProfile(DependencyObject element, string value)
        {
            if (element != null)
            {
                element.SetValue(ColumnProfileProperty, value);
            }
        }

        public static bool GetShowSelectedRowDetails(DependencyObject element)
        {
            return element != null && (bool)element.GetValue(ShowSelectedRowDetailsProperty);
        }

        public static void SetShowSelectedRowDetails(DependencyObject element, bool value)
        {
            if (element != null)
            {
                element.SetValue(ShowSelectedRowDetailsProperty, value);
            }
        }

        private static void OnUseCompactColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (grid == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                grid.AutoGeneratingColumn += Grid_AutoGeneratingColumn;
                grid.AutoGeneratedColumns += Grid_AutoGeneratedColumns;
            }
            else
            {
                grid.AutoGeneratingColumn -= Grid_AutoGeneratingColumn;
                grid.AutoGeneratedColumns -= Grid_AutoGeneratedColumns;
            }
        }

        private static void OnShowSelectedRowDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (grid == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                grid.RowDetailsTemplate = CreateRowDetailsTemplate();
                grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                grid.PreviewMouseLeftButtonUp += Grid_PreviewMouseLeftButtonUp;
                grid.LoadingRow += Grid_LoadingRow;
            }
            else
            {
                grid.PreviewMouseLeftButtonUp -= Grid_PreviewMouseLeftButtonUp;
                grid.LoadingRow -= Grid_LoadingRow;
                grid.ClearValue(ExpandedItemProperty);
            }
        }

        private static void Grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || e == null || e.Column == null)
            {
                return;
            }

            var profile = ResolveProfile(grid);
            if (!ShouldShowColumn(profile, e.PropertyName))
            {
                e.Cancel = true;
                return;
            }

            SetAutoColumnName(e.Column, e.PropertyName);
            e.Column.Header = GetColumnHeader(profile, e.PropertyName);
            e.Column.Width = GetColumnWidth(e.PropertyName);
        }

        private static void Grid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.Columns.Count == 0)
            {
                return;
            }

            var profile = ResolveProfile(grid);
            var orderedColumns = grid.Columns
                .OrderBy(column => GetColumnOrder(profile, GetAutoColumnName(column)))
                .ThenBy(column => column.DisplayIndex)
                .ToList();

            for (var i = 0; i < orderedColumns.Count; i++)
            {
                orderedColumns[i].DisplayIndex = i;
            }
        }

        private static void Grid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || e == null || e.Row == null)
            {
                return;
            }

            e.Row.DetailsVisibility = Equals(e.Row.Item, GetExpandedItem(grid))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private static void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || e == null)
            {
                return;
            }

            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null || row.Item == null)
            {
                return;
            }

            SetExpandedItem(grid, row.Item);
            UpdateVisibleRowDetails(grid);
        }

        private static string ResolveProfile(DataGrid grid)
        {
            var explicitProfile = GetColumnProfile(grid);
            if (!string.IsNullOrWhiteSpace(explicitProfile) &&
                !string.Equals(explicitProfile, AutoProfile, StringComparison.OrdinalIgnoreCase))
            {
                return explicitProfile;
            }

            return InferProfile(GetSourceColumnNames(grid));
        }

        private static string InferProfile(IEnumerable<string> columnNames)
        {
            var names = new HashSet<string>(columnNames ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            if (names.Contains("StudentNumber")) return "Student";
            if (names.Contains("FacultyCode")) return "Faculty";
            if (names.Contains("SettingKey")) return "Setting";
            if (names.Contains("Action") && names.Contains("Details") && names.Contains("CreatedAt")) return "Activity";
            if (names.Contains("Username") && names.Contains("Role")) return "User";
            if (names.Contains("DepartmentCode")) return "Department";
            if (names.Contains("SectionName") && names.Contains("YearLevel") && names.Contains("AcademicYear")) return "Section";
            if (names.Contains("SubjectCode") && names.Contains("DayOfWeek")) return "Schedule";
            if (names.Contains("SubjectCode") && names.Contains("SubjectName")) return "Subject";
            if (names.Contains("CourseCode") && names.Contains("CourseName")) return "Course";
            if (names.Contains("Name") && (names.Contains("SortOrder") || names.Contains("IsCurrent"))) return "Reference";

            return AutoProfile;
        }

        private static IEnumerable<string> GetSourceColumnNames(DataGrid grid)
        {
            var dataView = grid.ItemsSource as DataView;
            if (dataView != null && dataView.Table != null)
            {
                foreach (DataColumn column in dataView.Table.Columns)
                {
                    yield return column.ColumnName;
                }

                yield break;
            }

            var dataTable = grid.ItemsSource as DataTable;
            if (dataTable != null)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    yield return column.ColumnName;
                }
            }
        }

        private static bool ShouldShowColumn(string profile, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            if (IsTechnicalColumn(columnName) || HiddenMainColumns.Contains(columnName))
            {
                return false;
            }

            string[] allowedColumns;
            if (ProfileColumns.TryGetValue(profile ?? AutoProfile, out allowedColumns))
            {
                return allowedColumns.Any(column => string.Equals(column, columnName, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        private static bool IsTechnicalColumn(string columnName)
        {
            return columnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
                   columnName.EndsWith("ID", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetColumnHeader(string profile, string columnName)
        {
            if (string.Equals(profile, "Student", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(columnName, "StudentNumber", StringComparison.OrdinalIgnoreCase))
            {
                return "Code";
            }

            if (string.Equals(profile, "Faculty", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(columnName, "FacultyCode", StringComparison.OrdinalIgnoreCase))
            {
                return "Code";
            }

            return WpfUiDataHelper.ToFriendlyLabel(columnName);
        }

        private static DataGridLength GetColumnWidth(string columnName)
        {
            if (string.Equals(columnName, "FirstName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "LastName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "SubjectName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "CourseName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "DepartmentName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "SectionName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "Details", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "SettingValue", StringComparison.OrdinalIgnoreCase))
            {
                return new DataGridLength(1, DataGridLengthUnitType.Star);
            }

            if (string.Equals(columnName, "MiddleName", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "FacultyName", StringComparison.OrdinalIgnoreCase))
            {
                return new DataGridLength(170);
            }

            if (columnName.IndexOf("Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                string.Equals(columnName, "StudentNumber", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "Username", StringComparison.OrdinalIgnoreCase))
            {
                return new DataGridLength(150);
            }

            if (string.Equals(columnName, "Gender", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "Units", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "Room", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "IsCurrent", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "IsActive", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, "SortOrder", StringComparison.OrdinalIgnoreCase))
            {
                return new DataGridLength(100);
            }

            return new DataGridLength(140);
        }

        private static int GetColumnOrder(string profile, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return 1000;
            }

            string[] allowedColumns;
            if (!ProfileColumns.TryGetValue(profile ?? AutoProfile, out allowedColumns))
            {
                return 1000;
            }

            for (var i = 0; i < allowedColumns.Length; i++)
            {
                if (string.Equals(allowedColumns[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 1000;
        }

        private static DataTemplate CreateRowDetailsTemplate()
        {
            var template = new DataTemplate();

            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.MarginProperty, new Thickness(0, 8, 0, 12));
            border.SetValue(Border.PaddingProperty, new Thickness(14, 12, 14, 6));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(248, 251, 255)));
            border.SetResourceReference(Border.BorderBrushProperty, "BorderBrushSoft");

            var items = new FrameworkElementFactory(typeof(ItemsControl));
            items.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(".") { Converter = RecordFieldsConverter.Instance });

            var itemsPanel = new ItemsPanelTemplate();
            itemsPanel.VisualTree = new FrameworkElementFactory(typeof(WrapPanel));
            items.SetValue(ItemsControl.ItemsPanelProperty, itemsPanel);
            items.SetValue(ItemsControl.ItemTemplateProperty, CreateFieldTemplate());

            border.AppendChild(items);
            template.VisualTree = border;
            return template;
        }

        private static DataTemplate CreateFieldTemplate()
        {
            var template = new DataTemplate(typeof(DetailFieldViewModel));

            var card = new FrameworkElementFactory(typeof(Border));
            card.SetValue(Border.MarginProperty, new Thickness(0, 0, 10, 10));
            card.SetValue(Border.PaddingProperty, new Thickness(10, 8, 10, 9));
            card.SetValue(Border.MinWidthProperty, 160.0);
            card.SetValue(Border.MaxWidthProperty, 320.0);
            card.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            card.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            card.SetValue(Border.BackgroundProperty, Brushes.White);
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrushSoft");

            var stack = new FrameworkElementFactory(typeof(StackPanel));

            var label = new FrameworkElementFactory(typeof(TextBlock));
            label.SetBinding(TextBlock.TextProperty, new Binding(nameof(DetailFieldViewModel.Label)));
            label.SetValue(TextBlock.FontSizeProperty, 12.0);
            label.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            label.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryBrush");

            var value = new FrameworkElementFactory(typeof(TextBlock));
            value.SetBinding(TextBlock.TextProperty, new Binding(nameof(DetailFieldViewModel.Value)));
            value.SetValue(TextBlock.MarginProperty, new Thickness(0, 4, 0, 0));
            value.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            value.SetResourceReference(TextBlock.ForegroundProperty, "TextBrush");

            stack.AppendChild(label);
            stack.AppendChild(value);
            card.AppendChild(stack);
            template.VisualTree = card;
            return template;
        }

        private static void UpdateVisibleRowDetails(DataGrid grid)
        {
            var expandedItem = GetExpandedItem(grid);
            foreach (var item in grid.Items)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    row.DetailsVisibility = Equals(item, expandedItem)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject start) where T : DependencyObject
        {
            var current = start;
            while (current != null)
            {
                var typed = current as T;
                if (typed != null)
                {
                    return typed;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static string GetAutoColumnName(DependencyObject element)
        {
            return element == null ? string.Empty : (string)element.GetValue(AutoColumnNameProperty);
        }

        private static void SetAutoColumnName(DependencyObject element, string value)
        {
            if (element != null)
            {
                element.SetValue(AutoColumnNameProperty, value ?? string.Empty);
            }
        }

        private static object GetExpandedItem(DependencyObject element)
        {
            return element == null ? null : element.GetValue(ExpandedItemProperty);
        }

        private static void SetExpandedItem(DependencyObject element, object value)
        {
            if (element != null)
            {
                element.SetValue(ExpandedItemProperty, value);
            }
        }

        private sealed class RecordFieldsConverter : IValueConverter
        {
            public static readonly RecordFieldsConverter Instance = new RecordFieldsConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var fields = new ObservableCollection<DetailFieldViewModel>();
                AddFields(fields, value);
                return fields;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }

            private static void AddFields(ICollection<DetailFieldViewModel> fields, object value)
            {
                if (fields == null || value == null)
                {
                    return;
                }

                var rowView = value as DataRowView;
                if (rowView != null)
                {
                    AddDataRowFields(fields, rowView.Row);
                    return;
                }

                var row = value as DataRow;
                if (row != null)
                {
                    AddDataRowFields(fields, row);
                    return;
                }

                AddObjectFields(fields, value);
            }

            private static void AddDataRowFields(ICollection<DetailFieldViewModel> fields, DataRow row)
            {
                if (row == null || row.Table == null)
                {
                    return;
                }

                foreach (DataColumn column in row.Table.Columns)
                {
                    if (column == null || !ShouldShowDetailField(column.ColumnName, row[column]))
                    {
                        continue;
                    }

                    fields.Add(new DetailFieldViewModel
                    {
                        Label = WpfUiDataHelper.ToFriendlyLabel(column.ColumnName),
                        Value = WpfUiDataHelper.FormatValue(row[column])
                    });
                }
            }

            private static void AddObjectFields(ICollection<DetailFieldViewModel> fields, object value)
            {
                var properties = value.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.CanRead)
                    .OrderBy(property => GetObjectPropertyOrder(property.Name))
                    .ThenBy(property => property.Name);

                foreach (var property in properties)
                {
                    object fieldValue;
                    try
                    {
                        fieldValue = property.GetValue(value, null);
                    }
                    catch
                    {
                        continue;
                    }

                    if (!ShouldShowDetailField(property.Name, fieldValue))
                    {
                        continue;
                    }

                    fields.Add(new DetailFieldViewModel
                    {
                        Label = WpfUiDataHelper.ToFriendlyLabel(property.Name),
                        Value = WpfUiDataHelper.FormatValue(fieldValue)
                    });
                }
            }

            private static bool ShouldShowDetailField(string name, object value)
            {
                if (string.IsNullOrWhiteSpace(name) || value is byte[])
                {
                    return false;
                }

                if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "Tag", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "PhotoData", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "PhotoPath", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (value == DBNull.Value)
                {
                    return true;
                }

                var type = value == null ? null : value.GetType();
                return type == null ||
                       type == typeof(string) ||
                       type.IsValueType ||
                       type == typeof(DateTime) ||
                       type == typeof(TimeSpan);
            }

            private static int GetObjectPropertyOrder(string propertyName)
            {
                var preferred = new[]
                {
                    "StudentNumber",
                    "FacultyCode",
                    "CourseCode",
                    "SubjectCode",
                    "DepartmentCode",
                    "LastName",
                    "FirstName",
                    "MiddleName",
                    "CourseName",
                    "SubjectName",
                    "DepartmentName",
                    "Gender",
                    "Units",
                    "Description"
                };

                for (var i = 0; i < preferred.Length; i++)
                {
                    if (string.Equals(preferred[i], propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return 1000;
            }
        }
    }
}
