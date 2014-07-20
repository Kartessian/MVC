using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace MVCproject
{
    public static class Helpers
    {

        public static string GenerateUniqueName()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string uniqueName = new string(
                    Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray()
                );

            return uniqueName;
        }

        public static void SendEmail(string email, string subject, string body, string from = null)
        {

            using (System.Net.Mail.MailMessage oMail = new System.Net.Mail.MailMessage())
            {
                oMail.From = new MailAddress("info@kartessian.com", "no-reply");
                oMail.To.Add(new MailAddress(email));
                oMail.Subject = subject;
                oMail.IsBodyHtml = true;
                oMail.Priority = MailPriority.Normal;
                if (!string.IsNullOrEmpty(from))
                {
                    oMail.ReplyToList.Add(new MailAddress(from));
                }

                string template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("/Templates") + "\\email.html");

                oMail.Body = template.Replace("{BODY}", body);

                SmtpClient mClient = new SmtpClient(
                    ConfigurationManager.AppSettings["smtp_server"].ToString()
                    , 587);
                mClient.EnableSsl = true;
                mClient.UseDefaultCredentials = false;
                mClient.Credentials = new System.Net.NetworkCredential(
                    ConfigurationManager.AppSettings["smtp_user"].ToString(),
                    ConfigurationManager.AppSettings["smtp_pwd"].ToString());

                mClient.Send(oMail);
                mClient = null;	// free up resources
            }
        }

    }
}