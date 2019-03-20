using System;
using System.Collections.Generic;

namespace C1Contrib.LINQPad
{
    public class EntitySchema
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public Guid TypeId { get; set; }

        public List<EntityProperty> Properties { get; set; }
    }
}
