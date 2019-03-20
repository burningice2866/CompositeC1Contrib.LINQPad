using System;
using System.Web;

using Composite.Core.Linq;
using Composite.Data;
using Composite.Data.DynamicTypes;

using Newtonsoft.Json;

namespace C1Contrib.LINQPad.Server.Handlers
{
    public class FetchHandler : LinqPadHandler
    {
        protected override string GetJson(HttpContext context)
        {
            var typeId = context.Request.Form["typeId"];

            using (new DataScope(PublicationScope.Published))
            {
                var type = DynamicTypeManager.GetDataTypeDescriptor(Guid.Parse(typeId)).GetInterfaceType();
                var list = DataFacade.GetData(type).ToListOfObjects();

                var json = JsonConvert.SerializeObject(list);

                return json;
            }
        }
    }
}
