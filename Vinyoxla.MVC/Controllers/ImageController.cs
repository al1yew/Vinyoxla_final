using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vinyoxla.MVC.Controllers
{
    public class ImageController : Controller
    {
        public IActionResult Index(string vin)
        {
            return View();
        }
    }
}
