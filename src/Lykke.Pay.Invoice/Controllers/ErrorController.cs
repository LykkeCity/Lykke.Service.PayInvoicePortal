using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Pay.Invoice.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("{statusCode}")]
        public IActionResult Index(string statusCode)
        {
            if (!string.IsNullOrEmpty(statusCode))
            {
                switch (statusCode)
                {
                    case "404":
                    case "403":
                        return View("_404");
                }
            }

            return View("_404");
        }
    }
}