using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCproject
{

    public static class Converters
    {

        public static DataTable GetDataTable<T>(string content) where T:IConverter, new()
        {
            return new T().ToDataTable(content);
        }

    }
}