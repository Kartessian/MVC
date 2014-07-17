using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class EmbedController : BaseController
    {

        public ActionResult Mapbox(int id)
        {
            return View();
        }

        public ActionResult GoogleMaps(int id)
        {
            return View();
        }

    }
}
