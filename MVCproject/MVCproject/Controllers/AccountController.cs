using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class AccountController : BaseController
    {
        [ValidateInput(false)]
        public ActionResult Login(string username, string password, string remember)
        {

            if (!String.IsNullOrEmpty(username) || !String.IsNullOrEmpty(password))
            {
                using (Account account = new Account(database))
                {
                    var user = account.GetUser(username, password);
                    if (user != null)
                    {
                        Session["user"] = user;
                        account.SetLastVisit(user.id, RequestIP());
                        return RedirectToAction("Index", "Default");
                    }
                    else
                    {
                        ViewBag.error = "Email or Password not valid";
                    }
                }
            }

            return View();
        }

        [OmitDatabase]
        public ActionResult Logout()
        {
            // Clean the session, that will do the job
            Session.Clear();

            // Shows the login page
            return View("Login");
        }

    }
}
