using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{

    public class DefaultController : BaseController
    {
        
        [HttpGet]
        public ActionResult Index()
        {
            Default_Index model = new Default_Index();

            model.styles = database.GetRecords<MapStyle>(); // load all available styles
            model.maps = null; // TODO -> get the current user maps

            return View(model);
        }

        // skelleton of the available methos that will be needed
        [HttpPost]
        public JsonResult CreateMap(string Name)
        {
            return null;
        }

        [HttpPost]
        public JsonResult DeleteMap(int id)
        {
            return null;
        }

        [HttpPost]
        public JsonResult UpdateMap(int id, string newName)
        {
            return null;
        }

        [HttpPost]
        public JsonResult SetStyle(int id, string style, int zoom, string center) { return null; }




        [HttpPost]
        public JsonResult CreateDataset(string name)
        {
            return null;
        }

        [HttpPost]
        public JsonResult UpdateDataset(int id, string newName)
        {
            return null;
        }

        [Cache()]
        public ContentResult LoadDataset(int id)
        {
            return null;
        }

        [HttpPost]
        public JsonResult DeleteDataset(int id)
        {
            return null;
        }

        [Cache()]
        public ContentResult DownloadDataset(int id, string type)
        {
            return null;
        }

        [HttpPost]
        public JsonResult SetValue(int id, string name, int value) { return null; }
        // Alpha, Visible, Etc...

        [HttpPost][Cache()]
        public ContentResult GetPoint(int id, int ds) { return null; }

        [HttpPost]
        public JsonResult AddPoint(int ds, string point) { return null; }

        [HttpPost]
        public JsonResult DeletePoint(int ds, int id) { return null; }


        [ValidateInput(false)]
        [OmitDatabase()]
        [Cache(15)] // to prevent abuses, it will cache the results for 15 seconds.
        public ContentResult proxy(string url) {
            ContentResult result = new ContentResult();
            result.ContentType = "application/json";

            try
            {
                WebClient c = new WebClient();
                c.Headers.Add("User-Agent", ".Net 4 WebClient Component");
                result.Content = c.DownloadString(url);
            }
            catch (Exception ex)
            {
                result.Content = "{\"Error\":\"" + ex.Message.Replace("\"", "'") + "\"}";
            }

            return result;
        }

        [ValidateInput(false)]
        [OmitDatabase()]
        [Cache(15)] // to prevent abuses, it will cache the results for 15 seconds.
        public ContentResult analyze(string url) {
            ContentResult result = new ContentResult();
            result.ContentType = "application/json";

            string data;
            try
            {
                WebClient c = new WebClient();
                c.Headers.Add("User-Agent", ".Net 4 WebClient Component");
                data = c.DownloadString(url);

                var objects = Newtonsoft.Json.Linq.JArray.Parse(data);
                List<string> fields = new List<string>();
                foreach (Newtonsoft.Json.Linq.JToken root in objects)
                {
                    foreach (Newtonsoft.Json.Linq.JToken app in root)
                    {
                        fields.Add("\"" + ((Newtonsoft.Json.Linq.JProperty)(app)).Name + "\"");
                    }
                    break;
                }
                data = "[{\"fields\":[" + String.Join(",", fields.ToArray()) + "],\"count\":" + objects.Count() + "}]";
            }
            catch (Exception ex)
            {
                data = "{\"Error\":\"" + ex.Message.Replace("\"", "'") + "\"}";
            }

            result.Content = data;

            return result;
        }
    }
}
