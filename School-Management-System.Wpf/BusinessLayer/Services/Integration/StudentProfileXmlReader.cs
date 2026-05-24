using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using School_Management_System.Common;

namespace School_Management_System.BusinessLayer.Services.Integration
{
    internal sealed class StudentProfileXmlReader : IStudentProfileXmlReader
    {
        public StudentProfileXmlData ReadFromFile(string filePath)
        {
            Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Student XML file does not exist.", filePath);
            }

            return ReadFromContent(File.ReadAllText(filePath));
        }

        public StudentProfileXmlData ReadFromContent(string xmlContent)
        {
            Guard.NotNullOrWhiteSpace(xmlContent, nameof(xmlContent));

            try
            {
                var serializer = new XmlSerializer(typeof(StudentProfileXmlData));
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    IgnoreComments = true,
                    IgnoreWhitespace = true
                };

                using (var stringReader = new StringReader(xmlContent))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    var payload = serializer.Deserialize(xmlReader) as StudentProfileXmlData;
                    if (payload == null)
                    {
                        throw new InvalidDataException("Student XML did not produce a student profile payload.");
                    }

                    return payload;
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidDataException("Student XML is not well-formed or does not match the expected profile structure.", ex.InnerException ?? ex);
            }
            catch (XmlException ex)
            {
                throw new InvalidDataException("Student XML is not well-formed or does not match the expected profile structure.", ex);
            }
        }
    }
}
