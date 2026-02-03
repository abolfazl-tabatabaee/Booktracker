using bookTracker.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace bookTracker.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult About() => View();
        public IActionResult Contact() => View();
    }
  

}
