using System;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class BrandingProfileService
    {
        private readonly IBrandingProfileData _data;

        public BrandingProfileService(IBrandingProfileData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public BrandingProfile GetCurrent()
        {
            return MergeWithDefaults(_data.GetCurrent());
        }

        public BrandingProfile Save(BrandingProfile profile)
        {
            var normalized = MergeWithDefaults(profile);
            _data.Save(normalized);
            return normalized;
        }

        private static BrandingProfile MergeWithDefaults(BrandingProfile profile)
        {
            var merged = profile ?? BrandingProfile.CreateDefault();

            merged.BrandingProfileId = merged.BrandingProfileId <= 0 ? BrandingDefaults.ProfileId : merged.BrandingProfileId;
            merged.CompanyName = NormalizeText(merged.CompanyName, BrandingDefaults.CompanyName);
            merged.ApplicationTagline = NormalizeText(merged.ApplicationTagline, BrandingDefaults.ApplicationTagline);
            merged.ShellWorkspaceTagline = NormalizeText(merged.ShellWorkspaceTagline, BrandingDefaults.ShellWorkspaceTagline);
            merged.DashboardTitle = NormalizeText(merged.DashboardTitle, BrandingDefaults.DashboardTitle);
            merged.DashboardSubtitle = NormalizeText(merged.DashboardSubtitle, BrandingDefaults.DashboardSubtitle);
            merged.LoginHeadline = NormalizeText(merged.LoginHeadline, BrandingDefaults.LoginHeadline);
            merged.LoginBody = NormalizeText(merged.LoginBody, BrandingDefaults.LoginBody);
            merged.LoginFormTitle = NormalizeText(merged.LoginFormTitle, BrandingDefaults.LoginFormTitle);
            merged.LoginFormSubtitle = NormalizeText(merged.LoginFormSubtitle, BrandingDefaults.LoginFormSubtitle);
            merged.FeatureOneTitle = NormalizeText(merged.FeatureOneTitle, BrandingDefaults.FeatureOneTitle);
            merged.FeatureOneBody = NormalizeText(merged.FeatureOneBody, BrandingDefaults.FeatureOneBody);
            merged.FeatureTwoTitle = NormalizeText(merged.FeatureTwoTitle, BrandingDefaults.FeatureTwoTitle);
            merged.FeatureTwoBody = NormalizeText(merged.FeatureTwoBody, BrandingDefaults.FeatureTwoBody);
            merged.FeatureThreeTitle = NormalizeText(merged.FeatureThreeTitle, BrandingDefaults.FeatureThreeTitle);
            merged.FeatureThreeBody = NormalizeText(merged.FeatureThreeBody, BrandingDefaults.FeatureThreeBody);
            merged.ClientSerialNumber = NormalizeText(merged.ClientSerialNumber, BrandingDefaults.ClientSerialNumber);
            merged.SupportEmail = NormalizeText(merged.SupportEmail, BrandingDefaults.SupportEmail);
            merged.SupportPhoneNumber = NormalizeText(merged.SupportPhoneNumber, BrandingDefaults.SupportPhoneNumber);
            merged.CompanyAddress = NormalizeText(merged.CompanyAddress, BrandingDefaults.CompanyAddress);
            merged.BrandLogoData = NormalizeBytes(merged.BrandLogoData);
            merged.CompactLogoData = NormalizeBytes(merged.CompactLogoData);

            return merged;
        }

        private static string NormalizeText(string value, string fallback)
        {
            var safe = string.IsNullOrWhiteSpace(value) ? (fallback ?? string.Empty) : value.Trim();
            return safe;
        }

        private static byte[] NormalizeBytes(byte[] value)
        {
            return value == null || value.Length == 0 ? null : value;
        }
    }
}
