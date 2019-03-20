using System;
using System.IO;
using C1Contrib.LINQPad;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var uri = "***";
            var userName = "***";
            var password = "***";

            var schemaBuilder = new SchemaBuilder(new Uri(uri), userName, password);

            var entities = schemaBuilder.DataRetriever.ListTypes();
            var code = schemaBuilder.GenerateCode("test", entities);
            schemaBuilder.BuildAssembly(code, Path.GetTempFileName());

            Console.Write(code);

            Console.ReadKey();
        }
    }
}
