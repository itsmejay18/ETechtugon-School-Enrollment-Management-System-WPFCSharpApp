namespace School_Management_System.BusinessLayer.Services.Integration
{
    internal interface IStudentProfileXmlReader
    {
        StudentProfileXmlData ReadFromFile(string filePath);
        StudentProfileXmlData ReadFromContent(string xmlContent);
    }
}
