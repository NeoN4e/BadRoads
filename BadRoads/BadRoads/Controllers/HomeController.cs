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
            //db.Database.Delete();
            db.Database.Initialize(false);
            
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

        public ActionResult Map(string stringForMap = null)   // отображение основной карты со всеми сохраненными точками. Принимает координаты для центра карты, если переходили с экшена PointInfo
        {
            ViewBag.MarkerLocation = stringForMap;
            List<Point> listPoints = db.Points.Where(v => v.isValid == true).ToList<Point>();   // список точек прошедших валидацию
            //List<Point> listPoints = db.Points.ToList<Point>();   //список всех точек в базе
            return View(listPoints);
        }
    }
}
