using ContactManagerApplication.Models.Domain;

namespace ContactManagerApplication.Models.ViewModel
{
    public class CsvImportResultVM
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<Employee> ImportedEmployees { get; set; } = new();
        public List<CsvRowError> Errors { get; set; } = new();
        public string? DatabaseError { get; set; }

        public bool HasErrors => Errors.Count > 0 || DatabaseError is not null;
        public int TotalRows => SuccessCount + FailureCount;
    }

    public class CsvRowError
    {
        public int RowNumber { get; set; }

        public string? RawData { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
