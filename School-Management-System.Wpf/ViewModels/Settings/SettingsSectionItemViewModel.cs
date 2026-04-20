namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class SettingsSectionItemViewModel
    {
        public SettingsSectionItemViewModel(string title, string description, string glyph, object module)
        {
            Title = title;
            Description = description;
            Glyph = glyph;
            Module = module;
        }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Glyph { get; private set; }
        public object Module { get; private set; }
    }
}
