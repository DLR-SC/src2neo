using System.Collections.Generic;
using System.Xml;

namespace OOS
{
    public class Class
    {
        // Properties
        public string Name { get; set; } // Fully qialified name.
        public string FileLocation { get; set; } // srcML file Name; e.g. IslandViz_/OsgiViz_Classes/Experimental/VoronoiTester.cs
        public AccessModifier AccessModifier { get; set; }
        public List<string> Specifier { get; set; }
        public int LOC { get; set; } // Lines of Code.
        public Language Language { get; set; }

        // Relationships
        public Namespace ParentNamespace { get; set; } // [0,1]        
        public List<Namespace> ImportedNamespaces { get; set; } // [0,*]
        public List<Class> BaseClasses { get; set; } // [0,*]
        public List<Interface> ImplementedInterfaces { get; set; } // [0,*]

        // Content
        public List<Method> Methods { get; set; } // [0,*]
        public List<Variable> Variables { get; set; } // [0,*]

        // Code
        public string SourceCode { get; set; }

        // XML
        public XmlElement XML { get; set; }

        // Neo4J
        public long Neo4JID { get; set; }


        public Class()
        {
            ImportedNamespaces = new List<Namespace>();
            Methods = new List<Method>();
            Variables = new List<Variable>();
            Specifier = new List<string>();
            BaseClasses = new List<Class>();
            ImplementedInterfaces = new List<Interface>();
            Neo4JID = -1;
        }



        /// <summary>
        /// Resolves a list of specifiers to an access modifier.
        /// </summary>
        /// <param name="specifier">List of specifiers.</param>
        /// <returns>Returns a access modifier if found. Else "private" will be returned.</returns>
        public static AccessModifier FindAccessModifierInSpecifier(List<string> specifier) // TODO use enums
        {
            if (specifier.Count == 0)
            {
                return AccessModifier.Private;
            }
            for (int i = 0; i < specifier.Count; i++)
            {
                switch (specifier[i])
                {
                    case "public":
                        return AccessModifier.Public;
                    case "private":
                        return AccessModifier.Private;
                    case "protected":
                        return AccessModifier.Protected;
                    case "internal":
                        return AccessModifier.Internal;
                    default:
                        break;
                }

            }
            return AccessModifier.Private;
        }



        /// <summary>
        /// Checks if this function or a function with same name and code exists in this class.
        /// </summary>
        /// <param name="method"></param>
        /// <returns>True, if this functions or a function with same name and code exists in this class.</returns>
        public bool ContainsMethod(Method method)
        {
            for (int i = 0; i < Methods.Count; i++)
            {
                if (Methods[i].Name == method.Name && Methods[i].SourceCode == method.SourceCode)
                    return true;
            }
            return false;
        }

    }
}
