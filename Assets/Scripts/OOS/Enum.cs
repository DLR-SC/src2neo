using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace OOS
{
    public class Enum
    {
        public Namespace Namespace { get; set; } // [0,1]        
        public List<Variable> Members { get; set; } // [0,*]

        // Proteries
        public string Name { get; set; }
        public string Filename { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public int LOC { get; set; }

        // Code
        public string Code { get; set; }
        public XmlElement XML { get; set; }

        // Neo4J
        public long Neo4JID { get; set; }


        public Enum()
        {
            Members = new List<Variable>();
            Neo4JID = -1;
        }

    }
}
