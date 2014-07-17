using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheAttribute : System.Attribute
    {

        private int seconds_;

        public readonly int seconds
        {
            get
            {
                return seconds_;
            }

        }

        public CacheAttribute(int seconds)
        {
            this.seconds_ = seconds;
        }

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