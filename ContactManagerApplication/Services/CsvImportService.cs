using System.Globalization;
using System.Text;
using ContactManagerApplication.Models.Domain;
using ContactManagerApplication.Models.ViewModel;
using ContactManagerApplication.Repositories;

namespace ContactManagerApplication.Services
{
    public class CsvImportService : ICsvImportService
    {
        private readonly IEmployeeRepository _repository;

        private static readonly string[] DateFormats =
            { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd" };

        public CsvImportService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<CsvImportResultVM> ImportAsync(Stream csvStream)
        {
            var result = new CsvImportResultVM();
            var validEmployees = new List<Employee>();

            using var reader = new StreamReader(csvStream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                result.Errors.Add(new CsvRowError
                {
                    RowNumber = 0,
                    ErrorMessage = "The CSV file is empty or contains no header row."
                });
                result.FailureCount = result.Errors.Count;
                return result;
            }

            Dictionary<string, int> columnMap;
            try
            {
                columnMap = BuildColumnMap(headerLine);
                ValidateRequiredColumns(columnMap);
            }
            catch (InvalidOperationException ex)
            {
                result.Errors.Add(new CsvRowError { RowNumber = 0, ErrorMessage = ex.Message });
                result.FailureCount = result.Errors.Count;
                return result;
            }

            int rowNumber = 1;
            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var fields = SplitCsvLine(line);
                    var employee = MapToEmployee(fields, columnMap);
                    validEmployees.Add(employee);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new CsvRowError
                    {
                        RowNumber = rowNumber,
                        RawData = line.Length > 200 ? line[..200] + "…" : line,
                        ErrorMessage = ex.Message
                    });
                }
            }

            if (validEmployees.Count > 0)
            {
                try
                {
                    result.SuccessCount = await _repository.CreateBulkAsync(validEmployees);
                    result.ImportedEmployees = validEmployees;
                }
                catch (Exception ex)
                {
                    result.DatabaseError =
                        $"All {validEmployees.Count} parsed record(s) could not be saved: {ex.Message}";
                    result.Errors.Add(new CsvRowError
                    {
                        RowNumber = 0,
                        ErrorMessage = result.DatabaseError
                    });
                    validEmployees.ForEach(_ => result.FailureCount++);
                }
            }

            result.FailureCount += result.Errors.Count(e => e.RowNumber > 0);

            return result;
        }

        private static Dictionary<string, int> BuildColumnMap(string headerLine)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var columns = SplitCsvLine(headerLine);
            for (int i = 0; i < columns.Length; i++)
            {
                var name = columns[i].Trim();
                if (!string.IsNullOrEmpty(name))
                    map[name] = i;
            }
            return map;
        }

        private static void ValidateRequiredColumns(Dictionary<string, int> columnMap)
        {
            string[] required = ["Name", "Salary"];
            var missing = required.Where(c => !columnMap.ContainsKey(c)).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"Missing required column(s): {string.Join(", ", missing)}. " +
                    $"The CSV must contain at least: Name, Salary.");
        }

        private static Employee MapToEmployee(string[] fields, Dictionary<string, int> columnMap)
        {
            string GetField(string name) =>
                columnMap.TryGetValue(name, out var idx) && idx < fields.Length
                    ? fields[idx].Trim()
                    : string.Empty;

            var name = GetField("Name");
            if (string.IsNullOrWhiteSpace(name))
                throw new FormatException("Name is required and cannot be empty.");

            var salaryStr = GetField("Salary");
            if (!decimal.TryParse(salaryStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var salary))
                throw new FormatException(
                    $"Invalid Salary value \"{salaryStr}\". Expected a numeric value (e.g. 50000 or 50000.00).");

            if (salary < 0)
                throw new FormatException("Salary cannot be negative.");

            DateOnly? birthDay = null;
            var birthDayStr = GetField("BirthDay");
            if (!string.IsNullOrEmpty(birthDayStr))
            {
                if (!DateOnly.TryParseExact(birthDayStr, DateFormats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var bd))
                    throw new FormatException(
                        $"Invalid BirthDay value \"{birthDayStr}\". " +
                        "Supported formats: yyyy-MM-dd, dd/MM/yyyy, MM/dd/yyyy.");
                birthDay = bd;
            }

            bool? isMarried = null;
            var isMarriedStr = GetField("IsMarried").ToLowerInvariant();
            if (!string.IsNullOrEmpty(isMarriedStr))
            {
                if (isMarriedStr is "true" or "yes" or "1")
                    isMarried = true;
                else if (isMarriedStr is "false" or "no" or "0")
                    isMarried = false;
                else
                    throw new FormatException(
                        $"Invalid IsMarried value \"{GetField("IsMarried")}\". " +
                        "Accepted values: true/false, yes/no, 1/0.");
            }

            var phone = GetField("Phone");

            return new Employee
            {
                Id       = Guid.NewGuid(),
                Name     = name,
                BirthDay = birthDay,
                IsMarried = isMarried,
                Phone    = string.IsNullOrEmpty(phone) ? null : phone,
                Salary   = salary
            };
        }

        private static string[] SplitCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString().Trim());
            return [.. fields];
        }
    }
}
