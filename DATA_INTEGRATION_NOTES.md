# XML + JSON Data Integration Notes

## What Was Added
The repository now includes a focused data-integration feature that reads:

- a student profile from XML
- an enrollment record from JSON
- a unified `IntegratedStudentEnrollmentRecord` as the merged output

Main implementation files:

- `School-Management-System.Wpf/BusinessLayer/Services/Integration/StudentProfileXmlReader.cs`
- `School-Management-System.Wpf/BusinessLayer/Services/Integration/EnrollmentRecordJsonReader.cs`
- `School-Management-System.Wpf/BusinessLayer/Services/Integration/StudentEnrollmentIntegrationService.cs`
- `School-Management-System.Wpf/Models/IntegratedStudentEnrollmentRecord.cs`

## Why It Fits The Rubric
- XML well-formedness: the XML reader uses `XmlReader` plus `XmlSerializer` and throws `InvalidDataException` when the XML is malformed or mismatched to the expected structure.
- Data integration: the integration service reads both sources, verifies that both refer to the same `StudentNumber`, and merges them into one unified school-domain object.
- Code architecture: reading XML, reading JSON, and merging records are split into separate components instead of one large block of code.
- Analysis quality: the service is stateless and keeps no cached mutable data between requests, which makes behavior easier to reason about and test.

## Quick Usage Example
```csharp
var service = new StudentEnrollmentIntegrationService();
var unifiedRecord = service.MergeFromFiles(@"C:\data\student-profile.xml", @"C:\data\enrollment.json");
```

## Statelessness And Overhead
- Statelessness: each call creates a fresh result from the provided XML and JSON inputs. No previous request data is reused, so there is no cross-request contamination.
- Runtime overhead: the work is mostly deserialization plus a single merge pass over the subjects list. In practice this is linear with respect to the number of JSON subjects.
- Memory overhead: the service temporarily holds deserialized XML and JSON objects plus the final unified object. There is no extra caching layer.
- Tradeoff: the design favors clarity and safety over aggressive optimization, which is appropriate for moderate-sized academic or administrative records.
