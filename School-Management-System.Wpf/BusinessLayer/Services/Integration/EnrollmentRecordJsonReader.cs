using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using School_Management_System.Common;

namespace School_Management_System.BusinessLayer.Services.Integration
{
    internal sealed class EnrollmentRecordJsonReader : IEnrollmentRecordJsonReader
    {
        public EnrollmentRecordJsonData ReadFromFile(string filePath)
        {
            Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Enrollment JSON file does not exist.", filePath);
            }

            return ReadFromContent(File.ReadAllText(filePath));
        }

        public EnrollmentRecordJsonData ReadFromContent(string jsonContent)
        {
            Guard.NotNullOrWhiteSpace(jsonContent, nameof(jsonContent));

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(EnrollmentRecordJsonData));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
                {
                    var payload = serializer.ReadObject(stream) as EnrollmentRecordJsonData;
                    if (payload == null)
                    {
                        throw new InvalidDataException("Enrollment JSON did not produce an enrollment payload.");
                    }

                    if (payload.Subjects == null)
                    {
                        payload.Subjects = new List<EnrollmentSubjectJsonData>();
                    }

                    return payload;
                }
            }
            catch (SerializationException ex)
            {
                throw new InvalidDataException("Enrollment JSON is invalid or does not match the expected structure.", ex);
            }
        }
    }
}
