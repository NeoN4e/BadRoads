using BadRoads.Filters;
using BadRoads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.Web.Security;
using System.Data.Entity.Validation;

namespace BadRoads.Controllers
{
    [Culture]
    public class PointController : Controller
    {
        BadroadsDataContext db = new BadroadsDataContext();      // объект модели

        static List<Point> PaginatorList;

        /// <summary>
        /// Sorting Point list by last comment date
        /// </summary>
        /// <param name="list">list of Points for sort</param>
        /// <returns>sorted list</returns>
        public List<Point> SortByLastComment(List<Point> list)
        {
            list.Sort((x, y) => x.Comments.Max(c => c.Date).CompareTo(y.Comments.Max(c => c.Date)));
            return list;
        }

        /// <summary>
        /// Экшен, который принимает данные с формы, для создания новой точки
        /// Last Author: Yuriy Kovalenko (anekosheik@gmail.com). Last modified 07/04/2015 23:40
        /// </summary>
        /// <param name="collection">Данные с формы добавления точки</param>
        /// <param name="upload">Коллекция фото</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Add(FormCollection collection, IEnumerable<HttpPostedFileBase> upload)
        {
            try
            {
                UserProfile CurAutor = db.GetUSerProfile(User);
                string lat = collection["latitude"];
                if (lat.Count() > 10)
                {
                    lat = lat.Substring(0, 10); // уменьшаем размер символов после запятой
                }
                lat = lat.Replace(".", ",");
                string lng = collection["longitude"];
                if (lng.Count() > 10)
                {
                    lng = lng.Substring(0, 10); // уменьшаем размер символов после запятой
                }
                lng = lng.Replace(".", ",");
                double latdouble = Convert.ToDouble(lat);
                double lngdouble = Convert.ToDouble(lng);

                Point p = new Point()
                {
                    GeoData = new GeoData(latdouble, lngdouble, collection["adresset"]),
                    Autor = CurAutor,
                    Defect = db.GetDefect(collection["DefName"])
                };
                p.AddComent(new Comment() { ContentText = collection["FirstComment"], Autor = CurAutor });
                
                db.Points.Add(p);
                
                db.SaveChanges();
                int id = p.ID;
                List<string> fileList = ImageHelper.SaveUploadFiles(id, upload); // Метод сохранения фото
                p.isValid = ImageHelper.CheckPointMetaDataAndDistance(fileList, p); // check

                foreach (var item in fileList)
                {
                    p.AddPhoto(new Photo() { Url = item.ToString() }); // запись ссылки на фото в таблицу ФОТО
                }
                p.Cover = p.Photos.First(); // запись ссылки на фото в кавер для галлереи
                
                db.SaveChanges();
                return RedirectToAction("Map", "Home"); // переход на Карту
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("MyError");
            }

            //return RedirectToAction("ThanksForPoint", "Point");    // переход в экшен ThanksForPoint
        }

        /// <summary>Передача во "view" данных о выбранной "точке" </summary>
        /// <param name="id">ID Выбранной "точке"</param>
        /// <returns>Point</returns>
        public ActionResult PointInfo(int id)
        {
            Point p = (from entry in db.Points where entry.ID == id select entry).Single();     // получаем необходимую точку
            Comment c = new Comment();
            c = p.Comments.FirstOrDefault();                                   // передаем первый комментарий к точке как описание
            if (c != null)
            {
                ViewBag.Description = c.ContentText;
            }
            else
            {
                ViewBag.Description = "Нет описания к проблеме.";
            }
            return View(p);
        }

        /// <summary>
        /// Action for moderators and administrators
        /// Shows a list of demands moderation
        /// </summary>
        /// <returns>List of points that require moderation</returns>
        [Authorize(Roles = "Moderator, Administrator")]
        public ActionResult ModerationList()
        {
            if (User.Identity.IsAuthenticated)    // Auth check
            {
                List<Point> NeedModeratePoints = db.Points.Where(p => p.isValid == false).ToList(); //Creating and filling a list of points that require moderation

                return View(NeedModeratePoints);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult Add(string stringForMap = null)                    // оформление добавления новой точки, принимает строку координат для новой точки, если она передвалась с экшена Map
        {
            if (User.Identity.IsAuthenticated)                                 // если пользователь авторизован
            {

                ViewBag.MarkerLocation = stringForMap;
                string[] defects = { "Яма", "Открытый люк", "Отсутствие разметки" };
                ViewBag.Problems = defects;  // список дефектов для выбора их на форме заполнения точки
                //List<Point> listPoints = db.Points.Where(v => v.isValid == true).ToList<Point>();   // список точек прошедших валидацию
                List<Point> listPoints = db.Points.ToList<Point>();//список всех точек в базе
                return View(listPoints);      // отправляем список всех точек, чтобы при выборе точки не выбирали ее там, где она уже есть
            }
            else
            {
                return RedirectToAction("Login", "Account");                              // иначе перенаправляем к экшену авторизации
            }
        }

        /// <summary>
        /// show points on Gallery View
        /// Default sorting by last comment date [SOON]
        /// </summary>
        /// <param name="page"> current page </param>
        /// <param name="flag"> is filtred data? </param>
        /// <returns> PartialView LIST (All pages) </returns>
        public ActionResult Gallery(int? page, bool? flag)
        {
            int pointsOnPage = 8;//maximum Point elements on page
            if (flag != true)
            {
                PaginatorList = db.Points.ToList<Point>();
            }
            //return View(SortByLastComment(PaginatorList).ToPagedList<Point>(page ?? 1, pointsOnPage)); uncomment after adding normal data in database
            return View(PaginatorList.ToPagedList<Point>(page ?? 1, pointsOnPage));
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
                //return PartialView(SortByLastComment(PaginatorList).ToPagedList<Point>(1, pointsOnPage)); uncomment after adding normal data in database
                return PartialView(PaginatorList.ToPagedList<Point>(1, pointsOnPage));
            }
            else
            {
                //return PartialView(SortByLastComment(PaginatorList).ToPagedList<Point>(page, pointsOnPage));uncomment after adding normal data in database
                return PartialView(PaginatorList.ToPagedList<Point>(page, pointsOnPage));
            }
        }

        /// <summary>Добавляет комментарий к текущей точке</summary>
        /// <param name="Id">ID текущей точки</param>
        /// <param name="collection">Новый комментарий </param>
        /// <returns>перенаправляет на PointInfo с текущей точкой</returns>
        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult AddComment(int Id, FormCollection collection)
        {
            UserProfile CurAutor = db.GetUSerProfile(User);                                     // получаем автора сообщения
            Point p = (from entry in db.Points where entry.ID == Id select entry).Single();     // получаем необходимую точку
            if (collection["NewComment"] == "")
                p.AddComent(new Comment() { ContentText = "No comment", Autor = CurAutor });             // создаём комментарий с указанием того, что он пуст (Заглушка)
            else
                p.AddComent(new Comment() { ContentText = collection["NewComment"], Autor = CurAutor }); // создаём комментарий и заполняем его текстом и автором
            db.SaveChanges(); // сохраняем изменения

            return RedirectToAction("PointInfo", "Point", new { id = Id }); // перенапрявляем на другой экшен
        }

        /// <summary> Editor для изменения комментариев </summary>
        /// <param name="content">Новое содержимое комментария</param>
        /// <param name="Id_Point">ID точки в котором меняем комментарий</param>
        /// <param name="Id_Comment">ID необходимого комментария</param>
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EditComments(string content, int Id_Point, int Id_Comment)
        {
            try
            {
                Point Pnt = db.Points.Where(p => p.ID.Equals(Id_Point)).Single();        // находим нужный нам Point 
                Comment Cmt = Pnt.Comments.Where(c => c.ID.Equals(Id_Comment)).Single(); // Берём необходимый нам комментарий

                // если изменения в комментарии были - заменяем на новые и сохраняем в базе.
                if (Cmt.ContentText != content && content != "")
                {
                    Cmt.ContentText = content;
                    db.SaveChanges();
                }

                return Json("OK");
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                return Json(new { ok = false, message = ex.Message });
            }
        }

        public ActionResult ThanksForPoint()       // благодарность пользователю за добавление новой точки
        {
            return View();
        }

        public ActionResult DeletePoint(int id)          // экшен удаления точки модератором. Принимает ID точки.     Волков Антон. 06.04.15
        {
            if (Request.IsAuthenticated && Roles.IsUserInRole("Moderator"))     //  если авторизован с ролью "модератор"
            {
                Point p = db.Points.Find(id);                                   // находим точку по ID   
                db.Points.Remove(p);                                            // удаляем точку.
                db.SaveChanges();                                               // сохраняем изменения в базе
                return RedirectToAction("Map", "Home");                        // переход на основную карту
            }
            else
                return RedirectToAction("Login", "Account");                              // иначе перенаправляем к экшену авторизации                    
        }

        public ActionResult ConfirmPoint(int id)               // экшен валидации точки модератором. Принимает ID точки.     Волков Антон 06.04.15
        {
            if (Request.IsAuthenticated && Roles.IsUserInRole("Moderator"))                    //  если авторизован с ролью "модератор"
            {
                Point p = db.Points.Find(id);                                                  // находим точку по ID   
                p.isValid = true;                                                              // подтверждаем точку
                try
                {
                    db.SaveChanges();            // сохраняем изменения в базе !!!! НЕ СОХРАНЯЕТСЯ   выкидывает исключение!!!!!!!
                }
                catch { }
                return RedirectToAction("Map", "Home");                                        // переход на основную карту
            }
            else
                return RedirectToAction("Login", "Account");                              // иначе перенаправляем к экшену авторизации
        }
    }
}