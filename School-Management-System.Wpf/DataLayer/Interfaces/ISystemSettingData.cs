using System.Collections.Generic;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ISystemSettingData
    {
        SystemSetting Get(string key);
        IDictionary<string, string> GetAll();
        void Set(string key, string value);
    }
}
