namespace ContactManagerApplication.Models
{
    public class PaginationParameters
    {
        //PAGINATION
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        //SORT
        public string? SortBy { get; set; }

        public bool SortDescending { get; set; } = false;

        //SEARCH
        public string? SearchQuery { get; set; }

        // FILTER
        public DateOnly? BirthDayFrom { get; set; }
        public DateOnly? BirthDayTo { get; set; }

        public bool? IsMarried { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
    }
}
