using School_Management_System.Common;

namespace School_Management_System.Models
{
    public sealed class BrandingProfile
    {
        public int BrandingProfileId { get; set; }
        public string CompanyName { get; set; }
        public string ApplicationTagline { get; set; }
        public string ShellWorkspaceTagline { get; set; }
        public string DashboardTitle { get; set; }
        public string DashboardSubtitle { get; set; }
        public string LoginHeadline { get; set; }
        public string LoginBody { get; set; }
        public string LoginFormTitle { get; set; }
        public string LoginFormSubtitle { get; set; }
        public string FeatureOneTitle { get; set; }
        public string FeatureOneBody { get; set; }
        public string FeatureTwoTitle { get; set; }
        public string FeatureTwoBody { get; set; }
        public string FeatureThreeTitle { get; set; }
        public string FeatureThreeBody { get; set; }
        public string ClientSerialNumber { get; set; }
        public string SupportEmail { get; set; }
        public string SupportPhoneNumber { get; set; }
        public string CompanyAddress { get; set; }
        public byte[] BrandLogoData { get; set; }
        public byte[] CompactLogoData { get; set; }

        public static BrandingProfile CreateDefault()
        {
            return new BrandingProfile
            {
                BrandingProfileId = BrandingDefaults.ProfileId,
                CompanyName = BrandingDefaults.CompanyName,
                ApplicationTagline = BrandingDefaults.ApplicationTagline,
                ShellWorkspaceTagline = BrandingDefaults.ShellWorkspaceTagline,
                DashboardTitle = BrandingDefaults.DashboardTitle,
                DashboardSubtitle = BrandingDefaults.DashboardSubtitle,
                LoginHeadline = BrandingDefaults.LoginHeadline,
                LoginBody = BrandingDefaults.LoginBody,
                LoginFormTitle = BrandingDefaults.LoginFormTitle,
                LoginFormSubtitle = BrandingDefaults.LoginFormSubtitle,
                FeatureOneTitle = BrandingDefaults.FeatureOneTitle,
                FeatureOneBody = BrandingDefaults.FeatureOneBody,
                FeatureTwoTitle = BrandingDefaults.FeatureTwoTitle,
                FeatureTwoBody = BrandingDefaults.FeatureTwoBody,
                FeatureThreeTitle = BrandingDefaults.FeatureThreeTitle,
                FeatureThreeBody = BrandingDefaults.FeatureThreeBody,
                ClientSerialNumber = BrandingDefaults.ClientSerialNumber,
                SupportEmail = BrandingDefaults.SupportEmail,
                SupportPhoneNumber = BrandingDefaults.SupportPhoneNumber,
                CompanyAddress = BrandingDefaults.CompanyAddress
            };
        }
    }
}
