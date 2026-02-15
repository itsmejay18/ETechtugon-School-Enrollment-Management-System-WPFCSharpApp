using System;
using System.Windows.Forms;

namespace School_Management_System.Presentation.Helpers
{
    public static class UserPreferences
    {
        private const string RememberMeKey = "RememberMe";
        private const string RememberedUsernameKey = "RememberedUsername";

        public static bool RememberMe
        {
            get
            {
                try
                {
                    var value = Application.UserAppDataRegistry.GetValue(RememberMeKey);
                    if (value == null) return false;
                    return Convert.ToBoolean(value);
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    Application.UserAppDataRegistry.SetValue(RememberMeKey, value);
                }
                catch
                {
                    // ignore
                }
            }
        }

        public static string RememberedUsername
        {
            get
            {
                try
                {
                    var value = Application.UserAppDataRegistry.GetValue(RememberedUsernameKey);
                    return value == null ? null : Convert.ToString(value);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Application.UserAppDataRegistry.DeleteValue(RememberedUsernameKey, false);
                    }
                    else
                    {
                        Application.UserAppDataRegistry.SetValue(RememberedUsernameKey, value.Trim());
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}

