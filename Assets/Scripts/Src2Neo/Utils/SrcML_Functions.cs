using System.Xml;
using System.Collections.Generic;
using Src2Neo.SrcML;
using OOS;
using UnityEngine;

namespace Src2Neo.OOSConv
{
    /// <summary>
    /// This class contains helper functions to find srcML and OOS elements.
    /// </summary>
    public class SrcML_Functions
    {
        #region Find method by name

        /// <summary>
        /// This finds the function of a given scrML function call inside a source function.
        /// </summary>
        /// <param name="methodNameXML">The XML content inside the <call><name></name></call> tags.</param>
        /// <param name="srcMethod">The function in which the looked for function was called.</param>
        /// <returns></returns>
        public static Method FindMethodByName(XmlElement methodNameXML, Method srcMethod, Project project) // TODO Constructors?
        {
            if (methodNameXML == null)
                return null;

            if (methodNameXML["name"] == null) // e.g. |functionName = Instantiate| in case of <call><name>Instantiate</name>...</call>
            {
                return FindMethodBySingleName(methodNameXML, srcMethod, project);
            }
            else  // e.g. <call><name><name>compass</name><operator>.</operator><name>transform</name><operator>.</operator><name>Find</name></name>...</call>
            {
                return FindFunctionByMultipleNames(methodNameXML, srcMethod, project);
            }
        }


        private static Method FindMethodBySingleName (XmlElement methodNameXML, Method srcMethod, Project project)
        {
            // Method is local method.
            if (IsLocalMethod(methodNameXML.InnerText, srcMethod.ParentClass, project, false, out Method m))
            {
                return m;
            }
            // Function is imported function.
            if (IsImportedFunction(methodNameXML.InnerText, srcMethod.ParentClass, project, out Method mm))
            {
                return mm;
            }
            // Function was not found.
            Debug.LogWarning("FindFunctionByName (Single): Could not find the function by the name " + methodNameXML.InnerText + " -> Creating new Function.");
            return new Method
            {
                Name = methodNameXML.InnerText,
                SourceCode = "UNKOWN"
            };
        }

        // e.g. <call><name><name>compass</name><operator>.</operator><name>transform</name><operator>.</operator><name>Find</name></name>...</call>
        private static Method FindFunctionByMultipleNames(XmlElement functionNameXML, Method srcFunction, Project project)
        {
            Class _class;
            Method _function;
            Variable _object;

            Class currentClass = srcFunction.ParentClass;
            XmlNodeList functionChildNodes = functionNameXML.ChildNodes;
            int i = 0;

            if (FindNamespaceInName(functionNameXML, project, out Namespace _namespace))
            {
                Debug.LogWarning("FindFunctionByName (Multiple): Found Namespace " + _namespace.Name + " in function name " + functionNameXML.InnerText + " -> Skipping processing the function " + functionNameXML.InnerText + " in Function " + srcFunction.Name + "!");
                return FindMethodBySingleName(functionNameXML, srcFunction, project);
            }
            else if (functionChildNodes[0].InnerText == "this")
            {
                i = 1;
            }
            else if (functionChildNodes[0].InnerText == "base")
            {
                // TODO
                return FindMethodBySingleName(functionNameXML, srcFunction, project);
            }
            // First name.
            // Check if first is LOCAL OBJECT of current function.
            // E.g. "dockList.Add();"
            else if (IsLocalObject(functionChildNodes[0].InnerText, srcFunction, project, false, out _object))
            {
                currentClass = _object.Type;
                i = 1;
            }

            for (i = i; i < functionChildNodes.Count - 1; i++) // The last one is the function.
            {
                if (currentClass == null)
                {
                    break;
                }
                if (functionChildNodes[i].Name != "name") // Skip <operator>
                {
                    continue;
                }
                // Check if first is LOCAL OBJECT of current class.
                // E.g. "dockList.Add();"
                if (IsLocalObject(functionChildNodes[i].InnerText, currentClass, project, i != 0, out _object))
                {
                    currentClass = _object.Type;
                    continue;
                }
                // Check if first is LOCAL FUNCTION of current class.
                // E.g. "getDockList().Add();"
                if (IsLocalMethod(functionChildNodes[i].InnerText, currentClass, project, i != 0, out _function))
                {
                    currentClass = _function.ReturnClass;
                    continue;
                }
                // Check if current is a PUBLIC IMPORTED CLASS. 
                // E.g. "IslandVizUI.Instance.UpdateLoadingScreenUI();"
                if (IsImportedClass(functionChildNodes[i].InnerText, currentClass, project, out _class))
                {
                    currentClass = _class;
                    continue;
                }
                // Function was not found.
                currentClass = null;
                break;
            }

            // Last name.
            // (=function). 
            if (currentClass != null && currentClass.ParentNamespace.Name != "404")
            {
                if (IsLocalMethod(functionChildNodes[functionChildNodes.Count - 1].InnerText, currentClass, project, true, out _function))
                {
                    return _function;
                }
            }

            // Function was not found.
            Debug.LogWarning("FindFunctionByName (Multiple): Could not find the function by the name " + functionNameXML.InnerText);
            return new Method
            {
                Name = functionNameXML.InnerText,
                SourceCode = "UNKOWN"
            };
        }

        #endregion

        #region Find class by name

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classNameXML">E.g., \<name>Vector2</name>"</param>
        /// <param name="srcClass"></param>
        /// <returns></returns>
        public static Class FindClassByName(XmlElement classNameXML, Class srcClass, Project project, bool createNewIfNotFound)
        {
            if (classNameXML == null || classNameXML.InnerText == "void")
            {
                return null;
            }

            if (classNameXML["name"] == null) 
            {
                return FindClassBySingleName(classNameXML, srcClass, project, createNewIfNotFound);
            }
            else 
            {
                return FindClassByMultipleNames(classNameXML, srcClass, project, createNewIfNotFound);
            }
        }

        /// <summary>
        /// 
        /// e.g. <name>IslandVizBehaviour</name>
        /// </summary>
        private static Class FindClassBySingleName(XmlElement classNameXML, Class srcClass, Project project, bool createNewIfNotFound)
        {
            Class _class;

            if (classNameXML.InnerText == srcClass.Name)
            {
                return srcClass;
            }
            if (IsImportedClass(classNameXML.InnerText, srcClass, project, out _class))
            {
                return _class;
            }
            else if (!createNewIfNotFound)
            {
                return null;
            }

            _class = new Class
            {
                Name = classNameXML.InnerText,
                ParentNamespace = project.GetNamespaceByName("404"),
                SourceCode = "UNKOWN",
                Language = Language.Undefined,
                AccessModifier = AccessModifier.Public // TODO?
            };

            project.Classes.Add(_class);
            _class.ParentNamespace.Classes.Add(_class);

            Debug.LogWarning("FindClassByName (Single): Could not find the class " + classNameXML.InnerText + " in Class " + srcClass.Name + "! -> Created new Class <i>" + _class.Name + "</i> in Namespace " + _class.ParentNamespace.Name);

            return _class;
        }

        /// <summary>
        /// 
        /// Usecases: 1) Namespace.Class, 2) Class.Subclass, 3) List<Triangle>, 4) Dictionary<ServiceSlice, List<Service>>, string[]
        /// e.g., <name><name>IslandVizManager</name><operator>.</operator><name>Instance</name></name> 
        /// </summary>
        private static Class FindClassByMultipleNames(XmlElement classNameXML, Class srcClass, Project project, bool createNewIfNotFound)
        {
            if (classNameXML.InnerText.Contains(".") && FindNamespaceInName(classNameXML, project, out Namespace _namespace))
            {
                Debug.LogWarning("FindClassByName (Multiple): Found Namespace " + _namespace.Name + " in class name " + classNameXML.InnerText + " -> Skipping processing the class " + classNameXML.InnerText + " in Class " + srcClass.Name + "!");
                return FindClassBySingleName(classNameXML, srcClass, project, createNewIfNotFound);
            }
            else // TODO check classChildNodes[i].InnerText == currentClass.Name; TODO handle List content?
            {
                return FindClassBySingleName((XmlElement)classNameXML.ChildNodes[0], srcClass, project, createNewIfNotFound);
            }
        }

        #endregion

        #region Find interface by name

        public static Interface FindInterfaceByName(XmlElement interfaceNameXML, Class srcClass)
        {
            if (interfaceNameXML["name"] == null) // e.g. <name>IslandVizBehaviour</name>
            {
                List<Interface> importedInterfaces = GetAllImportedInterfaces(srcClass);

                for (int i = 0; i < importedInterfaces.Count; i++)
                {
                    if (importedInterfaces[i].Name == interfaceNameXML.InnerText)
                    {
                        return importedInterfaces[i];
                    }
                }
                // Class was not found.
                return null;
            }
            else // e.g., <name><name>OsgiViz</name><operator>.</operator><name>Unity</name></name> // TODO
            {
                // TODO
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Checks if a certain class name matches the name of an imported class.
        /// </summary>
        public static bool IsImportedClass (string className, Class srcCLass, Project project, out Class _class)
        {
            List<Class> importClasses = GetAllImportedClasses(srcCLass, project);
            _class = null;
            for (int i = 0; i < importClasses.Count; i++)
            {
                if (importClasses[i].AccessModifier != AccessModifier.Private && importClasses[i].Name == className)
                {
                    _class = importClasses[i];
                    return true;
                }
            }
            return false;
        }

        public static bool IsLocalMethod(string functionName, Class srcCLass, Project project, bool onlyPublic, out Method _function)
        {
            _function = null;

            for (int i = 0; i < srcCLass.Methods.Count; i++)
            {
                if (srcCLass.Methods[i].Name == functionName && (!onlyPublic || srcCLass.Methods[i].AccessModifier != AccessModifier.Private))
                {
                    _function = srcCLass.Methods[i];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a certain class name matches the name of an imported class.
        /// </summary>
        public static bool IsImportedFunction(string functionName, Class srcCLass, Project project, out Method _function)
        {
            _function = null;
            List<Class> importedClasses = GetAllImportedClasses(srcCLass, project);
            for (int i = 0; i < importedClasses.Count; i++)
            {
                for (int j = 0; j < importedClasses[i].Methods.Count; j++)
                {
                    if (importedClasses[i].Methods[j].AccessModifier != AccessModifier.Private && importedClasses[i].Methods[j].Name == functionName)
                    {
                        _function = importedClasses[i].Methods[j];
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsNamespace (string namespaceName, Project project, out Namespace _namespace)
        {
            _namespace = null;
            for (int i = 0; i < project.Namespaces.Count; i++)
            {
                if (project.Namespaces[i].Name == namespaceName)
                {
                    _namespace = project.Namespaces[i];
                    return true;
                }
            }
            return false;
        }

        public static bool FindNamespaceInName (XmlElement xmlName, Project project, out Namespace _namespace)
        {
            string[] names = xmlName.InnerText.Split('.');
            _namespace = null;

            for (int i = names.Length - 1; i > 0; i--)
            {
                string name = "";
                for (int j = 0; j < i; j++)
                {
                    name += names[j];
                    if (j < i - 1)
                    {
                        name += ".";
                    }
                }
                if (IsNamespace(name, project, out _namespace))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLocalObject(string objectName, Class srcCLass, Project project, bool onlyPublic, out Variable _object)
        {
            _object = null;
            for (int i = 0; i < srcCLass.Variables.Count; i++)
            {
                if (srcCLass.Variables[i].Name == objectName && (!onlyPublic || srcCLass.Variables[i].AccessModifier != AccessModifier.Private))
                {
                    _object = srcCLass.Variables[i];
                    return true;
                }
            }
            return false;
        }

        public static bool IsLocalObject(string objectName, Method srcFunction, Project project, bool onlyPublic, out Variable _object)
        {
            _object = null;
            for (int i = 0; i < srcFunction.Variables.Count; i++)
            {
                if (srcFunction.Variables[i].Name == objectName && (!onlyPublic || srcFunction.Variables[i].AccessModifier != AccessModifier.Private))
                {
                    _object = srcFunction.Variables[i];
                    return true;
                }
            }
            return false;
        }

        public static bool IsImportedObject (string objectName, Class srcCLass, Project project, out Variable _object)
        {
            _object = null;
            List<Class> importedClasses = GetAllImportedClasses(srcCLass, project);
            for (int i = 0; i < importedClasses.Count; i++)
            {
                for (int j = 0; j < importedClasses[i].Variables.Count; j++)
                {
                    if (importedClasses[i].Variables[j].AccessModifier != AccessModifier.Private && importedClasses[i].Variables[j].Name == objectName)
                    {
                        _object = importedClasses[i].Variables[j];
                        return true;
                    }
                }
            }
            return false;
        }


        public static Namespace GetParentNamespaceOfClass(XmlNode classXmlNode, Language language, Project project)
        {
            XmlNode namespaceXmlNode = XmlNodeFinder.FindParentNamespaceOfClass(classXmlNode, language);
            Namespace _namespace;

            if (namespaceXmlNode is null || namespaceXmlNode["name"] is null)
                _namespace = null;
            else
                _namespace = project.GetNamespaceByName(namespaceXmlNode["name"].InnerText);

            // Add the class to a default namespace that contains all classes without parent namespace.
            _namespace ??= project.GetNamespaceByName("");
            
            return _namespace;
        }

        public static List<Namespace> GetImportedNamespacesOfClass(XmlNode classXmlNode, Language language, Project project)
        {
            List<XmlNode> importedNamespaceNodes = XmlNodeFinder.FindImportedNamespacesOfClass(classXmlNode, language);
            var importedNamespaces = new List<Namespace>();
            foreach (XmlElement importNamespace in importedNamespaceNodes)
            {
                Namespace n = project.GetNamespaceByName(importNamespace["name"].InnerText);
                if (n != null)
                    importedNamespaces.Add(n);
            }
            return importedNamespaces;
        }

        public static List<Variable> GetMembersOfEnum(XmlNode enumNode, Language language)
        {
            // Enum members
            List<XmlNode> memberList = XmlNodeFinder.FindEnumMembers(enumNode, language);
            List<Variable> members = new List<Variable>();
            foreach (XmlNode xmlMember in memberList)
            {
                Variable _member = new Variable
                {
                    Name = xmlMember["name"].InnerText,
                    AccessModifier = AccessModifier.Public
                    //Type = _project.Namespaces. // TODO int
                };
                members.Add(_member);
            }
            return members;
        }

        public static Class GetParentClassFromMethod(XmlNode xmlNode, Language language, Project project)
        {
            XmlNode classNode = xmlNode.ParentNode.ParentNode;

            if (classNode == null || classNode["name"] == null)
            {
                Debug.LogWarning("Could not find parent class of function " + xmlNode["name"].InnerText);
                return null;
            }
            if (language == Language.Java && classNode.ParentNode.Name == "expr")
            {
                Debug.LogWarning("skipping");
                return null;
            }
            Namespace _namespace = GetParentNamespaceOfClass(classNode, language, project);
            if (_namespace == null)
            {
                _namespace = project.GetNamespaceByName("");
            }
            Class _class = _namespace.Classes.Find(x => x.Name == classNode["name"].InnerText);
            return _class;
        }



        


        #region OOP Helper Functions

        // ##########
        // OOP Helper Functions
        // ##########

        public static List<Class> GetAllImportedClasses(Class _class, Project project, bool includeDefaults = true)
        {
            List<Class> classes = new List<Class>();
            for (int i = 0; i < _class.ImportedNamespaces.Count; i++)
            {
                classes.AddRange(_class.ImportedNamespaces[i].Classes);
            }
            classes.AddRange(_class.ParentNamespace.Classes);
            for (int i = 0; i < _class.BaseClasses.Count; i++)
            {
                classes.AddRange(GetAllImportedClasses(_class.BaseClasses[i], project, false));
            }
            if (includeDefaults)
            {
                classes.AddRange(project.GetNamespaceByName("").Classes); // Add all classes without a namespace.
                classes.AddRange(project.GetNamespaceByName("404").Classes); // Add all classes without a namespace.
                                                                             //classes.AddRange(_project.GetNamespaceByName("Default").Classes); // Add all default class names. // TODO
            }
            return classes;
        }

        public static List<Interface> GetAllImportedInterfaces(Class _class)
        {
            List<Interface> interfaces = new List<Interface>();
            for (int i = 0; i < _class.ImportedNamespaces.Count; i++)
            {
                interfaces.AddRange(_class.ImportedNamespaces[i].Interfaces);
            }
            interfaces.AddRange(_class.ParentNamespace.Interfaces);
            for (int i = 0; i < _class.BaseClasses.Count; i++)
            {
                interfaces.AddRange(GetAllImportedInterfaces(_class.BaseClasses[i]));
            }
            //interfaces.AddRange(_project.GetNamespaceByName("404").Classes); // Add all classes without a namespace.
            //classes.AddRange(_project.GetNamespaceByName("Default").Classes); // Add all default class names. // TODO
            return interfaces;
        }

        #endregion
    }
}