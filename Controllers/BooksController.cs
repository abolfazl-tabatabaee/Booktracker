using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult Details(int id)
        {

            return View();
        }
    }
}
