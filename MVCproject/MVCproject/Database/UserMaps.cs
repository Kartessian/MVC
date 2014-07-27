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

        public string name { get; set; }
        public string access { get; set; }
        public DateTime createdOn { get; set; }

        public UserMaps() { }

    }
}