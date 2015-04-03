using BadRoads.Filters;
using BadRoads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

namespace BadRoads.Controllers
{
    [Culture]
    public class PointController : Controller
    {
        BadroadsDataContext db = new BadroadsDataContext();      // объект модели

        static List<Point> PaginatorList;

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

        // экшен, который принимает данные с формы для создания новой точки
        
        [HttpPost]
        public ActionResult Add(FormCollection collection)
        {
            return RedirectToAction("Index", "Home");
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

        public ActionResult Add()                    // оформление добавления новой точки
        {
            List<Defect> df = db.Defects.ToList<Defect>();
            ViewBag.Problems = df;                // список дефектов для выбора их на форме заполнения точки
            
            List<Point> listPoints = db.Points.ToList<Point>();//список всех точек в базе
            return View(listPoints);      // отправляем список всех точек, чтобы при выборе точки не выбирали ее там, где она уже есть
        }

        /// <summary>
        /// show points on Gallery View
        /// Default sorting by last comment date [SOON]
        /// </summary>
        /// <param name="page"> current page </param>
        /// <param name="flag"> is filtred data? </param>
        /// <returns> PartialView LIST (All pages) </returns>
        public ActionResult Gallery(int? page,bool? flag)
        {
            int pointsOnPage = 8;//maximum Point elements on page
            if (flag != true)
            {
                PaginatorList = db.Points.ToList<Point>();
            }
            return View(PaginatorList.ToPagedList<Point>(page??1,pointsOnPage));
        }
        /// <summary>
        /// show points on pages using partial view
        /// </summary>
        /// <param name="searchText"> search string from forms </param>
        /// <param name="page"> current page </param>
        /// <returns> Partial view LIST(one page) </returns>
        public ActionResult Search(string searchText = "", int page = -1)
        {
            int pointsOnPage = 8;//maximum Point elements on page
            PaginatorList = db.Points.Where(p => p.GeoData.FullAddress.Contains(searchText)).ToList();

            if (page == -1)
            {
                return PartialView(PaginatorList.ToPagedList<Point>(1, pointsOnPage));
            }
            else
            {
                return PartialView(PaginatorList.ToPagedList<Point>(page, pointsOnPage));
            }
        }
    }
}