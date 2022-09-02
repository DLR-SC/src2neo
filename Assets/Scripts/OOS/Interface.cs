using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace OOS
{
    public class Interface
    {
        // Proteries
        public string Name { get; set; }
        public string FileLocation { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public List<string> Specifier { get; set; }
        public int LOC { get; set; }

        // Relationships
        public Namespace ParentNamespace { get; set; } // [0,1]
        public List<Namespace> ImportedNamespaces { get; set; } // [0,*]

        // Content
        public List<Method> Method { get; set; } // [0,*]
        public List<Variable> Variables { get; set; } // [0,*]
        public List<Interface> Interfaces { get; set; } // [0,*]

        // Code
        public string SourceCode { get; set; }

        // XML
        public XmlElement XML { get; set; }

        // Neo4J
        public long Neo4JID { get; set; }


        public Interface()
        {
            Method = new List<Method>();
            Variables = new List<Variable>();
            Specifier = new List<string>();
            ImportedNamespaces = new List<Namespace>();
            Neo4JID = -1;
        }



        /// <summary>
        /// Checks if this function or a function with same name and code exists in this class.
        /// </summary>
        /// <param name="function"></param>
        /// <returns>True, if this functions or a function with same name and code exists in this class.</returns>
        public bool ContainsMethod(Method function)
        {
            for (int i = 0; i < Method.Count; i++)
            {
                if (Method[i].Name == function.Name && Method[i].SourceCode == function.SourceCode)
                    return true;
            }
            return false;
        }

    }
}
