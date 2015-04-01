using BadRoads.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BadRoads.Models;

namespace BadRoads.Controllers
{
    [Culture]
    public class HomeController : Controller
    {

        BadroadsDataContext db = new BadroadsDataContext();      // объект модели

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

        public ActionResult ChangeCulture(string lang)
        {
            string returnUrl = Request.UrlReferrer.AbsolutePath;
            // Список культур
            List<string> cultures = new List<string>() { "ru", "en", "uk" };
            if (!cultures.Contains(lang))
            {
                lang = "ru";
            }
            // Сохраняем выбранную культуру в куки
            HttpCookie cookie = Request.Cookies["lang"];
            if (cookie != null)
                cookie.Value = lang;   // если куки уже установлено, то обновляем значение
            else
            {

                cookie = new HttpCookie("lang");
                cookie.HttpOnly = false;
                cookie.Value = lang;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return Redirect(returnUrl);
        }

        public ActionResult Map()   // отображение основной карты со всеми сохраненными точками
        {
            //List<Point> listPoints = db.Points.ToList<Point>();                список всех точек в базе
            List<Point> listPoints = new List<Point>();
            for(int x = 0; x<100;x++)                              // заглушка. чтобы наполнить список с точками, которых пока нет в базе
            {
                double latitude = 48.459015 + (x * 0.00045);
                double longitude = 35.042302 + (x * 0.00045);
                string adress = String.Format("Проблема на улице " + x);
                UserProfile u = new UserProfile();
                GeoData g = new GeoData(latitude, longitude);
                g.FullAddress = adress;
                Point p = new Point(u);
                p.GeoData = g;
                listPoints.Add(p);
            }
            return View(listPoints);
        }

        public ActionResult PointInfo( int id = 0)
        {
            // получение необходимой "точки" и передаём её во "View"
            //Point p = (from entry in db.Points where entry.ID == id select entry).Single();
            //return View(p);

            // заглушка. чтобы наполнить список с точками, которых пока нет в базе
            List<Point> listPoints = new List<Point>();
            for (int x = 0; x < 100; x++)                              
            {
                double latitude = 48.459015 + (x * 0.00045);
                double longitude = 35.042302 + (x * 0.00045);
                string adress = String.Format("Проблема на улице " + x);
                UserProfile u = new UserProfile();
                GeoData g = new GeoData(latitude, longitude);
                g.FullAddress = adress;
                Point p = new Point(u);
                p.GeoData = g;
                listPoints.Add(p);
            }
            // 
            Point pnt = listPoints[0];
            return View(pnt);
        }
    }
}
