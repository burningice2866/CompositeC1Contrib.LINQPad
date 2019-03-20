using System.Web;
using System.Web.Routing;

namespace C1Contrib.LINQPad.Server
{
    public static class RouteCollectionExtensions
    {
        public static void AddGenericHandler<T>(this RouteCollection routes, string route) where T : IHttpHandler, new()
        {
            routes.Add(new Route(route, new GenericRouteHandler<T>()));
        }
    }
}
