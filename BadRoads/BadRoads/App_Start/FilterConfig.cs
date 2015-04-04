using BadRoads.Filters;
using System.Web;
using System.Web.Mvc;

namespace BadRoads
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new InitializeSimpleMembershipAttribute());
        }
    }
}