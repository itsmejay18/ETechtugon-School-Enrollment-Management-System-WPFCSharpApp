namespace School_Management_System.Wpf.ViewModels.Shared
{
    public sealed class LookupOptionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public object Tag { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Subtitle))
                {
                    return Title ?? string.Empty;
                }

                return (Title ?? string.Empty) + " | " + Subtitle;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
