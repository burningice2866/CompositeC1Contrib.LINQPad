using System.Web;
using System.Web.Routing;

namespace C1Contrib.LINQPad.Server
{
    public class GenericRouteHandler<T> : IRouteHandler where T : IHttpHandler, new()
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var handler = new T();

            return handler;
        }
    } 
}
