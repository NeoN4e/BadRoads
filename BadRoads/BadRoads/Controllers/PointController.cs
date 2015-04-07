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

        // экшен, который принимает данные с формы для создания новой точки

        [HttpPost]
        [Authorize]
        public ActionResult Add(FormCollection collection, IEnumerable<HttpPostedFileBase> upload)
        {
            UserProfile CurAutor = db.GetUSerProfile(User);
            string lat = collection["latitude"];
            if(lat.Count()>10)
            { 
                lat = lat.Substring(0, 10);
            }
            lat = lat.Replace(".", ",");
            string lng = collection["longitude"];
            if (lng.Count() > 10)
            {
                lng = lng.Substring(0, 10);
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
            List<string> fileList = ImageHelper.SaveUploadFiles(id, upload); // Метод сохранения фотки
            return RedirectToAction("ThanksForPoint", "Point");    // переход в экшен ThanksForPoint
            foreach (var item in fileList)
            {
                p.AddPhoto(new Photo() { Url = item.ToString() }); // запись ссылки на фото в таблицу ФОТО
            }
            p.Cover = p.Photos.First(); // запись ссылки на фото в кавер для галлереи
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>Передача во "view" данных о выбранной "точке" </summary>
        /// <param name="id">ID Выбранной "точке"</param>
        /// <returns>Point</returns>
        public ActionResult PointInfo(int id)                  // экшен выводит описание одной точки
        {
            Point p = (from entry in db.Points where entry.ID == id select entry).Single();     // получаем необходимую точку
            Comment c = new Comment();
            c = p.Comments.FirstOrDefault();                                   // передаем первый комментарий к точке как описание
            if(c != null)
            {
                ViewBag.Description = c.ContentText;
            }
            else
            {
                ViewBag.Description = "Нет описания к проблеме.";
            }
            return View(p);
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
                var def = p.Defect;     //дополнительное обращение к полю Defect позволяет избежать DbEntityValidationException, связанную со странной потерей данных в поле Defect 07.04.15 Дон
                try
                {
                    db.SaveChanges();            // сохраняем изменения в базе !!!! НЕ СОХРАНЯЕТСЯ   выкидывает исключение!!!!!!!
                }
                catch (DbEntityValidationException ex)  //обработка Исключения валидации БД 07.04.15 Дон
                {
                    var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                    this.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    return View();
                }

                return RedirectToAction("Map", "Home");                                        // переход на основную карту
            }
            else
                return RedirectToAction("Login", "Account");                              // иначе перенаправляем к экшену авторизации
        }
    }
}