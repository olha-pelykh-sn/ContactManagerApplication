using ContactManagerApplication.Models;
using ContactManagerApplication.Models.Domain;

namespace ContactManagerApplication.Models.ViewModel
{
    public class EmployeeListViewModel
    {
        public PaginatedResult<Employee> Data { get; set; } = new();
        public string? CurrentSort { get; set; }
        public bool CurrentSortDesc { get; set; }
        public string? SearchQuery { get; set; }
        public string? BirthDayFrom { get; set; }
        public string? BirthDayTo { get; set; }
        public string? IsMarriedFilter { get; set; }
        public string? MinSalary { get; set; }
        public string? MaxSalary { get; set; }
    }
}
