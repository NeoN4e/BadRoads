﻿using BadRoads.Filters;
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

        // экшен, который принимает данные с формы для создания новой точки
        
        [HttpPost]
        public ActionResult Add(FormCollection collection)
        {
            return RedirectToAction("Index", "Home");
        }

        /// <summary>  Передача во "view" данных о выбраной "точке"  </summary>
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

        public ActionResult Gallery()
        {
            return View();
        }

        public ActionResult Test()   // отображение основной карты со всеми сохраненными точками
        {
            List<Point> listPoints = db.Points.ToList<Point>();   //список всех точек в базе
            Point p = listPoints[0];

            Comment c = new Comment();
            c.ContentText = "Start";

            p.Comments.Add(c);
            db.SaveChanges();

            ViewBag.Content = p.Comments.ElementAt(0).ContentText;

            return View(p);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Edit(string content, int Id_P, string Id_C) 
        {
            List<Point> listPoints = db.Points.ToList<Point>();//список всех точек в базе
            listPoints[0].Comments.ElementAt(0).ContentText = content;
            db.SaveChanges();
            //Point p = (from entry in db.Points where entry.ID == P_Id select entry).Single();
            //p.Comments.ElementAt(C_Id).ContentText = content;
            return Json("OK");
        }
    }
}