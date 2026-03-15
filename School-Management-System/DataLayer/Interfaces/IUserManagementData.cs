using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IUserManagementData
    {
        DataTable GetAll();
        DataTable Search(string query);
        bool UsernameExists(string username, int? excludeUserId);
        int Insert(User user);
        void Update(User user);
        void SetPassword(int userId, byte[] passwordHash, byte[] passwordSalt);
        void SetActive(int userId, bool isActive);
        byte[] GetPhotoData(int userId);
    }
}

