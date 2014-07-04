using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class HomeController : BaseController
    {
        [OmitDatabase()]
        public ActionResult Index()
        {
            // There is no need to connect to the database at this point at all as only static content is being displayed
            return View();
        }

        /// <summary>
        /// returns a json string with all available users in the "users" table
        /// </summary>
        [Cache()]
        //[OutputCache(Duration=60)]
        public ContentResult getUsers()
        {
            // The result of this request will be cached and next time will be returned instead of being created again
            // It can be convined with the client cache so it will also reduce the number of the calls to the server

            ContentResult result = new ContentResult();
            result.ContentType = "application/json";
            result.Content = database.fillDataTable("select * from users").ToJson();

            return result;
        }

    }
}
