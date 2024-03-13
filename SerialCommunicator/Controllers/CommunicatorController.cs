using Microsoft.AspNetCore.Mvc;

namespace SerialCommunicator.Controllers
{
    public class CommunicatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
