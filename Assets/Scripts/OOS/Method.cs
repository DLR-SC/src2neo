using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace OOS
{
    public class Method
    {
        // Proteries
        public string Name { get; set; }
        public Class ReturnClass { get; set; } // "void" if null
        public AccessModifier AccessModifier { get; set; }
        public int LOC { get; set; }
        public List<string> Specifier { get; set; }

        // Relationships
        public Class ParentClass { get; set; } // [1]
        public List<Method> MethodCalls { get; set; } // Functions that are called by this function. The first list item is the first function that is called.

        // Content
        public List<Variable> Variables { get; set; } // [0...*]


        // Neo4j
        public long Neo4jID { get; set; }

        // Code
        public string SourceCode { get; set; }

        // XML
        public XmlElement XML { get; set; }


        public Method()
        {
            MethodCalls = new List<Method>();
            Variables = new List<Variable>();
            Specifier = new List<string>();
        }

    }
}

