﻿using System;
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


            // it will store the resume in the session as will be access later with other requests
            // this will also help to validate requests from the user
            Session["user-maps"] = model.maps;

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
            return null;
        }

        [Cache("ds")]
        public ContentResult GetDataset(int ds)
        {
            return null;
        }

        [Cache("ds")]
        public ContentResult DownloadDataset(int id, string type)
        {
            return null;
        }

        [Cache("ds")]
        public ContentResult GetPoints(int ds)
        {
            return null;
        }

        [HttpPost][VerifyOwner]
        public JsonResult SetValue(int ds, string name, int value) { return null; }
        // Alpha, Visible, Etc...


        [HttpPost]
        public ContentResult GetPoint(int ds, int point) { return null; }

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
