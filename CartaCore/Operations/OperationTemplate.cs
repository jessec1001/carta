using System.Collections.Generic;

namespace CartaCore.Operations
{
    public class OperationTemplate
    {
        public string Type { get; set; }
        public string Subtype { get; set; }

        public Dictionary<string, object> Default { get; set; }
    }
}