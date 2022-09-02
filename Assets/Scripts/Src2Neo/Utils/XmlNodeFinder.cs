using OOS;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Src2Neo.SrcML
{
    /// <summary>
    /// This class finds <see cref="XmlNode"/>s inside the srcML file using XPath.
    /// </summary>
    public class XmlNodeFinder
    {
        /// <summary>
        /// Get all unit nodes one level deep in the current context.
        /// </summary>
        /// <param name="xmlRoot">The root XmlNode in which we search for the units.</param>
        public static XmlNodeList FindUnits (XmlNode xmlRoot)
        {
            return xmlRoot.SelectNodes("./def:unit", SrcLoader.XmlNsManager);
        }

        /// <summary>
        /// Find all xmlNodes of a certain type inside a XmlNode.
        /// The specific xPath instructions for each nodeType are loaded from in the SrcML_Dictionary class.
        /// </summary>
        /// <param name="nodeType">The srcML node type that we want to find.</param>
        /// <param name="xmlRoot">The root XmlNode in which we search for the nodes.</param>
        /// <param name="unitLanguage">The programming language of the xml root node.</param>
        /// <param name="onlyCurrentLevel">Whether all nodes inside the root node or only the nodes on 
        /// the first lower level are returned.</param>
        /// <returns>List of XmlNodes that match the nodeType.</returns>
        public static List<XmlNode> FindNodesOfType (SrcElement nodeType, XmlNode xmlRoot, Language unitLanguage, bool onlyCurrentLevel)
        {
            // Return empty list for undefined languages.
            if (unitLanguage is Language.Undefined)
                return new List<XmlNode>();
            
            // Construct xPath command.
            string xPath = onlyCurrentLevel ? "./def:" : ".//def:";
            xPath += Converter.Settings.NodeTags[nodeType][(int)unitLanguage];

            // Search and return the nodes.
            XmlNodeList nodeList = xmlRoot.SelectNodes(xPath, SrcLoader.XmlNsManager);
            return CleanXmlNodeList(nodeList);
        }

        //public static List<XmlNode> FindClassLevelObjectNodes (XmlNode root, Language unitLanguage, bool onlyCurrentLevel)
        //{
        //    string xPath = onlyCurrentLevel ? "./def:" : ".//def:";
        //    switch (unitLanguage)
        //    {
        //        default:
        //            xPath += "decl";
        //            break;
        //    }

        //    XmlNodeList classList = root.SelectNodes(xPath, SrcML_DataLoader.Instance.XmlNsManager); // All <enum> elements one or more levels deep in the current context.
        //    return CleanXmlNodeList(classList);

        //    // Objects declared on class level.
        //    //XmlNodeList objectsList = _project.Classes[i].XML["block"].SelectNodes("./def:decl_stmt/decl", xmlnsManager);

        //    // Objects declared on class level.
        //    //objectsList = _project.Classes[i].XML["block"].SelectNodes("./def:property", xmlnsManager); // TODO C# specific?

        //    // Objects declared on function level.
        //    //objectsList = _project.Classes[i].Functions[j].XML.SelectNodes(".//def:decl", xmlnsManager);
        //}

        //public static List<XmlNode> FindFunctionLevelObjectNodes(XmlNode root, Language unitLanguage, bool onlyCurrentLevel)
        //{
        //    string xPath = onlyCurrentLevel ? "./def:" : ".//def:";
        //    switch (unitLanguage)
        //    {
        //        default:
        //            xPath += "decl";
        //            break;
        //    }

        //    XmlNodeList classList = root.SelectNodes(xPath, SrcML_DataLoader.Instance.XmlNsManager); // All <enum> elements one or more levels deep in the current context.
        //    return CleanXmlNodeList(classList);

        //    // Objects declared on class level.
        //    //XmlNodeList objectsList = _project.Classes[i].XML["block"].SelectNodes("./def:decl_stmt/decl", xmlnsManager);

        //    // Objects declared on class level.
        //    //objectsList = _project.Classes[i].XML["block"].SelectNodes("./def:property", xmlnsManager); // TODO C# specific?

        //    // Objects declared on function level.
        //    //objectsList = _project.Classes[i].Functions[j].XML.SelectNodes(".//def:decl", xmlnsManager);
        //}

        /// <summary>
        /// Find the parent namespace XmlNode of a class XmlNode.
        /// </summary>
        /// <param name="xmlNode">The XmlNode of the class.</param>
        /// <param name="language">The programming language of the class.</param>
        /// <returns>The namespace or Null.</returns>
        public static XmlNode FindParentNamespaceOfClass(XmlNode xmlNode, Language language)
        {
            // Construct xPath command.
            string xPath = language switch
            {
                Language.Java => "./../def:package",
                Language.Csharp => "./../../../def:namespace",
                _ => "./../../../def:namespace", // TODO
            };

            // Search and return the node.
            return xmlNode.SelectSingleNode(xPath, SrcLoader.XmlNsManager);
        }

        /// <summary>
        /// Find all namespace XmlNodes that a class XmlNode imports.
        /// </summary>
        /// <param name="xmlNode">The XmlNode of the class.</param>
        /// <param name="language">The programming language of the class.</param>
        /// <returns>A list of found namespace XmlNodes (could be empty).</returns>
        public static List<XmlNode> FindImportedNamespacesOfClass(XmlNode xmlNode, Language language)
        {
            // Search for imported namespaces one layer above the class.
            string xPath = language switch
            {
                Language.Java => "./../def:import",
                Language.Csharp => "./../def:using",
                _ => "./../def:using", // TODO
            };            
            XmlNodeList importNamespaceNodes = xmlNode.SelectNodes(xPath, SrcLoader.XmlNsManager);

            // If the class is inside a namespace, we must also look outside the namespace for additional imports.
            if ((language == Language.Csharp || language == Language.Cplusplus) && FindParentNamespaceOfClass(xmlNode, language) != null)
            {
                xPath = "./../../../def:using";
            }
            XmlNodeList importNamespaceNodes2 = xmlNode.SelectNodes(xPath, SrcLoader.XmlNsManager);

            // Cobine XmlNodeLists.
            importNamespaceNodes.Cast<XmlNode>().Concat(importNamespaceNodes2.Cast<XmlNode>());

            return CleanXmlNodeList(importNamespaceNodes);
        }

        /// <summary>
        /// Find all "implements" nodes that a class XmlNode contains.
        /// </summary>
        /// <param name="xmlNode">The XmlNode of the class.</param>
        /// <param name="language">The programming language of the class.</param>
        /// <returns>A list of implemented interface XmlNodes (could be empty).</returns>
        public static List<XmlNode> FindImplementedInterfaceNodes(XmlNode xmlNode, Language language)
        {
            // Construct xPath command.
            string xPath = language switch
            {
                Language.Java => "./def:super_list/def:implements/def:super",
                Language.Csharp => "./def:super_list/def:super",
                _ => "./def:super_list/def:super", // TODO
            };

            // Search and return the nodes.
            XmlNodeList superList = xmlNode.SelectNodes(xPath, SrcLoader.XmlNsManager);
            return CleanXmlNodeList(superList);
        }

        /// <summary>
        /// Find all "extends" nodes that a class XmlNode contains.
        /// </summary>
        /// <param name="xmlNode">The XmlNode of the class.</param>
        /// <param name="language">The programming language of the class.</param>
        /// <returns>A list of extended (abstract) class XmlNodes (could be empty).</returns>
        public static List<XmlNode> FindExtendedClassNodes(XmlNode xmlNode, Language language)
        {
            // Construct xPath command.
            string xPath = language switch
            {
                Language.Java => "./def:super_list/def:extends/def:super",
                Language.Csharp => "./def:super_list/def:super",
                _ => "./def:super_list/def:super",
            };

            // Search and return the nodes.
            XmlNodeList baseClassList = xmlNode.SelectNodes(xPath, SrcLoader.XmlNsManager);
            return CleanXmlNodeList(baseClassList);
        }

        /// <summary>
        /// Finds all members of a enum XmlNode.
        /// </summary>
        /// <param name="xmlNode">The XmlNode of the enum.</param>
        /// <param name="language">The programming language of the enum.</param>
        /// <returns></returns>
        public static List<XmlNode> FindEnumMembers(XmlNode xmlNode, Language language)
        {
            XmlNodeList members = xmlNode.SelectNodes("./def:block/def:decl", SrcLoader.XmlNsManager); // All <decl> elements one level deep in the current context.
            return CleanXmlNodeList(members);
        }

        /// <summary>
        /// Finds all members of a enum XmlNode.
        /// </summary>
        /// <param name="xmlNode">The XmlNode of the enum.</param>
        /// <param name="language">The programming language of the enum.</param>
        /// <returns></returns>
        public static List<XmlNode> FindCalledMethods(XmlElement xmlNode, Language language)
        {
            XmlNodeList members = xmlNode.GetElementsByTagName("call");
            return CleanXmlNodeList(members);
        }


        public static List<string> FindSpecifier(XmlNode root)
        {
            XmlNodeList specifierList = root.SelectNodes("./def:specifier", SrcLoader.XmlNsManager);
            List<string> specifier = new List<string>();
            for (int i = 0; i < specifierList.Count; i++)
            {
                specifier.Add(specifierList[i].InnerText);
            }
            return specifier;
        }







        // #############
        // Helper Functions
        // #############

        public static List<XmlNode> CleanXmlNodeList(XmlNodeList xmlNodeList)
        {
            List<XmlNode> cleanList = new List<XmlNode>();

            if (xmlNodeList == null || xmlNodeList.Count == 0)
            {
                return cleanList;
            }

            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                if (string.IsNullOrEmpty(xmlNodeList[i].InnerText))
                    continue;

                cleanList.Add(xmlNodeList[i]);
            }
            return cleanList;
        }


    }
}
