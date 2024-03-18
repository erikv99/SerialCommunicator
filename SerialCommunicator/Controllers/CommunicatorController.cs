using Microsoft.AspNetCore.Mvc;
using SerialCommunicator.Models;

namespace SerialCommunicator.Controllers
{
    public class CommunicatorController : Controller
    {
        public IActionResult Index()
        {
            var model = new CommunicatorVM 
            {
                
            };

            return View();
        }

        
    }
}
