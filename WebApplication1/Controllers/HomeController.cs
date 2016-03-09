using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }
        public ActionResult View1()
        {
            return View();
        }
        public ActionResult View2()
        {
            return View();
        }
        public ActionResult View3()
        {
            return View();
        }
        public ActionResult View4()
        {
            return View();
        }
        public ActionResult View5()
        {
            return View();
        }
        public ActionResult View6()
        {
            return View();
        }
        public ActionResult Upload(HttpPostedFileBase uploadImg, HttpPostedFileBase uploadVid, HttpPostedFileBase uploadAud, string userID)
        {
            if (uploadImg != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(uploadImg.FileName);
                // сохраняем файл в папку Files в проекте
                uploadImg.SaveAs(Server.MapPath("~/Files/Img/" + fileName));
            }
            if (uploadAud != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(uploadAud.FileName);
                // сохраняем файл в папку Files в проекте
                uploadAud.SaveAs(Server.MapPath("~/Files/Aud/" + fileName));
            } 
            if (uploadVid != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(uploadVid.FileName);
                // сохраняем файл в папку Files в проекте
                uploadAud.SaveAs(Server.MapPath("~/Files/Vid/" + fileName));
            }

            return RedirectToAction(userID);
        }
    }
}
