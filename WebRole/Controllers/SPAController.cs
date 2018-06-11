using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebRole.Controllers
{
    public class SPAController : Controller
    {
        public IActionResult Page(string tuttoilrestoeilnulla)
        {
            return View();
        }
    }
}