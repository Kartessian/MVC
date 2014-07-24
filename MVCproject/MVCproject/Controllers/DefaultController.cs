using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{

    public class DefaultController : BaseController
    {
        const string user_maps = "user-maps";
        const string user_datasets = "user-datasets";


        private void SetUserSession(List<UserMaps> userMaps, List<MapDataset> mapDatasets)
        {
            Session[user_maps] = userMaps;
            Session[user_datasets] = mapDatasets;
        }

        private List<UserMaps> UserMaps()
        {
            if (Session[user_maps] != null)
            {
                return ((List<UserMaps>)Session[user_maps]);
            }
            else
            {
                return new List<UserMaps>();
            }
        }

        private List<MapDataset> UserDatasets()
        {
            if (Session[user_datasets] != null)
            {
                return ((List<MapDataset>)Session[user_datasets]);
            }
            else
            {
                return new List<MapDataset>();
            }
        }


        [HttpGet]
        public ActionResult Index()
        {
            Default_Index model = new Default_Index();

            //model.styles = database.GetRecords<MapStyle>(); // load all available styles
            
            // get the current user maps
            using (Maps maps = new Maps(database))
            {
                model.maps = maps.UserMaps(user.id);
            }

            // current user available datasets only
            using (Dataset dataset = new Dataset(database))
            {
                model.datasets = dataset.UserActiveList(user.id);
            }

            // it will store the resume in the session as will be access later with other requests
            // this will also help to validate requests from the user
            SetUserSession(model.maps, model.datasets);

            return View(model);
        }

        // skelleton of the available methos that will be needed
        [HttpPost]
        public JsonResult CreateMap(string Name)
        {
            int mapId;
            using (Maps maps = new Maps(database))
            {
                mapId = maps.Create(Name, string.Empty, user.id);
            }

            JsonResult result = new JsonResult();
            result.Data = mapId;
            return result;
        }

        [HttpPost][VerifyOwner]
        public JsonResult DeleteMap(int id)
        {
            using (Maps maps = new Maps(database))
            {
                maps.Delete(id);
            }

            JsonResult result = new JsonResult();
            result.Data = "Ok";
            return result;
        }

        [HttpPost][VerifyOwner]
        public JsonResult UpdateMap(int id, string newName)
        {
            using (Maps maps = new Maps(database))
            {
                maps.Update(id, newName, string.Empty);
            }

            JsonResult result = new JsonResult();
            result.Data = "Ok";
            return result;
        }

        [HttpPost][VerifyOwner]
        public JsonResult SetStyle(int id, string style, int zoom, string center) { return null; }




        [HttpPost]
        public JsonResult CreateDataset(string name)
        {
            return null;
        }

        [HttpPost][VerifyOwner]
        public JsonResult UpdateDataset(int ds, string newName)
        {
            return null;
        }

        [HttpPost][VerifyOwner]
        public JsonResult DeleteDataset(int ds)
        {
            using (Dataset dataset = new Dataset(database))
            {
                dataset.Delete(ds);
            }

            JsonResult result = new JsonResult();
            result.Data = "Ok";
            return result;
        }

        [Cache("ds")]
        public JsonResult GetDataset(int ds)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = UserDatasets().FirstOrDefault(D => D.id == ds);

            return result;
        }

        // planning to cache File Results, combining file cache and mongodb cache, so depending on the 
        // Response it will be using one of the other...
        public FileContentResult DownloadDataset(int ds, string type)
        {

            MapDataset mapDataset = UserDatasets().FirstOrDefault(D => D.id == ds);

            if (mapDataset != null)
            {

                FileContentResult result = null;
                switch (type.ToLower())
                {
                    case "csv":
                        result = new FileContentResult(
                                Encoding.UTF8.GetBytes(database.GetDataTable("select * from datasets.`" + mapDataset.tmpTable + "`").ToCSV())
                                , "text/csv");
                        result.FileDownloadName = mapDataset.name + ".csv";
                        break;

                    case "json":
                        result = new FileContentResult(
                                Encoding.UTF8.GetBytes(database.GetDataTable("select * from datasets.`" + mapDataset.tmpTable + "`").ToGEOJson(mapDataset.latColumn, mapDataset.lngColumn))
                                , "application/vnd.geo+json");
                        result.FileDownloadName = mapDataset.name + ".json";
                        break;
                }
                return result;

            }
            else
            {
                return null;
            }
        }

        [Cache("ds")]
        public ContentResult GetPoints(int ds)
        {
            ContentResult result = new ContentResult();
            result.ContentType = "application/json";


            return result;
        }

        [HttpPost][VerifyOwner]
        public JsonResult SetValue(int ds, string name, int value) { return null; }
        // Alpha, Visible, Etc...


        [HttpPost]
        public ContentResult GetPoint(int ds, int point) {

            MapDataset mapDataset = UserDatasets().FirstOrDefault(D => D.id == ds);

            ContentResult result = new ContentResult();
            result.ContentType = "application/json";

            using (Dataset dataset = new Dataset(database)) {

                result.Content = dataset.GetPoint(mapDataset.tmpTable, point).ToJson();
            
            }

            return result;
        }

        [HttpPost]
        public JsonResult AddPoint(int ds, string point) { return null; }

        [HttpPost]
        public JsonResult DeletePoint(int ds, int point) { return null; }


        /// <summary>
        /// return a list with all available public datasets
        /// </summary>
        /// <param name="search">search string to filter the list</param>
        [HttpPost]
        public JsonResult PublicDatasets(string search)
        {

            JsonResult result = new JsonResult();

            using (Dataset dataset = new Dataset(database))
            {
                result.Data = dataset.PublicList();
            }

            return result;
        }


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
