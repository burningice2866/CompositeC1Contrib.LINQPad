using System;
using System.Collections.Generic;
using System.Reflection;

using LINQPad.Extensibility.DataContext;

namespace C1Contrib.LINQPad
{
    public class C1CMSDynamicDriver : DynamicDataContextDriver
    {
        public override string Name { get; } = "C1 CMS";
        public override string Author { get; } = "Pauli Østerø (burningice)";

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return new ConnectionProperties(cxInfo).Uri;
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
        {
            var result = new ConnectionDialog(cxInfo).ShowDialog();

            return result == true;
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            return new[]
            {
                "Newtonsoft.Json.dll",
                "C1Contrib.LINQPad.dll"
            };
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return new[]
            {
                "Newtonsoft.Json",
                "C1Contrib.LINQPad"
            };
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            var connectionProperties = new ConnectionProperties(cxInfo);
            var schemaBuilder = new SchemaBuilder(connectionProperties);

            return schemaBuilder.GetSchemaAndBuildAssembly(assemblyToBuild.CodeBase, ref nameSpace, ref typeName);
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            // We need to pass arguments into the Context's constructor:
            return new[]
            {
                new ParameterDescriptor("uri", "System.Uri"),
                new ParameterDescriptor("userName", "System.String"),
                new ParameterDescriptor("password", "System.String")
            };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            var props = new ConnectionProperties(cxInfo);

            // We need to pass arguments into the Context's constructor:
            return new object[]
            {
                new Uri(props.Uri),
                props.UserName,
                props.Password
            };
        }

        public override bool AreRepositoriesEquivalent(IConnectionInfo r1, IConnectionInfo r2)
        {
            // Two repositories point to the same endpoint if their URIs are the same.
            return Equals(r1.DriverData.Element("Uri"), r2.DriverData.Element("Uri"));
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            // This method gets called after a DataServiceContext has been instantiated. It gives us a chance to
            // perform further initialization work.
        }
    }
}
