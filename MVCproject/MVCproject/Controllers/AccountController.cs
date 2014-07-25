using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class AccountController : BaseController
    {
        const string imHere_Register = "imHere";
        const string imHere_Login = "imHereToLog";
        const string imHere_Password = "imHerePwd";

        [HttpGet]
        [OmitDatabase]
        public ActionResult Login()
        {
            Session[imHere_Login] = true;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Login(string username, string password, string remember)
        {

            if (Session[imHere_Login] == null)
            {
                return RedirectToAction("Login");
            }

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                using (Account account = new Account(database))
                {
                    var user = account.GetUser(username, password);
                    if (user != null)
                    {
                        // be sure the session is empty before continuing
                        Session.Clear();
                        
                        Session["user"] = user;
                        account.SetLastVisit(user.id, RequestIP());

                        return RedirectToAction("Index", "Editor");
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

            // Ready to do login again
            Session[imHere_Login] = true;

            // Shows the login page
            return View("Login");
        }

        [HttpGet]
        [OmitDatabase]
        public ActionResult Register()
        {
            // The Session variable imHere is used to prevent a post without first come to the register page
            // that should help prevent for automatic posting calling directly the register action
            Session[imHere_Register] = true;
            return View();
        }

        [ValidateInput(false)]
        public ActionResult Register(string email, string[] password, string name)
        {
            ViewBag.email = email;
            ViewBag.name = name;

            if (Session[imHere_Register] == null)
            {
                return RedirectToAction("Register");
            }

            if (!string.IsNullOrEmpty(email) && password != null && !string.IsNullOrEmpty(name))
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

                        Session.Remove(imHere_Register);
                    }
                    catch
                    {
                        ViewBag.error = "An error occurred creating your account. Check your details.";
                        database.RollBack();
                    }
                }


            }

            return View();
        }

        [HttpGet]
        [OmitDatabase]
        public ActionResult PasswordLost()
        {
            // The Session variable imHere is used to prevent a post without first come to the register page
            // that should help prevent for automatic posting calling directly the register action
            Session[imHere_Password] = true;
            return View();
        }

        public ActionResult PasswordLost(string email)
        {

            if (Session[imHere_Password] == null)
            {
                return RedirectToAction("PasswordLost");
            }

            if (!string.IsNullOrEmpty(email))
            {

                using (Account account = new Account(database))
                {

                    if (account.EmailExist(email))
                    {

                        User user = account.GetUser(email);

                        if (user != null)
                        {
                            string sbody = "<p>You are receiving this message because you asked to remember your password.</p>" +
                            "<p>Please use the information below to log into your kartessian account where you will be able to change your password.</p>" +
                            "<p>E-mail: " + email + "</p><p>Password: " + user.password.ToString() + "</p>";

                            Helpers.SendEmail(email, "kartessian password recovery", sbody);

                            ViewBag.result = "OK";
                            Session.Remove(imHere_Password);
                        }
                    }
                    else
                    {

                        ViewBag.result = "NO";

                    }
                }
            }

            return View();
        }

    }
}
