using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MVCproject
{
    public class FromJSON : IConverter
    {
        public System.Data.DataTable ToDataTable(string source)
        {
            JObject root = JObject.Parse(source);

            DataTable dt = new DataTable();

            // get the content of the geojson object into the object list
            List<List<object>> content = new List<List<object>>();

            foreach(JObject feature in root["features"]) {

                List<object> item = new List<object>();

                item.Add((double)feature["geometry"]["coordinates"][1]); // latitude
                item.Add((double)feature["geometry"]["coordinates"][0]); // longitude

                foreach (var property in (JObject)feature["properties"])
                {
                    switch (property.Value.Type)
                    {
                        case JTokenType.String:
                            item.Add(property.Value.ToString());
                            break;
                        case JTokenType.Integer:
                            item.Add((long)property.Value);
                            break;
                        case JTokenType.Float:
                            item.Add((double)property.Value);
                            break;
                        case JTokenType.Date:
                            item.Add((DateTime)property.Value);
                            break;
                        case JTokenType.Boolean:
                            item.Add((bool)property.Value);
                            break;
                        case JTokenType.None:
                            item.Add(DBNull.Value);
                            break;
                        case JTokenType.Null:
                            item.Add(DBNull.Value);
                            break;
                        case JTokenType.Uri:
                            item.Add(property.Value.ToString());
                            break;
                    }
                }
                

                content.Add(item);
            }

            //create the columns based on the first item

            dt.Columns.Add("latitude", typeof(double));
            dt.Columns.Add("longitude", typeof(double));

            foreach (var property in (JObject)root["features"][0]["properties"])
            {
                switch (property.Value.Type)
                {
                    case JTokenType.String:
                        dt.Columns.Add(property.Key.ToValidName(), typeof(string));
                        break;
                    case JTokenType.Integer:
                        dt.Columns.Add(property.Key.ToValidName(), typeof(long));
                        break;
                    case JTokenType.Float:
                        dt.Columns.Add(property.Key.ToValidName(), typeof(double));
                        break;
                    case JTokenType.Date:
                        dt.Columns.Add(property.Key.ToValidName(), typeof(DateTime));
                        break;
                    case JTokenType.Boolean:
                        dt.Columns.Add(property.Key.ToValidName(), typeof(bool));
                        break;
                    case JTokenType.None:
                        dt.Columns.Add(property.Key.ToValidName());
                        break;
                    case JTokenType.Null:
                        dt.Columns.Add(property.Key.ToValidName());
                        break;
                    case JTokenType.Uri:
                        dt.Columns.Add(property.Key.ToValidName());
                        break;
                }
            }

            // now that we have the columns
            // insert the values into the datatable

            foreach (var entry in content)
            {
                dt.Rows.Add(entry.ToArray());
            }

            return dt;
        }
    }
}