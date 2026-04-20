using System.Collections.Generic;

namespace School_Management_System.Wpf.Views.Enrollment
{
    public sealed class CorPrintPreviewData
    {
        public CorPrintPreviewData()
        {
            SummaryRows = new List<CorPrintKeyValueRow>();
            Subjects = new List<CorPrintSubjectRow>();
            AssessmentRows = new List<CorPrintAssessmentRow>();
            Notices = new List<string>();
        }

        public IList<CorPrintKeyValueRow> SummaryRows { get; private set; }
        public IList<CorPrintSubjectRow> Subjects { get; private set; }
        public IList<CorPrintAssessmentRow> AssessmentRows { get; private set; }
        public IList<string> Notices { get; private set; }
    }

    public sealed class CorPrintKeyValueRow
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public sealed class CorPrintSubjectRow
    {
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Units { get; set; }
        public string Schedule { get; set; }
    }

    public sealed class CorPrintAssessmentRow
    {
        public string Item { get; set; }
        public string Basis { get; set; }
        public string Amount { get; set; }
        public bool IsEmphasized { get; set; }
    }
}
