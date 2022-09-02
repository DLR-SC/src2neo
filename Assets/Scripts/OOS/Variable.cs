using System.Collections;
using System.Collections.Generic;

namespace OOS
{
    public class Variable
    {
        public Class Type { get; set; } // TODO Enum
        public string Name { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public long Neo4JID { get; set; }
        public List<string> Specifier { get; set; }

        public Variable()
        {
            Specifier = new List<string>();
            Neo4JID = -1;
        }
    }
}


