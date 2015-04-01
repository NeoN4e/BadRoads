using BadRoads.Filters;
using BadRoads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BadRoads.Controllers
{
    [Culture]
    public class PointController : Controller
    {
        BadroadsDataContext db = new BadroadsDataContext();      // объект модели

        [HttpPost]
        public ActionResult CreatePoint(Point Pnt)
        {
            if (ModelState.IsValid)
            {
                db.Points.Add(Pnt);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(Pnt);
        }

        /// <summary>  Передача во "view" данных о выброной "точке"  </summary>
        /// <param name="id">Выбранная "точка"</param>
        /// <returns>Point</returns>
        public ActionResult PointInfo(int id)
        {
            //получение необходимой "точки" и передаём её во "View"
            Point p = (from entry in db.Points where entry.ID == id select entry).Single();
            return View(p);

            //// заглушка. чтобы наполнить список с точками, которых пока нет в базе
            //List<Point> listPoints = new List<Point>();
            //for (int x = 0; x < 100; x++)                              
            //{
            //    double latitude = 48.459015 + (x * 0.00045);
            //    double longitude = 35.042302 + (x * 0.00045);
            //    string adress = String.Format("Проблема на улице " + x);
            //    UserProfile u = new UserProfile();
            //    GeoData g = new GeoData(latitude, longitude);
            //    g.FullAddress = adress;
            //    Point p = new Point(u);
            //    p.GeoData = g;
            //    listPoints.Add(p);
            //}
            //// 
            //Point pnt = listPoints[0];
            //return View(pnt);
        }
    }
}