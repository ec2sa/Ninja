using EC2.Chat.Helpers;
using System.Web;
using System.Web.Mvc;

namespace Chat
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CultureAttribute());
        }
    }
}