using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCproject
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheAttribute : System.Attribute
    {

        private int seconds_;

        private string datasetParameter_;

        public int seconds
        {
            get
            {
                return seconds_;
            }

        }

        public string datasetParameter
        {
            get
            {
                return datasetParameter_;
            }
        }

        public CacheAttribute(string datasetParameter)
        {
            this.datasetParameter_ = datasetParameter;
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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class VerifyOwnerAttribute : AuthorizeAttribute
    {

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["user"] != null && httpContext.Session["user-maps"] != null)
            {

                User user = (User)httpContext.Session["user"];

                // get the datasetId from the parameters
                string datasetId = httpContext.Request["ds"];
                // get the mapId from the parameters
                string mapId = httpContext.Request["id"];

                // the idea is try to find the datasetIdfrom the datasets available for the user
                // that were stored in session using the Default_Index model -- open to improvements

                if (!string.IsNullOrEmpty(mapId))
                {
                    List<UserMaps> userMaps = (List<UserMaps>)httpContext.Session["user-maps"];
                    int map = int.Parse(mapId);

                    return userMaps.Any(M => M.id == map);
                }

                if (!string.IsNullOrEmpty(datasetId))
                {

                    List<MapDataset> userDs = (List<MapDataset>)httpContext.Session["user-datasets"];
                    int ds = int.Parse(datasetId);

                    return userDs.Any(D => D.id == ds);
                }


                return false;
            }

            return base.AuthorizeCore(httpContext);
        }
    }
}