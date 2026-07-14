namespace ContactManagerApplication.Models.Domain
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateOnly? BirthDay { get; set; }
        public bool? IsMarried { get; set; }
        public string? Phone { get; set; }
        public decimal Salary { get; set; }
    }
}
