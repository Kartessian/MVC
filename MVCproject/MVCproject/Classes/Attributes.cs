using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheAttribute : System.Attribute
    {
        public CacheAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OmitDatabaseAttribute : System.Attribute
    {
        public OmitDatabaseAttribute() { }
    }
}