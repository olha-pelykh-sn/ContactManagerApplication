using ContactManagerApplication.Models;
using ContactManagerApplication.Models.Domain;

namespace ContactManagerApplication.Repositories
{
    public interface IEmployeeRepository
    {
        Task<PaginatedResult<Employee>> GetAllAsync(PaginationParameters parameters);
        Task<Employee> GetByIdAdync(Guid id);
        Task<int> CreateAsync(Employee employee);
        Task<int> UpdateAsync(Employee employee);
        Task<int> DeleteAsync(Guid id);
        Task<int> CreateBulkAsync(IEnumerable<Employee> employees);
        Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeId = null);
    }
}
