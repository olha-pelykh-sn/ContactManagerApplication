using ContactManagerApplication.Models.Domain;

namespace ContactManagerApplication.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> GetByIdAdync(Guid id);
        Task<int> CreateAsync(Employee employee);
    }
}
