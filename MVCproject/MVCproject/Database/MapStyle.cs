using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public class MapStyle : ITable
    {
        [RelatedField]
        public string TableName
        {
            get { return "Styles"; }
        }

        [PrimaryKeyDefinition(true, true)]
        public int id { get; set; }
        public string name { get; set; }
        public string style { get; set; }

        public MapStyle() { }

        public MapStyle(DataRow r)
        {
            id = (int)r["id"];
            name = r["name"].ToString();
            style = r["style"].ToString();
        }

    }
}