using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class AccountController : BaseController
    {
        public ActionResult Login(string username, string password)
        {
            return View();
        }

        public ActionResult Logout()
        {
            return View();
        }

        public ActionResult UpdateAccount(string username, string password)
        {
            return View();
        }
    }
}
