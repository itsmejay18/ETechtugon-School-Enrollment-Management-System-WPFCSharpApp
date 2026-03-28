namespace School_Management_System.Wpf.ViewModels
{
    public sealed class ModulePlaceholderViewModel : Infrastructure.ViewModelBase
    {
        public ModulePlaceholderViewModel(string title, string description, string note)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            Note = note ?? string.Empty;
        }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Note { get; private set; }
    }
}
