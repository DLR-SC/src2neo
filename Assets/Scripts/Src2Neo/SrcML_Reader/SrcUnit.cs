using System.Collections.Generic;
using System.Xml;
using OOS;

namespace Src2Neo.SrcML
{
    /// <summary>
    /// This class represents a single file inside the software project (i.e., <unit></unit> in the srcML file) and references
    /// all XML Nodes that we later need to extract the software structure. 
    /// </summary>
    public class SrcUnit
    {
        public Language Language { get; private set; } // Programming language of this srcML unit.
        public string SrcMlFileName { get; private set; } // File name of this srcML unit.
        public List<XmlNode> NamespaceNodes { get; private set; } // All namespaces defined in this unit, i.e., <namespace> or <package>.
        public List<XmlNode> ImportedNamespaceNodes { get; private set; } // All namespaces that are imported on the hightest level of this unit, i.e., <using> or <imports>.
        public List<XmlNode> ClassNodes { get; private set; } // All classes that exist in this unit, i.e., <class>.
        public List<XmlNode> InterfaceNodes { get; private set; } // All interfaces that exist in this unit, i.e., <interface>.
        public List<XmlNode> EnumNodes { get; private set; } // All enums that exist in this unit, i.e., <enum>.
        public List<XmlNode> MethodNodes { get; private set; } // All functions that exist in this unit, i.e., <function>.
        public List<XmlNode> ConstructorNodes { get; private set; } // All functions that exist in this unit, i.e., <function>.
        public List<XmlNode> FieldNodes { get; private set; } // All fields that exist in this unit.


        private readonly XmlNode rootNode; // XML node of this srcML unit.


        public SrcUnit (XmlNode node)
        {
            rootNode = node;
            SetUnitLanguage();
            SetFileName();
            SetNodes();
        }

        /// <summary>
        /// Detects and sets the unit language.
        /// </summary>
        private void SetUnitLanguage ()
        {
            // <unit> does not contain a language.
            if (rootNode.Attributes.GetNamedItem("language") is null)
            {
                Language = Language.Undefined;
                return;
            }

            Language = rootNode.Attributes.GetNamedItem("language").Value switch
            {
                "Java" => Language.Java,
                "C#" => Language.Csharp,
                "C++" => Language.Cplusplus,
                _ => Language.Undefined
            };
        }

        /// <summary>
        /// Detects and sets the unit file name.
        /// </summary>
        private void SetFileName ()
        {
            // <unit> does not contain a filename.
            if (rootNode.Attributes.GetNamedItem("filename") is null)
            {
                SrcMlFileName = "404";
                return;
            }
                        
            SrcMlFileName = rootNode.Attributes.GetNamedItem("filename").Value;            
        }

        /// <summary>
        /// Finds all unit content inside the xml-unit and stores the xml nodes.
        /// </summary>
        private void SetNodes ()
        {
            NamespaceNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Namespace, rootNode, Language, false);
            ImportedNamespaceNodes = XmlNodeFinder.FindNodesOfType(SrcElement.ImportedNamespace, rootNode, Language, true);
            ClassNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Class, rootNode, Language, false);
            InterfaceNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Interface, rootNode, Language, false);
            EnumNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Enum, rootNode, Language, false);
            MethodNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Method, rootNode, Language, false);
            ConstructorNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Constructor, rootNode, Language, false);
            FieldNodes = XmlNodeFinder.FindNodesOfType(SrcElement.Field, rootNode, Language, false);
        }

    }
}
