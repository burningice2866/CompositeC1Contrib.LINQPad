using System.Linq;
using System.Web;

using Composite.Data;
using Composite.Data.DynamicTypes;

using Newtonsoft.Json;

namespace C1Contrib.LINQPad.Server.Handlers
{
    public class TypesHandler : LinqPadHandler
    {
        protected override string GetJson(HttpContext context)
        {
            var list = DataFacade.GetAllInterfaces()
                .Select(i =>
                {
                    try
                    {
                        return DynamicTypeManager.GetDataTypeDescriptor(i);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(d => d != null)
                .Select(d =>
                    new
                    {
                        d.Name,
                        d.Namespace,
                        TypeId = d.DataTypeId,
                        Properties = d.Fields.Select(f => new
                        {
                            f.Name,
                            Type = f.InstanceType.FullName
                        })
                    })
                .OrderBy(o => o.Name);

            var json = JsonConvert.SerializeObject(list);

            return json;
        }
    }
}
