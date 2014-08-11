using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public class MapDataset: ITable
    {
        [RelatedField]
        [System.Web.Script.Serialization.ScriptIgnore] // prevent send this property when serializend the class to the client
        public string TableName
        {
            get { return "Datasets"; }
        }

        [PrimaryKeyDefinition(true, true)]
        public int id { get; set; }

        public string name { get; set; }
        
        [System.Web.Script.Serialization.ScriptIgnore] // prevent send this property when serializend the class to the client
        public string tmpTable { get; set; } // name of the table in the datasets schema
        
        [System.Web.Script.Serialization.ScriptIgnore] // prevent send this property when serializend the class to the client
        public string latColumn { get; set; } // name of the latitude column in the table

        [System.Web.Script.Serialization.ScriptIgnore] // prevent send this property when serializend the class to the client
        public string lngColumn { get; set; } // name of the longigute column in the table

        public bool visible { get; set; }

        [RelatedField]
        public DatasetStyle style { get; set; } // custom style based on the selected map

        public MapDataset() {}
    }
}