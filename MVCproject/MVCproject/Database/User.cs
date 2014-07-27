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

        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public DateTime createdon { get; set; }
        public DateTime lastvisit { get; set; }
        public string createdIP { get; set; }
        public string lastVisitIP { get; set; }


    }
}