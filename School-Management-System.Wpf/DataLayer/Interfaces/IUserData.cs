using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IUserData
    {
        User GetByUsername(string username);
        void UpdateLastLogin(int userId);
    }
}

