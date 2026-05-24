using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IBrandingProfileData
    {
        BrandingProfile GetCurrent();
        void Save(BrandingProfile profile);
    }
}
