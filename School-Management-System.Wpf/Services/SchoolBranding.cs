using System;
using School_Management_System.Common;
using School_Management_System.Models;

namespace School_Management_System.Wpf.Services
{
    public static class SchoolBranding
    {
        private static BrandingProfile _current = BrandingProfile.CreateDefault();

        public static event EventHandler BrandingChanged;

        public static BrandingProfile Current
        {
            get { return _current; }
        }

        public static void Apply(BrandingProfile profile)
        {
            _current = profile ?? BrandingProfile.CreateDefault();

            var handler = BrandingChanged;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        public static void Reset()
        {
            Apply(BrandingProfile.CreateDefault());
        }

        public static string ApplicationTitle
        {
            get { return Current.CompanyName; }
        }

        public static string ApplicationTagline
        {
            get { return Current.ApplicationTagline; }
        }

        public static string ShellWorkspaceTagline
        {
            get { return Current.ShellWorkspaceTagline; }
        }

        public static string DashboardTitle
        {
            get { return Current.DashboardTitle; }
        }

        public static string DashboardSubtitle
        {
            get { return Current.DashboardSubtitle; }
        }

        public static string LoginHeadline
        {
            get { return Current.LoginHeadline; }
        }

        public static string LoginBody
        {
            get { return Current.LoginBody; }
        }

        public static string LoginFormTitle
        {
            get { return Current.LoginFormTitle; }
        }

        public static string LoginFormSubtitle
        {
            get { return Current.LoginFormSubtitle; }
        }

        public static string FeatureOneTitle
        {
            get { return Current.FeatureOneTitle; }
        }

        public static string FeatureOneBody
        {
            get { return Current.FeatureOneBody; }
        }

        public static string FeatureTwoTitle
        {
            get { return Current.FeatureTwoTitle; }
        }

        public static string FeatureTwoBody
        {
            get { return Current.FeatureTwoBody; }
        }

        public static string FeatureThreeTitle
        {
            get { return Current.FeatureThreeTitle; }
        }

        public static string FeatureThreeBody
        {
            get { return Current.FeatureThreeBody; }
        }

        public static string ClientSerialNumber
        {
            get { return Current.ClientSerialNumber; }
        }

        public static string SupportEmail
        {
            get { return Current.SupportEmail; }
        }

        public static string SupportPhoneNumber
        {
            get { return Current.SupportPhoneNumber; }
        }

        public static string CompanyAddress
        {
            get { return Current.CompanyAddress; }
        }

        public static byte[] BrandLogoData
        {
            get { return Current.BrandLogoData; }
        }

        public static byte[] CompactLogoData
        {
            get { return Current.CompactLogoData; }
        }
    }
}
