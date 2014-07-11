using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class DeftaulController : BaseController
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
        public ContentResult getUsers()
        {
            // The result of this request will be cached and next time will be returned instead of being created again
            // It can be convined with the client cache so it will also reduce the number of the calls to the server

            ContentResult result = new ContentResult();
            result.ContentType = "application/json";

            // the database class offers two different ways to access the data into the database
            // (comment the one you are not going to use)

            // #1 write your own query
            result.Content = database.GetDataTable("select * from users").ToJsonTable();

            // #2 use an ITable object to query the database
            result.Content = database.GetRecords<User>().ToJsonTable<User>();

            return result;
        }

        [OmitDatabase()]
        public ContentResult test()
        {
            ContentResult result = new ContentResult();
            result.ContentType = "application/json";

            //Converters c = new Converters(System.IO.File.ReadAllText(@"C:\Temp\CSV\20140306200349-293432.csv"));
            //result.Content = c.GetDataTable<FromCSV>().ToJson();

            //Converters c = new Converters(System.IO.File.ReadAllText(@"C:\Temp\eq.json"));
            //result.Content = c.GetDataTable<FromJSON>().ToJsonTable();

            //Converters c = new Converters(@"C:\Temp\temp.xlsx");
            //result.Content = c.GetDataTable<FromExcel>().ToJsonTable();
            
            
            return result;
        }
    }
}
