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

        // экшен, который принимает данные с формы для создания новой точки

        [HttpPost]
        [Authorize]
        public ActionResult Add(FormCollection collection, IEnumerable<HttpPostedFileBase> upload)
        {
            //UserProfile Autor = db.GetUSerProfile(User);

            Point p = new Point()
            {
                GeoData = new GeoData(Convert.ToDouble(collection["latitude"]), Convert.ToDouble(collection["latitude"]), collection["adresset"]),
                //Autor = Autor,
                Defect = new Defect() { Name = collection["DefName"] },
            };
            //p.AddComent(new Comment() { ContentText = collection["FirstComment"] });//, Autor = Autor });

            db.Points.Add(p);
            db.SaveChanges();
            int id = p.ID;
            List<string> fileList = ImageHelper.SaveUploadFiles(id, upload); // Метод сохранения фотки
            return RedirectToAction("Index", "Home");
        }

        /// <summary>Передача во "view" данных о выбранной "точке" </summary>
        /// <param name="id">ID Выбранной "точке"</param>
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
        /// <summary>Тестовый Экшен-заглушка для корректной работы "EditComments"</summary>
        /// <returns>Объект типа Point</returns>
        public ActionResult Test()
        {
            List<Point> listPoints = db.Points.ToList<Point>();   //список всех точек в базе
            Point p = listPoints[0]; // получаем точку под индексом "0"

            // создаём и заполняем объект типа "Comment"
            Comment c = new Comment();
            c.ContentText = "Start";

            // добавляем новый коммент в БД и сохраняем изменения
            p.Comments.Add(c);
            db.SaveChanges();

            ViewBag.Content = p.Comments.ElementAt(0).ContentText; // для вывода во View содержимого "Comments"
            ViewBag.Comment_Id = p.Comments.ElementAt(0).ID;       // для получения ID выводимого комментария

            return View(p);
        }

        /// <summary> Editor для изменения комментариев </summary>
        /// <param name="content">Новое содержимое комментария</param>
        /// <param name="Id_Point">ID точки в котором меняем комментарий</param>
        /// <param name="Id_Comment">ID необходимого комментария</param>
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EditComments(string content, int Id_Point, int Id_Comment)
        {
            Point Pnt = db.Points.Where(p => p.ID.Equals(Id_Point)).Single();        // находим нужный нам Point 
            Comment Cmt = Pnt.Comments.Where(c => c.ID.Equals(Id_Comment)).Single(); // Берём необходимый нам комментарий

            // если изменения в комментарии были - заменяем на новые и сохраняем в базе.
            if (Cmt.ContentText != content)
            {
                Cmt.ContentText = content;
                db.SaveChanges();
            }

            return Json("OK");
        }
    }
}