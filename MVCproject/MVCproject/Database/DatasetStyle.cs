using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public class DatasetStyle : ITable
    {

        [RelatedField]
        [System.Web.Script.Serialization.ScriptIgnore] // prevent send this property when serializend the class to the client
        public string TableName
        {
            get { return "dataset_dots"; }
        }

        [PrimaryKeyDefinition(true,false)]
        public int dataset_id {get; set;}

        [PrimaryKeyDefinition(true, false)]
        public int map_id { get; set; }

        [DefaultValue("plain")]
        public string type { get; set; }
        [DefaultValue("#ff6767")]
        public string color1 { get; set; }
        [DefaultValue("#b92121")]
        public string color2 { get; set; }
        [DefaultValue(7)]
        public int size { get; set; }
        [DefaultValue(255)]
        public int alpha { get; set; }
        [DefaultValue(true)]
        public bool adjustzoom { get; set; }

        public DatasetStyle() { }
    }
}