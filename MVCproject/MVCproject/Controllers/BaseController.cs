﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MVCproject.Controllers
{
    public class BaseController : Controller
    {
        protected bool cache_enabled = false;
        protected bool cache_attribute = false;
        protected string cache_ID = null;
        protected bool omit_database = false;

        protected Database database = null;

        protected ICache cache = null;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // read configuration settings
            cache_enabled = ConfigurationManager.AppSettings["cache_enabled"].ToString() == "true";

            // check custom attributes - if any
            cache_attribute = filterContext.ActionDescriptor.GetCustomAttributes(typeof(CacheAttribute), false).FirstOrDefault() != null;
            omit_database = filterContext.ActionDescriptor.GetCustomAttributes(typeof(OmitDatabaseAttribute), false).FirstOrDefault() != null;

            // if the attribute cache is present and the cache is enabled in the parameter in the web.config file
            if (cache_enabled && cache_attribute)
            {

                cache_ID = filterContext.ActionDescriptor.ActionName + "_" + string.Join("_", filterContext.ActionParameters.Values);

                cache = new MongoCache();
                
                String cached = cache.Get(cache_ID);

                // if the item is already in the "cache"
                if (cached != null)
                {
                    var result = new ContentResult();
                    result.ContentType = "application/json";
                    result.Content = cached;

                    filterContext.Result = result;
                    return;
                }
                
            }

            // initiate the database only if it is needed
            if (!omit_database)
            {
                database = new Database(ConfigurationManager.AppSettings["connectionString"].ToString());
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (database != null) database.Dispose();


            // if the cache is enabled, after processing the request it will create a temporary file with the result
            // so next time is being accesed, it will return the file directly
            if (cache != null)
            {
                //working only for now for ContentResult
                if (filterContext.Result is ContentResult)
                {
                    cache.Add(new MongoCacheObject(cache_ID, ((System.Web.Mvc.ContentResult)(filterContext.Result)).Content, DateTime.MaxValue));
                }
            }

            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// Returns the IP address used by the client (or proxy)
        /// </summary>
        /// <returns>String: IP Address</returns>
        public string RequestIP()
        {
            string sIP = Request.ServerVariables["REMOTE_ADDR"];
            if (Request.ServerVariables["X_FORWARDED_FOR"] != null)
            {
                if (Request.ServerVariables["X_FORWARDED_FOR"] != sIP && Request.ServerVariables["X_FORWARDED_FOR"].ToString().Trim().Length > 0) 
                    sIP = Request.ServerVariables["X_FORWARDED_FOR"];
            }
            return sIP;
        }

    }
}