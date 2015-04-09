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
        /// Provides create a new point and processing the obtained data from the html-form
        /// Yuriy Kovalenko (anekosheik@gmail.com)
        /// </summary>
        /// <param name="collection">Data from the html-form</param>
        /// <param name="upload">Collection uploads images</param>
        /// <returns>Redirect to view "Map"</returns>
        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Add(FormCollection collection, IEnumerable<HttpPostedFileBase> upload)
        {
            int id = - 1;
            try
            {
                UserProfile CurAutor = db.GetUSerProfile(User);
                string lat = collection["latitude"];
                if (lat.Count() > 10)
                {
                    lat = lat.Substring(0, 10); // reduce the size of characters after the decimal point
                }
                lat = lat.Replace(".", ",");
                string lng = collection["longitude"];
                if (lng.Count() > 10)
                {
                    lng = lng.Substring(0, 10); // reduce the size of characters after the decimal point
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

                if (collection["FirstComment"] == null || collection["FirstComment"] == "")
                {
                    p.AddComent(new Comment() { ContentText = "No comment", Autor = CurAutor }); // <!-- NEED LOCALIZATION -->
                }
                else
                {
                    p.AddComent(new Comment() { ContentText = collection["FirstComment"], Autor = CurAutor });
                }
                
                db.Points.Add(p);
                db.SaveChanges();
                
                id = p.ID;
                List<string> fileList = ImageHelper.SaveUploadFiles(id, upload); // save foto
                p.isValid = ImageHelper.CheckPointMetaDataAndDistance(fileList, p); // check validation

                foreach (var item in fileList)
                {
                    p.AddPhoto(new Photo() { Url = item.ToString() }); // post links to photos in the table PHOTO
                }
                p.Cover = p.Photos.First(); // post links first photo in the main image from Gallery
                
                db.SaveChanges();
                return RedirectToAction("Map", "Home", new { flag = true });
            }
            catch (Exception ex)
            {
                if (id != -1)
                {
                    Point p = db.Points.Find(id);
                    db.Points.Remove(p);
                    db.SaveChanges();
                    ImageHelper.DeleteAllUploadFiles(id);       // delete folder whith uploads foto
                }
                ViewBag.Message = ex.Message;
                return View("MyError");
            }
        }
        /// <summary>
        /// Provides a transition to a new point View
        /// </summary>
        /// <param name="stringForMap">Takes a string of coordinates for the new point, if it is passed on to the action "Map"</param>
        /// <returns>view whith list of Points</returns>
        public ActionResult Add(string stringForMap = null)                    
        {
            if (User.Identity.IsAuthenticated)                        // only Authenticated user
            {
                ViewBag.MarkerLocation = stringForMap;
                ViewBag.Problems = db.Defects.ToList();                // defect list for selecting the form
                List<Point> listPoints = db.Points.Where(v => v.isValid == true).ToList<Point>();   // a list of validates points
                //List<Point> listPoints = db.Points.ToList<Point>();    //список всех точек в базе
                return View(listPoints);                               // view whith list of Points
            }
            else
            {
                return RedirectToAction("Login", "Account");           // redirects to login action
            }
        }

        /// <summary>Передача во "view" данных о выбранной "точке" </summary>
        /// <param name="id">ID Выбранной "точки"</param>
        /// <returns>Point</returns>
        public ActionResult PointInfo(int id)
        {
            Point p = (from entry in db.Points where entry.ID == id select entry).Single();     // получаем необходимую точку
            Comment c = new Comment();
            c = p.Comments.FirstOrDefault();                                   // передаем первый комментарий к точке как описание
            ViewBag.Description = c.ContentText;
            HttpCookie cookie = Request.Cookies["lang"];   // определяем текущий язык
            if (cookie != null)
            {
                ViewBag.Lang = cookie.Value;
            }
            else                                           // если язык еще не устанавливался, передаем русский по умолчанию
            {
                ViewBag.Lang = "ru";
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
                PaginatorList = db.Points.Where(v => v.isValid == true).ToList<Point>();    //только проверенные точкиотображаются в галерее 09.04.15 Дон
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
            page = page == -1 ? 1 : page;
            if (Request.IsAjaxRequest())
            {
                return PartialView(PaginatorList.ToPagedList<Point>(page, pointsOnPage));
            }
            return View("Gallery", PaginatorList.ToPagedList<Point>(page, pointsOnPage));
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

        // Необходимость этого куска кода под вопросом
        ///// <summary> Editor для изменения комментариев </summary>
        ///// <param name="content">Новое содержимое комментария</param>
        ///// <param name="Id_Point">ID точки в котором меняем комментарий</param>
        ///// <param name="Id_Comment">ID необходимого комментария</param>
        //[HttpPost]
        //[ValidateInput(false)]
        //public JsonResult EditComments(string content, int Id_Point, int Id_Comment)
        //{
        //    try
        //    {
        //        Point Pnt = db.Points.Where(p => p.ID.Equals(Id_Point)).Single();        // находим нужный нам Point 
        //        Comment Cmt = Pnt.Comments.Where(c => c.ID.Equals(Id_Comment)).Single(); // Берём необходимый нам комментарий

        //        // если изменения в комментарии были - заменяем на новые и сохраняем в базе.
        //        if (Cmt.ContentText != content && content != "")
        //        {
        //            Cmt.ContentText = content;
        //            db.SaveChanges();
        //        }

        //        return Json("OK");
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        var error = ex.EntityValidationErrors.First().ValidationErrors.First();
        //        return Json(new { ok = false, message = ex.Message });
        //    }
        //}

        /// <summary>Передача во view данных о выбранной "точке" для редактирования</summary>
        /// <param name="id">ID Выбранной "точки"</param>
        /// <returns>Point</returns>
        public ActionResult EditPoint(int id)
        {
            Point p = (from entry in db.Points where entry.ID == id select entry).Single();     // получаем необходимую точку
            Comment c = new Comment();
            c = p.Comments.FirstOrDefault();                                   // передаем первый комментарий к точке как описание
            ViewBag.Description = c.ContentText;

            return View(p);
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult EditPoint(int id, FormCollection collection) // Редактор "точки"
        {
            try
            {
                UserProfile CurAutor = db.GetUSerProfile(User);                                     // получаем автора сообщения
                Point p = (from entry in db.Points where entry.ID == id select entry).Single();     // получаем необходимую точку

                p.Comments.FirstOrDefault().ContentText = collection["FirstComment"];               // изменение "описания"
                p.Defect = db.GetDefect(collection["Defect"]);                                      // изменение "дефекта"

                db.SaveChanges();     

                return RedirectToAction("PointInfo", "Point", new { id = id });                     // перенапрявляем на другой экшен
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("MyError");
            }
        }

        public ActionResult ThanksForPoint()       // благодарность пользователю за добавление новой точки
        {
            return View();
        }

        public ActionResult DeletePoint(int id)          // action to delete point by moderator. Takes ID of point.
        {
            if (Request.IsAuthenticated && Roles.IsUserInRole("Moderator"))     //   if authorized and role is "Moderator"
            {
                Point p = db.Points.Find(id);                                   // find point by ID  
                db.Points.Remove(p);                                            // delete point
                db.SaveChanges();                                              // save the changes in  the database

                ImageHelper.DeleteAllUploadFiles(id);                          // Y.Kovalenko 08/04/2015 delete folder whith uploads foto
                return RedirectToAction("Map", "Home");                        // redirect to main map
            }
            else
                return RedirectToAction("Login", "Account");                              // else redirect to Login action                   
        }

        public ActionResult ConfirmPoint(int id)               // action of point's validation. Takes ID of point.
        {
            if (Request.IsAuthenticated && Roles.IsUserInRole("Moderator"))                    //  if authorized and role is "Moderator"
            {
                Point p = db.Points.Find(id);                                                  // find point by ID  
                p.isValid = true;                                                              // confirm point
                var def = p.Defect;                                                    // prevent strange loss of value Defect. 09.04.15 - Don
                try
                {
                    db.SaveChanges();                                                         // try to save the changes in  the database
                }
                catch(DbEntityValidationException ex)
                {
                    var errorMessages = (from eve in ex.EntityValidationErrors
                                         let entity = eve.Entry.Entity.GetType().Name
                                         from ev in eve.ValidationErrors
                                         select new
                                         {
                                             Entity = entity,
                                             PropertyName = ev.PropertyName,
                                             ErrorMessage = ev.ErrorMessage
                                         });

                    var fullErrorMessage = string.Join("; ", errorMessages.Select(e => string.Format("[Entity: {0}, Property: {1}] {2}", e.Entity, e.PropertyName, e.ErrorMessage)));

                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                }
                return RedirectToAction("Map", "Home");                                        // redirect to main map
            }
            else
                return RedirectToAction("Login", "Account");                                    // else redirect to Login action
        }
        public JsonResult Autocomplete(string term)
        {
            var resultComplete = (from p in db.Points
                                  where p.GeoData.FullAddress.Contains(term)
                                  select p.GeoData.FullAddress).ToArray<string>();
            return Json(resultComplete, JsonRequestBehavior.AllowGet);
        }
    }
}