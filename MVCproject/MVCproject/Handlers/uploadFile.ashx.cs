using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace MVCproject.Handlers
{
    /// <summary>
    /// Summary description for uploadFile
    /// </summary>
    public class uploadFile : IHttpHandler, IReadOnlySessionState
    {

        public void ProcessRequest(HttpContext context)
        {

            // if there is no session, exit and 
            // validate the form to be sure there is one file only
            if (context.Session["user"] == null
                || context.Request.Files.Count != 1
                )
            {
                context.Response.Write("\"Error\"");
                return;
            }

            int mapId = int.Parse(context.Request.Form["map"]);
            string mimeType = context.Request.Form["type"];

            // do not use the mimetype from the uploaded file, it uses the type 
            // passed from the client, as there are always possibilities for the user
            // to have a different extension in the file than what the mimetype will 
            // tell. Mostly with JSON based files. Also some CSV files are being sent as .txt

            DataTable table;

            switch (mimeType.ToLower())
            {
                case "csv":
                    table = Converters.GetDataTable<FromCSV>("");
                    break;
                case "json":
                    table = Converters.GetDataTable<FromJSON>("");
                    break;
            }

            //context.Response.ContentType = "application/json";
            //context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}