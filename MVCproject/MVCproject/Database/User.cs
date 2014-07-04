using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{

    public class User : ITable
    {
        [RelatedField]
        public string TableName
        {
            get { return "users"; }
        }

        [PrimaryKeyDefinition(IsPrimaryKey: true, AutoNumeric: true)]
        public int id { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }

    }
}