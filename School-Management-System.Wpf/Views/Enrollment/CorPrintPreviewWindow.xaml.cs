using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.Views.Enrollment
{
    public partial class CorPrintPreviewWindow : Window
    {
        private const double A4Width = 793.700787;
        private const double A4Height = 1122.519685;
        private const double PagePadding = 48;
        private readonly CorPrintPreviewData _corData;

        public CorPrintPreviewWindow(CorPrintPreviewData corData)
        {
            _corData = corData ?? new CorPrintPreviewData();
            InitializeComponent();
            PreviewViewer.Document = CreateDocument(A4Width, A4Height);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PrintButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new PrintDialog();
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var documentWidth = dialog.PrintableAreaWidth > 0
                    ? Math.Min(dialog.PrintableAreaWidth, A4Width)
                    : A4Width;
                var documentHeight = dialog.PrintableAreaHeight > 0
                    ? Math.Min(dialog.PrintableAreaHeight, A4Height)
                    : A4Height;

                var document = CreateDocument(documentWidth, documentHeight);
                var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                paginator.PageSize = new Size(documentWidth, documentHeight);
                dialog.PrintDocument(paginator, "Certificate of Registration");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to print the Certificate of Registration.\n\n" + ex.Message,
                    "Print COR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateDocument(double pageWidth, double pageHeight)
        {
            var safePageWidth = !double.IsNaN(pageWidth) && pageWidth > 0 ? pageWidth : A4Width;
            var safePageHeight = !double.IsNaN(pageHeight) && pageHeight > 0 ? pageHeight : A4Height;
            var contentWidth = Math.Max(0, safePageWidth - (PagePadding * 2));

            var document = new FlowDocument
            {
                PagePadding = new Thickness(PagePadding),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11.5,
                Foreground = Brushes.Black,
                ColumnGap = 0
            };

            document.PageWidth = safePageWidth;
            document.PageHeight = safePageHeight;
            document.ColumnWidth = contentWidth;

            document.Blocks.Add(BuildBrandHeaderBlock(contentWidth));

            var titleParagraph = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, 12),
                TextAlignment = TextAlignment.Center
            };
            titleParagraph.Inlines.Add(new Run("CERTIFICATE OF REGISTRATION / ASSESSMENT")
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 18,
                FontWeight = FontWeights.Bold
            });
            document.Blocks.Add(titleParagraph);
            document.Blocks.Add(BuildSectionHeading("Registration Details"));
            document.Blocks.Add(BuildSummaryTable(_corData));
            document.Blocks.Add(BuildSectionHeading("Enrolled Subjects"));
            document.Blocks.Add(BuildSubjectsTable(_corData));
            document.Blocks.Add(BuildSectionHeading("Assessment"));
            document.Blocks.Add(BuildAssessmentTable(_corData));

            if (_corData.Notices.Count > 0)
            {
                document.Blocks.Add(BuildNoticePanel(_corData));
            }

            return document;
        }

        private static Paragraph BuildSectionHeading(string title)
        {
            var heading = new Paragraph
            {
                Margin = new Thickness(0, 6, 0, 8)
            };
            heading.Inlines.Add(new Run(title ?? string.Empty)
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 74, 128))
            });
            return heading;
        }

        private static Table BuildSummaryTable(CorPrintPreviewData data)
        {
            var table = CreateTable(190, 0);
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            if (data == null || data.SummaryRows.Count == 0)
            {
                rowGroup.Rows.Add(CreateSingleCellRow("No registration details available.", 2));
                return table;
            }

            for (var index = 0; index < data.SummaryRows.Count; index++)
            {
                var rowData = data.SummaryRows[index];
                var row = new TableRow();
                var isStripe = index % 2 == 1;
                row.Cells.Add(CreateCell(rowData.Label, true, isStripe, TextAlignment.Left));
                row.Cells.Add(CreateCell(rowData.Value, false, isStripe, TextAlignment.Left));
                rowGroup.Rows.Add(row);
            }

            return table;
        }

        private static Table BuildSubjectsTable(CorPrintPreviewData data)
        {
            var table = CreateTable(110, 0, 90, 220);
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            rowGroup.Rows.Add(CreateHeaderRow("Code", "Subject", "Units", "Schedule"));

            if (data == null || data.Subjects.Count == 0)
            {
                rowGroup.Rows.Add(CreateSingleCellRow("No enrolled subjects selected.", 4));
                return table;
            }

            for (var index = 0; index < data.Subjects.Count; index++)
            {
                var subject = data.Subjects[index];
                var isStripe = index % 2 == 1;
                var row = new TableRow();
                row.Cells.Add(CreateCell(subject.Code, false, isStripe, TextAlignment.Left));
                row.Cells.Add(CreateCell(subject.Subject, false, isStripe, TextAlignment.Left));
                row.Cells.Add(CreateCell(subject.Units, false, isStripe, TextAlignment.Center));
                row.Cells.Add(CreateCell(subject.Schedule, false, isStripe, TextAlignment.Left));
                rowGroup.Rows.Add(row);
            }

            return table;
        }

        private static Table BuildAssessmentTable(CorPrintPreviewData data)
        {
            var table = CreateTable(190, 230, 120);
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            rowGroup.Rows.Add(CreateHeaderRow("Item", "Basis", "Amount"));

            if (data == null || data.AssessmentRows.Count == 0)
            {
                rowGroup.Rows.Add(CreateSingleCellRow("No assessment details available.", 3));
                return table;
            }

            for (var index = 0; index < data.AssessmentRows.Count; index++)
            {
                var assessment = data.AssessmentRows[index];
                var isStripe = index % 2 == 1;
                var row = new TableRow();
                row.Cells.Add(CreateCell(assessment.Item, assessment.IsEmphasized, isStripe, TextAlignment.Left));
                row.Cells.Add(CreateCell(assessment.Basis, assessment.IsEmphasized, isStripe, TextAlignment.Left));
                row.Cells.Add(CreateCell(assessment.Amount, assessment.IsEmphasized, isStripe, TextAlignment.Right));
                rowGroup.Rows.Add(row);
            }

            return table;
        }

        private static Table CreateTable(params double[] widths)
        {
            var table = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 16)
            };

            foreach (var width in widths)
            {
                if (width <= 0)
                {
                    table.Columns.Add(new TableColumn());
                    continue;
                }

                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(width)
                });
            }

            return table;
        }

        private static TableRow CreateHeaderRow(params string[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                row.Cells.Add(CreateCell(value, true, false, TextAlignment.Left, true));
            }

            return row;
        }

        private static TableRow CreateSingleCellRow(string text, int columnSpan)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty)))
            {
                ColumnSpan = columnSpan,
                Padding = new Thickness(10, 8, 10, 8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(214, 224, 236)),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromRgb(250, 252, 255))
            });
            return row;
        }

        private static TableCell CreateCell(string text, bool isBold, bool isStripe, TextAlignment alignment, bool isHeader = false)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0),
                TextAlignment = alignment
            };
            paragraph.Inlines.Add(new Run(text ?? string.Empty)
            {
                FontWeight = isBold ? FontWeights.SemiBold : FontWeights.Normal
            });

            return new TableCell(paragraph)
            {
                Padding = new Thickness(10, 7, 10, 7),
                BorderBrush = new SolidColorBrush(Color.FromRgb(214, 224, 236)),
                BorderThickness = new Thickness(1),
                Background = isHeader
                    ? new SolidColorBrush(Color.FromRgb(232, 242, 255))
                    : new SolidColorBrush(isStripe ? Color.FromRgb(250, 252, 255) : Colors.White)
            };
        }

        private static BlockUIContainer BuildNoticePanel(CorPrintPreviewData data)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 4, 0, 0)
            };

            panel.Children.Add(new TextBlock
            {
                Text = "Print Notes",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 74, 128)),
                Margin = new Thickness(0, 0, 0, 6)
            });

            for (var index = 0; index < data.Notices.Count; index++)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "\u2022 " + data.Notices[index],
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 11.5,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }

            return new BlockUIContainer(new Border
            {
                Padding = new Thickness(12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(214, 224, 236)),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromRgb(247, 250, 255)),
                Child = panel
            })
            {
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        private static BlockUIContainer BuildBrandHeaderBlock(double contentWidth)
        {
            var header = new StackPanel
            {
                Width = contentWidth,
                Margin = new Thickness(0, 0, 0, 18)
            };

            var logo = LoadCorBrandLogo();
            if (logo != null)
            {
                header.Children.Add(new Image
                {
                    Source = logo,
                    Height = 86,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                });
            }

            header.Children.Add(new TextBlock
            {
                Text = SchoolBranding.ApplicationTitle,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 74, 128)),
                TextAlignment = TextAlignment.Center
            });

            header.Children.Add(new TextBlock
            {
                Text = "Official Certificate of Registration",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            });

            header.Children.Add(new TextBlock
            {
                Text = SchoolBranding.ShellWorkspaceTagline,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11.5,
                Foreground = Brushes.DimGray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            });

            header.Children.Add(new Border
            {
                Height = 2,
                Margin = new Thickness(0, 14, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219))
            });

            return new BlockUIContainer(header)
            {
                Margin = new Thickness(0)
            };
        }

        private static ImageSource LoadCorBrandLogo()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            var candidates = new[]
            {
                Path.Combine(baseDir, "assets", "newbranding.png"),
                Path.Combine(baseDir, "assets", "brand-logo.png"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "newbranding.png"))
            };

            for (var index = 0; index < candidates.Length; index++)
            {
                var image = TryLoadImage(candidates[index]);
                if (image != null)
                {
                    return image;
                }
            }

            return BrandingAssetLoader.LoadBrandLogo();
        }

        private static ImageSource TryLoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}
