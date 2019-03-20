using System;
using System.Web;

using Composite.C1Console.Security;

namespace C1Contrib.LINQPad.Server.Handlers
{
    public abstract class LinqPadHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.HttpMethod != "POST")
            {
                return;
            }

            var userName = context.Request.Form["userName"];
            var password = context.Request.Form["password"];

            var result = FormValidateUser(userName, password);
            if (result != LoginResult.Success)
            {
                return;
            }

            var json = GetJson(context);

            context.Response.ContentType = "application/json";
            context.Response.Write(json);
        }

        protected abstract string GetJson(HttpContext context);

        private static LoginResult FormValidateUser(string userName, string password)
        {
            var loginProvider = Type.GetType("Composite.C1Console.Security.Foundation.PluginFacades.LoginProviderPluginFacade, Composite");
            var result = (LoginResult)loginProvider.GetMethod("FormValidateUser").Invoke(null, new[] { userName, password });

            return result;
        }
    }
}
