namespace School_Management_System.Models
{
    public sealed class SystemSetting
    {
        private string _settingKey;
        private string _settingValue;

        public string SettingKey { get { return _settingKey; } set { _settingKey = value; } }
        public string SettingValue { get { return _settingValue; } set { _settingValue = value; } }
    }
}
