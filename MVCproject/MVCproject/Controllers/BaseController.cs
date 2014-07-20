using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCproject.Controllers
{
    public class BaseController : Controller
    {
        protected bool cache_enabled = false;
        protected CacheAttribute cache_attribute = null;
        protected string cache_ID = null;
        protected int cache_duration = 0;
        protected bool omit_database = false;

        protected User user = null;

        protected Database database = null;

        protected ICache cache = null;

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {

            string controller_name = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string action_name = filterContext.ActionDescriptor.ActionName;
            Type action_type = (((System.Web.Mvc.ReflectedActionDescriptor)(filterContext.ActionDescriptor)).MethodInfo).ReturnType;

            // if there is no session info for the user, redirect to the login page
            if (Session["user"] == null && controller_name != "Account")
            {

                // depending on the type of action called, it should perform a redirect or return a json response
                // don't want to break an ajax call due to an expired session returning a redirect to the login page

                if (action_type == typeof(ActionResult))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                }
                else
                {
                    var result = new ContentResult();
                    result.ContentType = "application/json";
                    result.Content = "\"Unauthorized\"";

                    filterContext.Result = result;
                    return;
                }
            }
            else
            {
                if (Session["user"] != null)
                {
                    user = (User)Session["user"];
                }
            }
            

        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // read configuration settings
            cache_enabled = ConfigurationManager.AppSettings["cache_enabled"].ToString() == "true";

            // check custom attributes - if any
            cache_attribute = (CacheAttribute)filterContext.ActionDescriptor.GetCustomAttributes(typeof(CacheAttribute), false).FirstOrDefault();
            omit_database = filterContext.ActionDescriptor.GetCustomAttributes(typeof(OmitDatabaseAttribute), false).FirstOrDefault() != null;

            // if the attribute cache is present and the cache is enabled in the parameter in the web.config file
            if (cache_enabled && cache_attribute != null)
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
                    DateTime duration = DateTime.MaxValue;
                    
                    MongoCacheObject cacheObject = new MongoCacheObject(cache_ID, ((System.Web.Mvc.ContentResult)(filterContext.Result)).Content, duration);

                    if (cache_attribute.seconds > 0)
                    {
                        duration = DateTime.Now.AddSeconds(cache_attribute.seconds);
                    }

                    // if there is any parameter that contains the dataset id
                    if (!string.IsNullOrEmpty(cache_attribute.datasetParameter))
                    {
                        cacheObject.DatasetId = int.Parse(Request[cache_attribute.datasetParameter].ToString());
                    }

                    cache.Add(cacheObject);
                }
            }

            base.OnActionExecuted(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext)
        {

            // check the type of action to ensure we return the right value

            if (filterContext.Result is JsonResult || filterContext.Result is ContentResult)
            {

                var result = new ContentResult();
                result.ContentType = "application/json";
                result.Content = "\"Exception\"";

                filterContext.Result = result;

            }
            else
            {

                var result = new ViewResult() { ViewName = "~/Views/Shared/Error.cshtml" };

                filterContext.Result = result;
            }

            filterContext.ExceptionHandled = true;

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