using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace MVCproject
{
    public static class Extensions
    {
        /// <summary>
        /// This function will tranform the string into a valid Table Name for a SQL database
        /// </summary>
        public static string ToValidName(this string value) {
            // still lot of work to do, but some basics.
            return value.Replace(" ", "_").Replace("-", "_").Trim();
        }

        /// <summary>
        /// Parses a DataTable object into a JSON string
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this DataTable dt)
        {
            // need to convert the datatable object in something that can be seralized
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return serializer.Serialize(rows);
        }

        /// <summary>
        /// Serialize a List of ITable into a JSON string
        /// </summary>
        public static string ToJson(this List<ITable> source)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(source);
        }

        /// <summary>
        /// Serialize an ITable into a JSON string
        /// </summary>
        public static string ToJson(this ITable source)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(source);
        }

        /// <summary>
        ///  Serialize a DataTable into a JSON string in two objects: columns and data
        ///  Columns contains an array of all the columns in the datatable
        ///  Data contains and Array of Arrays with the content of the data table
        ///  The response is supposed to be smaller than the ToJson(DataTable) method.
        /// </summary>
        public static string ToJsonTable(this DataTable dt)
        {

            StringBuilder sb = new StringBuilder();

            foreach (DataColumn column in dt.Columns)
            {
                sb.Append(",'" + column.ColumnName + "'");
            }

            List<object[]> data = new List<object[]>();
            foreach(DataRow row in dt.Rows) {
                data.Add(row.ItemArray);
            }

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            string json = "{\"columns\":[" + sb.ToString().Substring(1) + "],\"data\":" + serializer.Serialize(data) + "}";

            return json;
        }

        /// <summary>
        ///  Serialize a DataTable into a JSON string in two objects: columns and data
        ///  Columns contains an array of all the columns in the datatable
        ///  Data contains and Array of Arrays with the content of the data table
        ///  The response is supposed to be smaller than the ToJson(DataTable) method.
        /// </summary>
        /// <typeparam name="T">Needed in case the List if empty to at least return the properties</typeparam>
        public static string ToJsonTable<T>(this List<T> source) where T: ITable
        {
            StringBuilder sb = new StringBuilder();
            List<string> available_properties = new List<string>();

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(P => P.Name != "TableName"))
            {
                sb.Append(",'" + property.Name + "'");
                available_properties.Add(property.Name);
            }


            List<object[]> data = new List<object[]>();
            foreach (ITable item in source)
            {
                object[] values = new object[available_properties.Count];

                var type = item.GetType();

                for (int i = 0; i < available_properties.Count; i++)
                {
                    values[i] = type.GetProperty(available_properties[i]).GetValue(item, null);
                }
                data.Add(values);
            }

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            string json = "{\"columns\":[" + sb.ToString().Substring(1) + "],\"data\":" + serializer.Serialize(data) + "}";

            return json;
        }

        /// <summary>
        /// Serialize a DataTable into a GeoJSON string
        /// </summary>
        /// <param name="dt">DataTable to serialize</param>
        /// <param name="latColumn">column for latitude</param>
        /// <param name="lngColumn">column for longitude</param>
        public static string ToGEOJson(this DataTable dt, string latColumn, string lngColumn)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder line;

            foreach (DataRow r in dt.Rows)
            {
                line = new StringBuilder();


                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName != latColumn && col.ColumnName != lngColumn)
                    {
                        string cValue = r[col].ToString();
                        line.Append(",\"" + col.ColumnName + "\":\"" + cValue.Replace("\"", "\\\"") + "\"");
                    }

                }

                result.Append(
                    ",{\"type\":\"Feature\",\"geometry\": {\"type\":\"Point\", \"coordinates\": [" + r[lngColumn].ToString() + "," + r[latColumn].ToString() + "]},\"properties\":{" +
                    line.ToString().Substring(1) + "}}");

            }

            string geojson = "{\"type\": \"FeatureCollection\",\"features\": [" +
                result.ToString().Substring(1) + "]}";

            return geojson;
        }

        /// <summary>
        /// Serializes a DataTable to CSV
        /// </summary>
        public static string ToCSV(this DataTable dt)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder line = new StringBuilder();

            foreach (DataColumn col in dt.Columns)
            {

                line.Append("," + col.ColumnName);
            }

            result.AppendLine(line.ToString().Substring(1));

            foreach (DataRow r in dt.Rows)
            {
                line = new StringBuilder();

                foreach (DataColumn col in dt.Columns)
                {

                    string cValue = r[col].ToString();

                    if (cValue.Contains(","))
                    {
                        line.Append(",\"" + cValue + "\"");
                    }
                    else
                    {
                        line.Append("," + cValue);
                    }


                }

                result.AppendLine(line.ToString().Substring(1));

            }

            return result.ToString();
        }
    }
}