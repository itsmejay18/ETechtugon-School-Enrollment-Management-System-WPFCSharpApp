using System;
using System.Data;
using MySqlConnector;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class BrandingProfileData : IBrandingProfileData
    {
        private readonly DatabaseHelper _db;

        public BrandingProfileData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public BrandingProfile GetCurrent()
        {
            const string sql = @"
SELECT
    BrandingProfileId,
    CompanyName,
    ApplicationTagline,
    ShellWorkspaceTagline,
    DashboardTitle,
    DashboardSubtitle,
    LoginHeadline,
    LoginBody,
    LoginFormTitle,
    LoginFormSubtitle,
    FeatureOneTitle,
    FeatureOneBody,
    FeatureTwoTitle,
    FeatureTwoBody,
    FeatureThreeTitle,
    FeatureThreeBody,
    ClientSerialNumber,
    SupportEmail,
    SupportPhoneNumber,
    CompanyAddress,
    BrandLogoData,
    CompactLogoData
FROM `brandingprofile`
ORDER BY BrandingProfileId
LIMIT 1;";

            var table = _db.ExecuteDataTable(sql, CommandType.Text, null);
            if (table.Rows.Count == 0)
            {
                return null;
            }

            var row = table.Rows[0];
            return new BrandingProfile
            {
                BrandingProfileId = Convert.ToInt32(row["BrandingProfileId"]),
                CompanyName = Convert.ToString(row["CompanyName"]),
                ApplicationTagline = Convert.ToString(row["ApplicationTagline"]),
                ShellWorkspaceTagline = Convert.ToString(row["ShellWorkspaceTagline"]),
                DashboardTitle = Convert.ToString(row["DashboardTitle"]),
                DashboardSubtitle = Convert.ToString(row["DashboardSubtitle"]),
                LoginHeadline = Convert.ToString(row["LoginHeadline"]),
                LoginBody = Convert.ToString(row["LoginBody"]),
                LoginFormTitle = Convert.ToString(row["LoginFormTitle"]),
                LoginFormSubtitle = Convert.ToString(row["LoginFormSubtitle"]),
                FeatureOneTitle = Convert.ToString(row["FeatureOneTitle"]),
                FeatureOneBody = Convert.ToString(row["FeatureOneBody"]),
                FeatureTwoTitle = Convert.ToString(row["FeatureTwoTitle"]),
                FeatureTwoBody = Convert.ToString(row["FeatureTwoBody"]),
                FeatureThreeTitle = Convert.ToString(row["FeatureThreeTitle"]),
                FeatureThreeBody = Convert.ToString(row["FeatureThreeBody"]),
                ClientSerialNumber = Convert.ToString(row["ClientSerialNumber"]),
                SupportEmail = Convert.ToString(row["SupportEmail"]),
                SupportPhoneNumber = Convert.ToString(row["SupportPhoneNumber"]),
                CompanyAddress = Convert.ToString(row["CompanyAddress"]),
                BrandLogoData = row["BrandLogoData"] == DBNull.Value ? null : (byte[])row["BrandLogoData"],
                CompactLogoData = row["CompactLogoData"] == DBNull.Value ? null : (byte[])row["CompactLogoData"]
            };
        }

        public void Save(BrandingProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            const string sql = @"
INSERT INTO `brandingprofile`
(
    BrandingProfileId,
    CompanyName,
    ApplicationTagline,
    ShellWorkspaceTagline,
    DashboardTitle,
    DashboardSubtitle,
    LoginHeadline,
    LoginBody,
    LoginFormTitle,
    LoginFormSubtitle,
    FeatureOneTitle,
    FeatureOneBody,
    FeatureTwoTitle,
    FeatureTwoBody,
    FeatureThreeTitle,
    FeatureThreeBody,
    ClientSerialNumber,
    SupportEmail,
    SupportPhoneNumber,
    CompanyAddress,
    BrandLogoData,
    CompactLogoData,
    UpdatedAt
)
VALUES
(
    @BrandingProfileId,
    @CompanyName,
    @ApplicationTagline,
    @ShellWorkspaceTagline,
    @DashboardTitle,
    @DashboardSubtitle,
    @LoginHeadline,
    @LoginBody,
    @LoginFormTitle,
    @LoginFormSubtitle,
    @FeatureOneTitle,
    @FeatureOneBody,
    @FeatureTwoTitle,
    @FeatureTwoBody,
    @FeatureThreeTitle,
    @FeatureThreeBody,
    @ClientSerialNumber,
    @SupportEmail,
    @SupportPhoneNumber,
    @CompanyAddress,
    @BrandLogoData,
    @CompactLogoData,
    UTC_TIMESTAMP()
)
ON DUPLICATE KEY UPDATE
    CompanyName = VALUES(CompanyName),
    ApplicationTagline = VALUES(ApplicationTagline),
    ShellWorkspaceTagline = VALUES(ShellWorkspaceTagline),
    DashboardTitle = VALUES(DashboardTitle),
    DashboardSubtitle = VALUES(DashboardSubtitle),
    LoginHeadline = VALUES(LoginHeadline),
    LoginBody = VALUES(LoginBody),
    LoginFormTitle = VALUES(LoginFormTitle),
    LoginFormSubtitle = VALUES(LoginFormSubtitle),
    FeatureOneTitle = VALUES(FeatureOneTitle),
    FeatureOneBody = VALUES(FeatureOneBody),
    FeatureTwoTitle = VALUES(FeatureTwoTitle),
    FeatureTwoBody = VALUES(FeatureTwoBody),
    FeatureThreeTitle = VALUES(FeatureThreeTitle),
    FeatureThreeBody = VALUES(FeatureThreeBody),
    ClientSerialNumber = VALUES(ClientSerialNumber),
    SupportEmail = VALUES(SupportEmail),
    SupportPhoneNumber = VALUES(SupportPhoneNumber),
    CompanyAddress = VALUES(CompanyAddress),
    BrandLogoData = VALUES(BrandLogoData),
    CompactLogoData = VALUES(CompactLogoData),
    UpdatedAt = UTC_TIMESTAMP();";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@BrandingProfileId", profile.BrandingProfileId),
                    new MySqlParameter("@CompanyName", (object)profile.CompanyName ?? DBNull.Value),
                    new MySqlParameter("@ApplicationTagline", (object)profile.ApplicationTagline ?? DBNull.Value),
                    new MySqlParameter("@ShellWorkspaceTagline", (object)profile.ShellWorkspaceTagline ?? DBNull.Value),
                    new MySqlParameter("@DashboardTitle", (object)profile.DashboardTitle ?? DBNull.Value),
                    new MySqlParameter("@DashboardSubtitle", (object)profile.DashboardSubtitle ?? DBNull.Value),
                    new MySqlParameter("@LoginHeadline", (object)profile.LoginHeadline ?? DBNull.Value),
                    new MySqlParameter("@LoginBody", (object)profile.LoginBody ?? DBNull.Value),
                    new MySqlParameter("@LoginFormTitle", (object)profile.LoginFormTitle ?? DBNull.Value),
                    new MySqlParameter("@LoginFormSubtitle", (object)profile.LoginFormSubtitle ?? DBNull.Value),
                    new MySqlParameter("@FeatureOneTitle", (object)profile.FeatureOneTitle ?? DBNull.Value),
                    new MySqlParameter("@FeatureOneBody", (object)profile.FeatureOneBody ?? DBNull.Value),
                    new MySqlParameter("@FeatureTwoTitle", (object)profile.FeatureTwoTitle ?? DBNull.Value),
                    new MySqlParameter("@FeatureTwoBody", (object)profile.FeatureTwoBody ?? DBNull.Value),
                    new MySqlParameter("@FeatureThreeTitle", (object)profile.FeatureThreeTitle ?? DBNull.Value),
                    new MySqlParameter("@FeatureThreeBody", (object)profile.FeatureThreeBody ?? DBNull.Value),
                    new MySqlParameter("@ClientSerialNumber", (object)profile.ClientSerialNumber ?? DBNull.Value),
                    new MySqlParameter("@SupportEmail", (object)profile.SupportEmail ?? DBNull.Value),
                    new MySqlParameter("@SupportPhoneNumber", (object)profile.SupportPhoneNumber ?? DBNull.Value),
                    new MySqlParameter("@CompanyAddress", (object)profile.CompanyAddress ?? DBNull.Value),
                    new MySqlParameter("@BrandLogoData", (object)profile.BrandLogoData ?? DBNull.Value),
                    new MySqlParameter("@CompactLogoData", (object)profile.CompactLogoData ?? DBNull.Value)
                });
        }
    }
}
