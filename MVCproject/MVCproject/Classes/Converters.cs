using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCproject
{

    public class Converters
    {

        string source_;

        public Converters(string source)
        {
            this.source_ = source;
        }


        public DataTable GetDataTable<T>() where T:IConverter, new()
        {

            return new T().ToDataTable(source_);
        }

    }
}