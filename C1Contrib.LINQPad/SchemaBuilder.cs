using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using LINQPad.Extensibility.DataContext;

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

        public List<ExplorerItem> GetSchemaAndBuildAssembly(string outputFile, ref string ns, ref string typeName)
        {
            typeName = TypeName;

            var entities = DataRetriever.ListTypes();
            var cSharpSourceCode = GenerateCode(ns, entities);

            BuildAssembly(cSharpSourceCode, outputFile);

            return GetSchema(entities);
        }

        public string GenerateCode(string ns, List<EntitySchema> entities)
        {
            using (var writer = new StringWriter())
            {
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

        public void BuildAssembly(string cSharpSourceCode, string outputFile)
        {
            IList<string> assembliesToReference =

#if NETCORE
            // GetCoreFxReferenceAssemblies is helper method that returns the full set of .NET Core reference assemblies.
            // (There are more than 100 of them.)
            DataContextDriver.GetCoreFxReferenceAssemblies().ToList();
#else
            // .NET Framework - here's how to get the basic Framework assemblies:
            new List<string>()
            {
                typeof (int).Assembly.Location,            // mscorlib
                typeof (Uri).Assembly.Location,            // System
                typeof (Enumerable).Assembly.Location,     // System.Core
            };                
#endif

            assembliesToReference.Add(Assembly.GetExecutingAssembly().Location);

            var compileResult = DataContextDriver.CompileSource(new CompilationInput
            {
                FilePathsToReference = assembliesToReference.ToArray(),
                OutputPath = outputFile,
                SourceCode = new[] { cSharpSourceCode }
            });

            if (compileResult.Errors.Any())
            {
                throw new Exception("Cannot compile typed context: " + compileResult.Errors[0]);
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
