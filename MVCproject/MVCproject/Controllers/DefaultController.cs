﻿using System;
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
        
        [HttpGet]
        public ActionResult Index()
        {
            Default_Index model = new Default_Index();

            //model.styles = database.GetRecords<MapStyle>(); // load all available styles
            model.maps = database.GetRecords<UserMaps>(); // get the current user maps
            model.datasets = database.GetRecords<MapDataset>(); // user datasets

            // it will store the resume in the session as will be access later with other requests
            // this will also help to validate requests from the user
            Session["user-maps"] = model.maps;
            Session["user-datasets"] = model.datasets;

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
            MapDataset mapDataset = ((List<MapDataset>)Session["user-datasets"]).First(D => D.id == ds);

            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = mapDataset;

            return result;
        }

        // planning to cache File Results, combining file cache and mongodb cache, so depending on the 
        // Response it will be using one of the other...
        public FileContentResult DownloadDataset(int ds, string type)
        {
            // need to improve the way the data is stored in session
            // is not the best way... thought it works.
            MapDataset mapDataset = ((List<MapDataset>)Session["user-datasets"]).First(D => D.id == ds);

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

            MapDataset mapDataset = ((List<MapDataset>)Session["user-datasets"]).First(D => D.id == ds);

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
