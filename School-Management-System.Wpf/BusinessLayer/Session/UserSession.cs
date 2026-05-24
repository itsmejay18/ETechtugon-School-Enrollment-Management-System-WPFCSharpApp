using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Session
{
    public static class UserSession
    {
        private static User _currentUser;

        public static User CurrentUser
        {
            get { return _currentUser; }
        }

        public static void Start(User user)
        {
            _currentUser = user;
        }

        public static void End()
        {
            _currentUser = null;
        }
    }
}

