using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace EC2.Chat.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CultureAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        private const String CookieLangEntry = "language";

        public String Name { get; set; }
        public static String CookieName
        {
            get { return "_Culture"; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var culture = Name;
            if (String.IsNullOrEmpty(culture))
                culture = GetSavedCultureOrDefault(filterContext.RequestContext.HttpContext.Request);

            // Set culture on current thread
            SetCultureOnThread(culture);

            // Proceed as usual
            base.OnActionExecuting(filterContext);
        }

        public static void SavePreferredCulture(HttpResponseBase response, String language,
                                                Int32 expireDays = 1)
        {
            var cookie = new HttpCookie(CookieName) { Expires = DateTime.Now.AddDays(expireDays) };
            cookie.Values[CookieLangEntry] = language;
            response.Cookies.Add(cookie);
        }

        public static String GetSavedCultureOrDefault(HttpRequestBase httpRequestBase)
        {
            var culture = "";
            var cookie = httpRequestBase.Cookies[CookieName];
            if (cookie != null)
                culture = cookie.Values[CookieLangEntry];
            else
            {
                var lang = httpRequestBase.UserLanguages.FirstOrDefault();
                culture = lang;                
            }
            return culture;
        }

        private static void SetCultureOnThread(String language)
        {
            System.Globalization.CultureInfo cultureInfo;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(language);
            }
            catch(Exception e)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
            }
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            
        }
    }
}