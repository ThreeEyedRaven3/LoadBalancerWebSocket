using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Web;
using System.Web.Mvc;

namespace ServerWorker1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public EmptyResult DoSmth()
        {
            int sum = 0;
            for (int j = 0; j < 1000; j++)
                for (int i = 0; i < 10000000; i++)
                    { sum++; sum--; }
            // ничего не возвращаем
            return new EmptyResult();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}