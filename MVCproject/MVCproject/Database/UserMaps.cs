using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public class UserMaps: ITable
    {
        [RelatedField]
        public string TableName
        {
            get { return "Maps"; }
        }

        [PrimaryKeyDefinition(true, true)]
        public int id { get; set; }

        public UserMaps() { }

        public UserMaps(DataRow r)
        {
            id = (int)r["id"];
        }

    }
}