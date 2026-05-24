namespace School_Management_System.BusinessLayer.Services.Integration
{
    internal interface IEnrollmentRecordJsonReader
    {
        EnrollmentRecordJsonData ReadFromFile(string filePath);
        EnrollmentRecordJsonData ReadFromContent(string jsonContent);
    }
}
