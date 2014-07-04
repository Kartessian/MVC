using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MVCproject
{
    public static class Extensions
    {
        /// <summary>
        /// Parses a DataTable object into a JSON string
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this DataTable dt)
        {
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

    }
}