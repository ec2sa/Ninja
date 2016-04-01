using EC2.Chat.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EC2.Chat.Controllers
{
    
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        [CultureAttribute]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Language(string culture_)
        {
            CultureAttribute.SavePreferredCulture(Response, culture_, 1);

            System.Globalization.CultureInfo cultureInfo = GetCulture(culture_);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            
            return RedirectToAction("Index");
        }

        [CultureAttribute]
        public JsonResult LocalizeJSON()
        {
            CultureInfo culture_ = GetCulture(Request);
            Dictionary<object, object> dict = ReadResources("LocalizedText", culture_);
            
            return Json(dict, JsonRequestBehavior.AllowGet);
        }

        private static Dictionary<object, object> ReadResources(string classKey,
                                                           CultureInfo requestedCulture)
        {
            var resourceManager = new ResourceManager("Resources." + classKey,
                                                Assembly.Load("App_GlobalResources"));
            using (var resourceSet =
                resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true))
            {

                return resourceSet
                    .Cast<DictionaryEntry>()
                    .ToDictionary(x => x.Key,
                         x => resourceManager.GetObject((string)x.Key, requestedCulture));
            }

        }

        private CultureInfo GetCulture(HttpRequestBase request)
        {
            var culture = "";
            var cookie = request.Cookies["_Culture"];
            if (cookie != null)
                culture = cookie.Values["language"];
            else
            {
                var lang = request.UserLanguages.FirstOrDefault();
                culture = lang;
            }
            
            CultureInfo cultureInfo;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(culture);
            }
            catch (Exception e)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
            }
            return cultureInfo;
        }

        private CultureInfo GetCulture(string culture_)
        {
            CultureInfo cultureInfo;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(culture_);
            }
            catch (Exception e)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
            }
            return cultureInfo;
        }

        public HomeController()
        {
            //var cultureString = GetCultureToApply();
            //var cultureInfo = CultureInfo.CreateSpecificCulture(cultureString);
            //Thread.CurrentThread.CurrentCulture = cultureInfo;
            //Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

    }
}
