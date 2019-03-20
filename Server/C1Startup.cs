using System.Web.Routing;

using C1Contrib.LINQPad.Server.Handlers;

using Composite.Core.Application;

namespace C1Contrib.LINQPad.Server
{
    [ApplicationStartup]
    public static class C1Startup
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            var routes = RouteTable.Routes;

            routes.AddGenericHandler<TypesHandler>("api/linqpad/types");
            routes.AddGenericHandler<FetchHandler>("api/linqpad/fetch");
        }
    }
}
