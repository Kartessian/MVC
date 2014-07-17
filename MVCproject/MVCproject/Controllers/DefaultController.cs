using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{

    public class DefaultController : BaseController
    {

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




        [OmitDatabase()]
        public ContentResult proxy(string url) { return null; }

        [OmitDatabase()]
        public ContentResult analyze(string url) { return null; }
    }
}
