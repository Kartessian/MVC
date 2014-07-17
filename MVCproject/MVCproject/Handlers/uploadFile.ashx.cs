using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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
            // resonse will be a valid json
            context.Response.ContentType = "application/json";

            // if there is no session, exit and 
            // validate the form to be sure there is one file only
            if (context.Session["user"] == null
                || context.Request.Files.Count != 1
                )
            {
                context.Response.Write("\"Error\"");
                return;
            }

            int userId = (int)context.Session["user"];

            int mapId = int.Parse(context.Request.Form["map"]);
            string mimeType = context.Request.Form["type"];

            // do not use the mimetype from the uploaded file, it uses the type 
            // passed from the client, as there are always possibilities for the user
            // to have a different extension in the file than what the mimetype will 
            // tell. Mostly with JSON based files. Also some CSV files are being sent as .txt

            // I'm interested in temporarely save the files into a temp folder so I have 
            // some examples of files to test and evaluate the code and further revisions 
            // or changes / needs ...
            string tmp_folder = ConfigurationManager.AppSettings["temp_folder"].ToString();

            var file = context.Request.Files[0];

            // custom file name to get when it was uploaded, the original filename and the specified mimetype
            string tmp_file_name = DateTime.Now.ToString("yyyyMMddHHmmss.") + file.FileName + "." + mimeType;

            file.SaveAs(tmp_folder + tmp_file_name);

            DataTable table;

            switch (mimeType.ToLower())
            {
                case "csv":
                    table = Converters.GetDataTable<FromCSV>(
                            new StreamReader(file.InputStream).ReadToEnd()
                        );
                    break;
                case "json":
                    table = Converters.GetDataTable<FromJSON>(
                            new StreamReader(file.InputStream).ReadToEnd()
                        );
                    break;
                case "excel":
                    table = Converters.GetDataTable<FromExcel>(tmp_folder + tmp_file_name);
                    break;
                default:
                    context.Response.Write("{\"Error\":\"specified mimetype is not supported\"}");
                    return;
            }

            // next step is put the datatable into the database for the user that uploaded the file
            // no reason to start the database connection before in case something failed
            // need to add some validations and try/catch before

            Database db = new Database(ConfigurationManager.AppSettings["connectionString"].ToString());

            using (Dataset dataset = new Dataset(db))
            {
                int new_dataset_id = dataset.CreateFromTable(table, userId, tmp_file_name);

                if (new_dataset_id > 0)
                {
                    context.Response.Write(
                                            string.Format("{\"id\":{0},\"rows\":{1}}", new_dataset_id, table.Rows.Count)
                                        );
                }
                else
                {
                    context.Response.Write("\"Error\"");
                }
            }

            db.Dispose();

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