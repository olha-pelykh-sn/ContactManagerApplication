using ContactManagerApplication.Models.ViewModel;

namespace ContactManagerApplication.Services
{
    public interface ICsvImportService
    {
        Task<CsvImportResultVM> ImportAsync(Stream csvStream);
    }
}
