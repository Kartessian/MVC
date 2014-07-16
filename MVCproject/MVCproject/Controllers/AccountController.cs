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

        [ValidateInput(false)]
        public ActionResult Resgiter(string email, string[] password, string name)
        {
            ViewBag.email = email;
            ViewBag.name = name;

            if (!string.IsNullOrEmpty(email) && password != null && !string.IsNullOrEmpty(name))
            {
                if (Session["imHere"] == null)
                {
                    return RedirectToAction("Register");
                }
                else
                {


                    if (password.Length == 2)
                    {
                        if (password[0] != password[1])
                        {
                            ViewBag.error = "The passwords entered do not match!";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.error = "The passwords entered do not match!";
                        return View();
                    }

                    using (Account account = new Account(database))
                    {

                        try
                        {

                            database.BeginTransaction();

                            if (account.EmailExist(email))
                            {
                                ViewBag.error = "The email already exist. Did you forget your password?";
                                return View();
                            }

                            account.CreateUser(email, name, password[0], RequestIP());

                            string sbody = "<p>Thanks for signing up!</p>" +
                                "<p>Your account is ready now, and you can log in and start creating awesome maps.<p>" +
                                "<p>Your login details are:</p>" +
                                "<p>Email: " + email + "</p>" +
                                "<p>Password: " + password + "</p><br/><br/><p>Contact us at info@kartessian.com if you have any question.</p>" +
                                "<p>Kind Regards,<br/>Kartessian</p>";

                            Helpers.SendEmail(email, "Welcome to Kartessian, " + name + "!", sbody);

                            ViewBag.error = "Your account have been created. We've sent you an email with your login details.";
                            database.Commit();
                        }
                        catch
                        {
                            ViewBag.error = "An error occurred creating your account. Check your details.";
                            database.RollBack();
                        }
                    }

                }
            }
            else
            {
                Session["imHere"] = true;
            }

            return View();
        }

    }
}
