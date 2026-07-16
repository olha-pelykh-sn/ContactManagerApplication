using ContactManagerApplication.Data;
using ContactManagerApplication.Models;
using ContactManagerApplication.Models.Domain;
using Dapper;

namespace ContactManagerApplication.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ContactManagerDbContext _dbContext;

        public EmployeeRepository(ContactManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Employee employee)
        {
            var query = "INSERT INTO Employees (Id, Name, BirthDay, IsMarried, Phone, Salary) VALUES (@Id, @Name, @BirthDay, @IsMarried, @Phone, @Salary)";

            using (var connection = _dbContext.CreateConnection())
            {
                return await connection.ExecuteAsync(query, employee);
            }

        }

        public async Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeId = null)
        {
            using var connection = _dbContext.CreateConnection();

            if (excludeId.HasValue)
            {
                var count = await connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM Employees WHERE Phone = @Phone AND Id != @ExcludeId",
                    new { Phone = phone, ExcludeId = excludeId.Value });
                return count == 0;
            }
            else
            {
                var count = await connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM Employees WHERE Phone = @Phone",
                    new { Phone = phone });
                return count == 0;
            }
        }

        public async Task<int> CreateBulkAsync(IEnumerable<Employee> employees)
        {
            var list = employees as List<Employee> ?? employees.ToList();
            if (list.Count == 0) return 0;

            const string query =
                "INSERT INTO Employees (Id, Name, BirthDay, IsMarried, Phone, Salary) " +
                "VALUES (@Id, @Name, @BirthDay, @IsMarried, @Phone, @Salary)";

            using var connection = _dbContext.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var count = await connection.ExecuteAsync(query, list, transaction);
                await transaction.CommitAsync();
                return count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var query = "DELETE FROM Employees WHERE Id = @Id";

            using (var connection = _dbContext.CreateConnection())
            {
                return await connection.ExecuteAsync(query, new { Id = id });
            }
        }

        public async Task<PaginatedResult<Employee>> GetAllAsync(PaginationParameters parameters)
        {
            var (query, countQuery, dbParams) = BuildQuery(parameters);

            using (var connection = _dbContext.CreateConnection())
            {
                var totalCount = await connection.QuerySingleAsync<int>(countQuery, dbParams);

                var employees = await connection.QueryAsync<Employee>(query, dbParams);

                return new PaginatedResult<Employee>
                {
                    Items = employees,
                    TotalCount = totalCount,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
            }
        }

        public async Task<Employee> GetByIdAdync(Guid id)
        {
            var query = "SELECT * FROM Employees WHERE Id = @Id";

            using (var connection = _dbContext.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Employee>(query, new { Id = id });
            }
        }

        public async Task<int> UpdateAsync(Employee employee)
        {
            var query = "UPDATE Employees SET Name = @Name, BirthDay = @BirthDay, IsMarried = @IsMarried, Phone = @Phone, Salary = @Salary WHERE Id = @Id";

            using (var connection = _dbContext.CreateConnection())
            {
                return await connection.ExecuteAsync(query, employee);
            }
        }

        private (string query, string countQuery, DynamicParameters dbParams) BuildQuery(PaginationParameters parameters)
        {
            var dbParams = new DynamicParameters();
            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                conditions.Add("(Name LIKE @SearchQuery OR Phone LIKE @SearchQuery)");
                dbParams.Add("SearchQuery", $"%{parameters.SearchQuery}%");
            }

            if (parameters.BirthDayFrom.HasValue)
            {
                conditions.Add("BirthDay >= @BirthDayFrom");
                dbParams.Add("BirthDayFrom", parameters.BirthDayFrom.Value);
            }

            if (parameters.BirthDayTo.HasValue)
            {
                conditions.Add("BirthDay <= @BirthDayTo");
                dbParams.Add("BirthDayTo", parameters.BirthDayTo.Value);
            }

            if (parameters.IsMarried.HasValue)
            {
                conditions.Add("IsMarried = @IsMarried");
                dbParams.Add("IsMarried", parameters.IsMarried.Value);
            }

            if (parameters.MinSalary.HasValue)
            {
                conditions.Add("Salary >= @MinSalary");
                dbParams.Add("MinSalary", parameters.MinSalary.Value);
            }

            if (parameters.MaxSalary.HasValue)
            {
                conditions.Add("Salary <= @MaxSalary");
                dbParams.Add("MaxSalary", parameters.MaxSalary.Value);
            }

            var whereClause = conditions.Count > 0
                ? " WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            var countQuery = $"SELECT COUNT(*) FROM Employees{whereClause}";

            var sortColumn = GetValidSortColumn(parameters.SortBy);
            var sortDirection = parameters.SortDescending ? "DESC" : "ASC";
            var offset = (parameters.PageNumber - 1) * parameters.PageSize;

            var query = $"SELECT * FROM Employees{whereClause}" +
                        $" ORDER BY {sortColumn} {sortDirection}" +
                        $" OFFSET {offset} ROWS FETCH NEXT {parameters.PageSize} ROWS ONLY";

            return (query, countQuery, dbParams);
        }

        private string GetValidSortColumn(string? sortBy)
        {
            var validColumns = new Dictionary<string, string>
            {
                { "name", "Name" },
                { "salary", "Salary" },
                { "birthday", "BirthDay" },
                { "ismarried", "IsMarried" },
                { "phone", "Phone" }
            };

            if (string.IsNullOrWhiteSpace(sortBy) || !validColumns.TryGetValue(sortBy.ToLower(), out var column))
            {
                return "Name";
            }

            return column;
        }
    }
}
