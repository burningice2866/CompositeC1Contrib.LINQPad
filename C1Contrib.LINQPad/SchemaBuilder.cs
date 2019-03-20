using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using LINQPad.Extensibility.DataContext;

using Microsoft.CSharp;

namespace C1Contrib.LINQPad
{
    public class SchemaBuilder
    {
        private const string TypeName = "C1DataContext";

        public C1DataRetriever DataRetriever { get; }

        public SchemaBuilder(ConnectionProperties props) : this(new Uri(props.Uri), props.UserName, props.Password) { }

        public SchemaBuilder(Uri uri, string userName, string password)
        {
            DataRetriever = new C1DataRetriever(uri, userName, password);
        }

        public List<ExplorerItem> GetSchemaAndBuildAssembly(string name, ref string ns, ref string typeName)
        {
            typeName = TypeName;

            var entities = DataRetriever.ListTypes();
            var code = GenerateCode(ns, entities);

            BuildAssembly(code, name);

            return GetSchema(entities);
        }

        public string GenerateCode(string ns, List<EntitySchema> entities)
        {
            var writer = new StringWriter();

            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine("using C1Contrib.LINQPad;");
            writer.WriteLine();
            writer.WriteLine("namespace " + ns);
            writer.WriteLine("{");

            foreach (var entity in entities)
            {
                WriteEntityClass(writer, entity);
            }

            writer.WriteLine();

            WriteContextClass(entities, writer);

            writer.WriteLine("}");

            return writer.ToString();
        }

        private static void WriteEntityClass(TextWriter writer, EntitySchema entity)
        {
            var className = $"{entity.Namespace.Replace(".", "_")}_{entity.Name}";

            writer.WriteLine($"public class {className}");
            writer.WriteLine("{");

            foreach (var prop in entity.Properties)
            {
                writer.WriteLine($"public {prop.Type} {prop.Name} {{ get; set; }}");
            }

            writer.WriteLine("}");
        }

        private static void WriteContextClass(IEnumerable<EntitySchema> entities, TextWriter writer)
        {
            writer.WriteLine($"public class {TypeName}");
            writer.WriteLine("{");
            writer.WriteLine();
            writer.WriteLine("  private readonly C1DataRetriever _dataRetriever;");
            writer.WriteLine();
            writer.WriteLine("  public C1DataContext(Uri uri, string userName, string password)");
            writer.WriteLine("  {");
            writer.WriteLine("    _dataRetriever = new C1DataRetriever(uri, userName, password);");
            writer.WriteLine("  }");
            writer.WriteLine();

            foreach (var entity in entities)
            {
                var className = $"{entity.Namespace.Replace(".", "_")}_{entity.Name}";

                writer.WriteLine($"public IList<{className}> {className}");
                writer.WriteLine("{");
                writer.WriteLine("  get");
                writer.WriteLine("  {");
                writer.WriteLine($"   return _dataRetriever.Fetch<{className}>(Guid.Parse(\"{entity.TypeId}\"));");
                writer.WriteLine("  }");
                writer.WriteLine("}");
                writer.WriteLine();
            }

            writer.WriteLine("}");
        }

        public void BuildAssembly(string code, string name)
        {
            CompilerResults results;

            var providerOptions = new Dictionary<string, string>
            {
                {
                    "CompilerVersion",
                    "v4.0"
                }
            };

            using (var codeProvider = new CSharpCodeProvider(providerOptions))
            {
                var location = Assembly.GetExecutingAssembly().Location;

                var assemblies = new List<string>()
                {
                    "System.dll",
                    "System.Core.dll",
                    Path.Combine(location.Substring(0, location.LastIndexOf('\\')), "C1Contrib.LINQPad.dll")
                };

                var options = new CompilerParameters(assemblies.ToArray(), name, true);

                results = codeProvider.CompileAssemblyFromSource(options, code);
            }

            if (results.Errors.Count > 0)
            {
                throw new Exception("Cannot compile typed context: " + results.Errors[0].ErrorText + " (line " + results.Errors[0].Line + ")");
            }
        }

        public List<ExplorerItem> GetSchema(List<EntitySchema> entities)
        {
            var items = new List<ExplorerItem>();

            foreach (var entity in entities)
            {
                var name = entity.Namespace.Replace(".", "_") + "_" + entity.Name;

                var itm = new ExplorerItem(name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                {
                    IsEnumerable = true,
                    ToolTipText = String.Empty,
                    Tag = entity.TypeId
                };

                items.Add(itm);
            }

            foreach (var item in items)
            {
                var entity = entities.Single(e => e.TypeId == (Guid)item.Tag);

                var children = new List<ExplorerItem>();
                foreach (var prop in entity.Properties)
                {
                    var explorerItem = new ExplorerItem($"{prop.Name} ({prop.Type})", ExplorerItemKind.Property, ExplorerIcon.Column);

                    children.Add(explorerItem);
                }

                item.Children = children.OrderBy(c => c.Text).ToList();
            }

            return items.OrderBy(i => i.Text).ToList();
        }
    }
}
