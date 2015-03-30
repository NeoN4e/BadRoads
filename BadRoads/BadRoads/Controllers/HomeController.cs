using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BadRoads.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About ViewBag.Message";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact ViewBag.Message";

            return View();
        }
    }
}
