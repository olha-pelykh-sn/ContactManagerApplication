using System.Text;
using Microsoft.AspNetCore.Mvc;
using ContactManagerApplication.Models;
using ContactManagerApplication.Models.ViewModel;
using ContactManagerApplication.Data;
using ContactManagerApplication.Repositories;
using ContactManagerApplication.Models.Domain;
using ContactManagerApplication.Services;
using AutoMapper;
using AutoMapper.Configuration.Annotations;

namespace ContactManagerApplication.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICsvImportService _csvImportService;

        public EmployeeController(
            IEmployeeRepository employeeRepository,
            IMapper employeeMapper,
            ICsvImportService csvImportService)
        {
            _repository = employeeRepository;
            _mapper = employeeMapper;
            _csvImportService = csvImportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool sortDescending = false,
            string? searchQuery = null,
            DateOnly? birthDayFrom = null,
            DateOnly? birthDayTo = null,
            string? isMarriedFilter = null,
            decimal? minSalary = null,
            decimal? maxSalary = null)
        {
            // Parse marital status: "true" = married, "false" = not married, anything else = all
            bool? isMarriedBool = isMarriedFilter?.ToLowerInvariant() switch
            {
                "true"  => true,
                "false" => false,
                _       => null
            };

            var parameters = new PaginationParameters
            {
                PageNumber    = pageNumber,
                PageSize      = pageSize,
                SortBy        = sortBy,
                SortDescending = sortDescending,
                SearchQuery   = searchQuery,
                BirthDayFrom  = birthDayFrom,
                BirthDayTo    = birthDayTo,
                IsMarried     = isMarriedBool,
                MinSalary     = minSalary,
                MaxSalary     = maxSalary
            };

            var paginatedResult = await _repository.GetAllAsync(parameters);

            ViewData["CurrentSort"]        = sortBy;
            ViewData["CurrentSortDesc"]    = sortDescending;
            ViewData["CurrentSearchQuery"] = searchQuery;
            ViewData["BirthDayFrom"]       = birthDayFrom?.ToString("yyyy-MM-dd");
            ViewData["BirthDayTo"]         = birthDayTo?.ToString("yyyy-MM-dd");
            // Store the normalised string (null when "all" so pagination links omit the param)
            ViewData["IsMarriedFilter"]    = string.IsNullOrEmpty(isMarriedFilter) ? null : isMarriedFilter.ToLowerInvariant();
            ViewData["MinSalary"]          = minSalary?.ToString();
            ViewData["MaxSalary"]          = maxSalary?.ToString();

            return View(paginatedResult);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> Create(CreateEmployeeVM addEmployeeVM)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", addEmployeeVM);
            }

            // Phone uniqueness check
            if (!string.IsNullOrWhiteSpace(addEmployeeVM.Phone))
            {
                var isUnique = await _repository.IsPhoneUniqueAsync(addEmployeeVM.Phone);
                if (!isUnique)
                {
                    ModelState.AddModelError(nameof(addEmployeeVM.Phone),
                        "This phone number is already registered to another employee.");
                    return View("Create", addEmployeeVM);
                }
            }

            try
            {
                var employee = _mapper.Map<Employee>(addEmployeeVM);

                var result = await _repository.CreateAsync(employee);

                if (result > 0)
                {
                    return RedirectToAction("GetAllEmployees");
                }

                ModelState.AddModelError(string.Empty, "Failed to add employee. Please try again.");
                return View("Create", addEmployeeVM);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while adding the employee.");
                return View("Create", addEmployeeVM);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var employee = await _repository.GetByIdAdync(id);

            if (employee == null)
            {
                return NotFound();
            }

            var updateEmployeeVM = _mapper.Map<UpdateEmployeeVM>(employee);

            return View(updateEmployeeVM);
        }

        // Inline-edit endpoint — called via fetch() from the table
        [HttpPost]
        public async Task<IActionResult> InlineUpdate([FromBody] UpdateEmployeeVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(new { errors });
            }

            // Phone uniqueness check (exclude the employee being edited)
            if (!string.IsNullOrWhiteSpace(model.Phone))
            {
                var isUnique = await _repository.IsPhoneUniqueAsync(model.Phone, model.Id);
                if (!isUnique)
                    return BadRequest(new { errors = new[]
                        { "This phone number is already registered to another employee." } });
            }

            try
            {
                var employee = _mapper.Map<Employee>(model);
                var result   = await _repository.UpdateAsync(employee);

                if (result > 0)
                {
                    return Ok(new
                    {
                        success   = true,
                        formatted = new
                        {
                            salary   = employee.Salary.ToString("C"),
                            birthDay = employee.BirthDay?.ToString("dd/MM/yyyy") ?? string.Empty
                        }
                    });
                }

                return BadRequest(new { errors = new[] { "Employee not found or could not be updated." } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { errors = new[] { "An unexpected error occurred. Please try again." } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeVM updateEmployeeVM)
        {
            if (id != updateEmployeeVM.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(updateEmployeeVM);
            }

            // Phone uniqueness check (exclude the employee being edited)
            if (!string.IsNullOrWhiteSpace(updateEmployeeVM.Phone))
            {
                var isUnique = await _repository.IsPhoneUniqueAsync(updateEmployeeVM.Phone, updateEmployeeVM.Id);
                if (!isUnique)
                {
                    ModelState.AddModelError(nameof(updateEmployeeVM.Phone),
                        "This phone number is already registered to another employee.");
                    return View(updateEmployeeVM);
                }
            }

            try
            {
                var employee = _mapper.Map<Employee>(updateEmployeeVM);
                var result = await _repository.UpdateAsync(employee);

                if (result > 0)
                {
                    return RedirectToAction(nameof(GetAllEmployees));
                }

                ModelState.AddModelError(string.Empty, "Failed to update employee.");
                return View(updateEmployeeVM);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the employee.");
                return View(updateEmployeeVM);
            }
        }

        // ── CSV Import ────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult UploadCsv()
        {
            return View();
        }

        [HttpPost]
        [ActionName("UploadCsv")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        public async Task<IActionResult> UploadCsvPost(IFormFile? csvFile)
        {
            if (csvFile is null || csvFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a CSV file to upload.");
                return View("UploadCsv");
            }

            var extension = Path.GetExtension(csvFile.FileName).ToLowerInvariant();
            if (extension != ".csv")
            {
                ModelState.AddModelError(string.Empty, "Only CSV files (.csv) are accepted.");
                return View("UploadCsv");
            }

            try
            {
                using var stream = csvFile.OpenReadStream();
                var result = await _csvImportService.ImportAsync(stream);
                return View("UploadCsvResult", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
                return View("UploadCsv");
            }
        }

        [HttpGet]
        public IActionResult DownloadCsvTemplate()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Name,BirthDay,IsMarried,Phone,Salary");
            csv.AppendLine("John Doe,1990-05-15,true,+1234567890,50000.00");
            csv.AppendLine("Jane Smith,1985-08-22,false,+0987654321,60000.00");
            csv.AppendLine("Alice Johnson,,,, 45000");

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "employees_template.csv");
        }

        // ── Delete ───────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);

                if (result > 0)
                {
                    return RedirectToAction(nameof(GetAllEmployees));
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the employee.");
            }
        }
    }
}
