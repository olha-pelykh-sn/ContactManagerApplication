using ContactManagerApplication.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagerApplication.ViewComponents
{
    public class EmployeeListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(EmployeeListViewModel model)
        {
            return View(model);
        }
    }
}
