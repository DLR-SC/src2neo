using System;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;
using OOS;
using Src2Neo.SrcML;

namespace Src2Neo.OOSConv
{
    /// <summary>
    /// This class converts <see cref="SrcUnit"/>s to a OOS <see cref="Project"/>.
    /// </summary>
    public static class OOSConverter
    {
        public static async Task<Project> ConvertAsync(SrcUnit[] units)
        {
            var project = new Project();

            // Convert SrcML units to OOS elements.
            ConvertUnitsToOOS(units, project);

            // Additional OOS dependencies.
            AddDependencies_ExtendsAndImplements(project);
            AddDependencies_Calls(project);

            // Misc - Sort lists alphabetical for better readability.
            SortProject(project);

            // Debug log & UI.
            await Src2NeoEvents.OnProgressAsync?.Invoke(0.5f);
            //Console.Write(project.GetProjectInformation());
            //Console.WriteLine(project.GetProjectInformation());

            return project;
        }


        #region OOP Element Conversion

        /// <summary>
        /// Convert a list of srcML units to OOP elements and add them to the project.
        /// </summary>
        /// <param name="srcMlUnits">The list of srcML units that will be converted.</param>
        /// <param name="project">The OOS project where the elements will be added.</param>
        public static void ConvertUnitsToOOS(SrcUnit[] srcMlUnits, Project project)
        {
            // Create namespaces and add them to the project.
            foreach (SrcUnit unit in srcMlUnits)
            {
                foreach (XmlNode namespaceNode in unit.ImportedNamespaceNodes)
                {
                    AddNamespace(namespaceNode, project);
                }
                foreach (XmlNode namespaceNode in unit.NamespaceNodes)
                {
                    AddNamespace(namespaceNode, project);
                }
            }

            // Create classes & interface & enums and add them to the project & namespaces.
            foreach (SrcUnit unit in srcMlUnits)
            {
                foreach (XmlNode classNode in unit.ClassNodes)
                {
                    AddClass(classNode, unit, project);
                }
                foreach (XmlNode interfaceNode in unit.InterfaceNodes)
                {
                    AddInterface(interfaceNode, unit, project);
                }
                foreach (XmlNode enumNode in unit.EnumNodes)
                {
                    AddEnum(enumNode, unit, project);
                }
            }

            // Third, all methods & constructors are added to the OOS project and OOS classes.
            foreach (SrcUnit unit in srcMlUnits)
            {
                foreach (XmlNode methodNode in unit.MethodNodes)
                {
                    AddMethod(methodNode, unit.Language, project);
                }
                foreach (XmlNode constructorNode in unit.ConstructorNodes)
                {
                    // TODO
                }
            }

            // TODO 
            foreach (SrcUnit unit in srcMlUnits)
            {
                foreach (XmlNode objectNode in unit.FieldNodes)
                {
                    //AddObject(objectNode, unit.Language); // TODO
                }
            }
            //for (int i = 0; i < Project.Classes.Count; i++)
            //{
            //    if (Project.Classes[i].Namespace.Name == "404")
            //    {
            //        continue;
            //    }

            //    // Objects declared on class level.
            //    XmlNodeList objectsList = Project.Classes[i].XML["block"].SelectNodes("./def:decl_stmt/decl", XmlNsManager);
            //    if (objectsList.Count > 0)
            //    {
            //        Project.Classes[i].Variables.AddRange(AddObjects(objectsList, Project.Classes[i]));
            //    }

            //    if (true) //(fileLanguage == Language.Csharp)
            //    {
            //        // Objects declared on class level.
            //        objectsList = Project.Classes[i].XML["block"].SelectNodes("./def:property", XmlNsManager); // TODO C# specific?
            //        if (objectsList.Count > 0)
            //        {
            //            Project.Classes[i].Variables.AddRange(AddObjects(objectsList, Project.Classes[i]));
            //        }
            //    }

            //    // Objects declared on function level.
            //    for (int j = 0; j < Project.Classes[i].Methods.Count; j++)
            //    {
            //        objectsList = Project.Classes[i].Methods[j].XML.SelectNodes(".//def:decl", XmlNsManager);

            //        Project.Classes[i].Methods[j].Variables.AddRange(AddObjects(objectsList, Project.Classes[i]));
            //    }
            //}

            // WIP

            // Constructors
            for (int i = project.Classes.Count - 1; i >= 0; i--) // Reverse, because missing classes get added during this process.
            {
                if (project.Classes[i].ParentNamespace.Name == "404")
                {
                    continue;
                }

                XmlElement OOclassContent = project.Classes[i].XML["block"];

                // Constructors

                XmlNodeList constructorList = OOclassContent.SelectNodes("./def:constructor", SrcLoader.XmlNsManager);
                for (int j = 0; j < constructorList.Count; j++)
                {
                    if (string.IsNullOrEmpty(constructorList[j].InnerText))
                        continue;

                    XmlElement OOfunction = (XmlElement)constructorList[j];
                    Method _function = new Method
                    {
                        Name = OOfunction["name"].InnerText,
                        LOC = OOfunction.InnerText.Split('\n').Length,
                        ParentClass = project.Classes[i],
                        AccessModifier = AccessModifier.Public,
                        ReturnClass = project.Classes[i],
                        SourceCode = OOfunction.InnerText,
                        XML = OOfunction // TODO add params
                    };

                    project.Classes[i].Methods.Add(_function);
                }

                // Delegates

                // TODO
            }
        }

        /// <summary>
        /// Create a new namespace and add it to the project.
        /// </summary>
        /// <param name="namespaceXmlNode">The XmlNode of the namespace.</param>
        /// <param name="project">The OOS project where the namespace will be added.</param>
        private static void AddNamespace(XmlNode namespaceXmlNode, Project project)
        {
            // Do nothing if a namespace with this name already exists.
            if (project.GetNamespaceByName(namespaceXmlNode["name"].InnerText) != null)
                return;

            // Create & add new namespace.
            var _namespace = new Namespace()
            {
                Name = namespaceXmlNode["name"].InnerText
            };
            project.Namespaces.Add(_namespace);
        }

        /// <summary>
        /// Create a new class and add it to the project.
        /// </summary>
        /// <param name="classXmlNode">The XmlNode of the class.</param>
        /// <param name="srcMlUnit">The srcML Unit that contains this class.</param>
        /// <param name="project">The OOS project where this class will be added.</param>
        private static void AddClass(XmlNode classXmlNode, SrcUnit srcMlUnit, Project project)
        {
            // TODO
            if (srcMlUnit.Language == Language.Java && classXmlNode.ParentNode.Name == "expr")
            {
                Console.Write("skipping");
                return;
            }
            // TODO handle "EulerianTrailAlgorithm<TVertex, TEdge>"

            // Parent namespace.
            var parentNamespace = SrcML_Functions.GetParentNamespaceOfClass(classXmlNode, srcMlUnit.Language, project);

            // Imported namespaces.
            List<Namespace> importedNamespaces = SrcML_Functions.GetImportedNamespacesOfClass(classXmlNode, srcMlUnit.Language, project);

            // Create new class.
            var _class = new Class
            {
                Name = classXmlNode["name"].InnerText,
                LOC = classXmlNode.InnerText.Split('\n').Length,
                Specifier = XmlNodeFinder.FindSpecifier(classXmlNode),
                SourceCode = classXmlNode.InnerText,
                ImportedNamespaces = importedNamespaces,
                ParentNamespace = parentNamespace,
                Language = srcMlUnit.Language,
                FileLocation = srcMlUnit.SrcMlFileName,
                XML = (XmlElement)classXmlNode
            };
            _class.AccessModifier = Class.FindAccessModifierInSpecifier(_class.Specifier);

            // Add class to project and namespace.
            project.Classes.Add(_class);
            parentNamespace?.Classes.Add(_class);
        }

        /// <summary>
        /// Add a new enum to the project.
        /// </summary>
        /// <param name="enumXmlNode">The XmlNode of the enum.</param>
        /// <param name="srcMlUnit">The srcML unit that contains the enum.</param>
        /// <param name="project">The OOS project where the enum will be added.</param>
        private static void AddEnum(XmlNode enumXmlNode, SrcUnit srcMlUnit, Project project)
        {
            // Parent namespace.
            Namespace parentNamespace = SrcML_Functions.GetParentNamespaceOfClass(enumXmlNode, srcMlUnit.Language, project);

            // Enum members.
            List<Variable> members = SrcML_Functions.GetMembersOfEnum(enumXmlNode, srcMlUnit.Language);

            // New enum.
            var _enum = new OOS.Enum
            {
                Name = enumXmlNode["name"].InnerText,
                LOC = enumXmlNode.InnerText.Split('\n').Length, // TODO remove comment lines?
                Code = enumXmlNode.InnerText,
                Namespace = parentNamespace,
                Filename = srcMlUnit.SrcMlFileName,
                XML = (XmlElement)enumXmlNode,
                Members = members
            };
            _enum.AccessModifier = (enumXmlNode["specifier"] != null && enumXmlNode["specifier"].InnerText == "public") ? AccessModifier.Public : AccessModifier.Private; // TODO

            // Add enums to project and namespace.
            project.Enums.Add(_enum); 
            parentNamespace?.Enums.Add(_enum);
        }

        /// <summary>
        /// Add a new interface to the project.
        /// </summary>
        /// <param name="interfaceXmlNode">The XmlNode of the interface.</param>
        /// <param name="srcMlUnit">The srcML unit that contains the interface.</param>
        /// <param name="project">The OOS project where the interface will be added.</param>
        private static void AddInterface(XmlNode interfaceXmlNode, SrcUnit srcMlUnit, Project project)
        {
            // Parent namespace.
            Namespace parentNamespace = SrcML_Functions.GetParentNamespaceOfClass(interfaceXmlNode, srcMlUnit.Language, project);

            // Imported namespaces.
            List<Namespace> importedNamespaces = SrcML_Functions.GetImportedNamespacesOfClass(interfaceXmlNode, srcMlUnit.Language, project);

            // New interface.
            Interface _interface = new Interface
            {
                Name = interfaceXmlNode["name"].InnerText,
                LOC = interfaceXmlNode.InnerText.Split('\n').Length, // TODO remove comment lines?
                Specifier = XmlNodeFinder.FindSpecifier(interfaceXmlNode),
                SourceCode = interfaceXmlNode.InnerText,
                ParentNamespace = parentNamespace,
                ImportedNamespaces = importedNamespaces,
                FileLocation = srcMlUnit.SrcMlFileName,
                XML = (XmlElement)interfaceXmlNode
            };
            _interface.AccessModifier = Class.FindAccessModifierInSpecifier(_interface.Specifier);

            // Add interface to project & namespace.
            project.Interfaces.Add(_interface); // Add class to project.
            parentNamespace?.Interfaces.Add(_interface); // Add class to namespace.
        }

        /// <summary>
        /// Add a new method to the project.
        /// </summary>
        /// <param name="methodXmlNode"></param>
        /// <param name="unitLanguage"></param>
        /// <param name="project"></param>
        private static void AddMethod(XmlNode methodXmlNode, Language unitLanguage, Project project)
        {
            // Parent class.
            Class parentClass = SrcML_Functions.GetParentClassFromMethod(methodXmlNode, unitLanguage, project);
            if (parentClass == null)
            {
                return;
            }

            // Return class.
            Class returnClass = SrcML_Functions.FindClassByName(methodXmlNode["type"]["name"], parentClass, project, Converter.Settings.Include404Namespace);

            Method method = new Method
            {
                Name = methodXmlNode["name"].InnerText,
                LOC = methodXmlNode.InnerText.Split('\n').Length,
                Specifier = XmlNodeFinder.FindSpecifier(methodXmlNode["type"]),
                ParentClass = parentClass,
                ReturnClass = returnClass,
                SourceCode = methodXmlNode.InnerText,
                XML = (XmlElement)methodXmlNode
            };
            method.AccessModifier = Class.FindAccessModifierInSpecifier(method.Specifier);

            parentClass.Methods.Add(method);
        }

        //private void AddObject(XmlNode objectNode, Language unitLanguage)
        //{
        //    // Skipping stuff like enum decl.
        //    // An object schould lool like e.g <decl><type><specifier>private</specifier> <name>IslandGOConstructor</name></type> <name>islandGOConstructor</name></decl>
        //    if (objectNode["name"] == null || objectNode["type"] == null || objectNode["type"]["name"] == null)
        //    {
        //        //Console.WriteWarning("Skipping object!\n" + objectsList[i].InnerXml);
        //        return;
        //    }

        //    Class _class = XmlFunctions.FindParentClass(objectNode, unitLanguage);
        //    //Method _function = XmlFunctions.FindParentFunction(objectNode, unitLanguage);

        //    Variable _object = new Variable
        //    {
        //        Name = objectNode["name"].InnerText,
        //        Type = FindClassByName(objectNode["type"]["name"], _class, true),
        //        Specifier = FindSpecifier(objectNode["type"])
        //    };
        //    _object.AccessModifier = Class.FindAccessModifierInSpecifier(_object.Specifier);

        //    if (_object.Type == null)
        //    {
        //        Console.WriteError("Could not resolve type of object <i>" + _object.Name + "</i> in class <i>" + _class.Name + "</i>. Skipping this object!");
        //        return;
        //    }

        //    _class.Variables.Add(_object);
        //}

        //private static List<Variable> AddVariable(XmlNodeList objectsList, Class srcClass, Project project)
        //{
        //    List<Variable> objects = new List<Variable>();

        //    for (int i = 0; i < objectsList.Count; i++)
        //    {
        //        // Skipping stuff like enum decl.
        //        // An object schould lool like e.g <decl><type><specifier>private</specifier> <name>IslandGOConstructor</name></type> <name>islandGOConstructor</name></decl>
        //        if (objectsList[i]["name"] == null || objectsList[i]["type"] == null || objectsList[i]["type"]["name"] == null)
        //        {
        //            //Console.WriteWarning("Skipping object!\n" + objectsList[i].InnerXml);
        //            continue;
        //        }

        //        Variable _object = new Variable
        //        {
        //            Name = objectsList[i]["name"].InnerText,
        //            Type = SrcML_Functions.FindClassByName(objectsList[i]["type"]["name"], srcClass, project, Converter.Include404Namespace),
        //            Specifier = XML_Functions.FindSpecifier(objectsList[i]["type"])
        //        };
        //        _object.AccessModifier = Class.FindAccessModifierInSpecifier(_object.Specifier);

        //        if (_object.Type == null)
        //        {
        //            Console.WriteError("Could not resolve type of object <i>" + _object.Name + "</i> in class <i>" + srcClass.Name + "</i>. Skipping this object!");
        //            continue;
        //        }

        //        objects.Add(_object);
        //    }

        //    return objects;
        //}

        #endregion

        private static void SortProject(Project project)
        {
            project.Namespaces.Sort((x, y) => string.Compare(x.Name, y.Name));
            foreach (var n in project.Namespaces)
            {
                n.Classes.Sort((x, y) => string.Compare(x.Name, y.Name));
            }
        }


        #region Additional Dependencies

        /// <summary>
        /// Resolve all Class-EXTENDS->Class and Class-IMPLEMENTS->Interface relationships inside the project.
        /// </summary>
        public static void AddDependencies_ExtendsAndImplements(Project project)
        {
            for (int i = project.Classes.Count - 1; i >= 0; i--) // Reverse, because missing (404) classes might get added during this process.
            {
                if (project.Classes[i].ParentNamespace.Name == "404" || project.Classes[i].XML["super_list"] == null)
                {
                    continue; // Skip class.
                }

                // Find relationships in XML.
                List<XmlNode> implementedInterfaceNodes = XmlNodeFinder.FindImplementedInterfaceNodes(project.Classes[i].XML, project.Classes[i].Language);
                List<XmlNode> extendedClassNodes = XmlNodeFinder.FindExtendedClassNodes(project.Classes[i].XML, project.Classes[i].Language);

                // Find interface and add relationship.
                foreach (XmlElement interfaceNode in implementedInterfaceNodes)
                {
                    Interface _interface = SrcML_Functions.FindInterfaceByName(interfaceNode["name"], project.Classes[i]);
                    if (_interface != null)
                        project.Classes[i].ImplementedInterfaces.Add(_interface);
                }

                // Find class and add relationship.
                foreach (XmlElement classNode in extendedClassNodes)
                {
                    Class _class = SrcML_Functions.FindClassByName(classNode["name"], project.Classes[i], project, Converter.Settings.Include404Namespace);
                    if (_class != null)
                        project.Classes[i].BaseClasses.Add(_class);
                }
            }
        }

        /// <summary>
        /// Resolve all Method-Calls->Method relationships inside the project.
        /// </summary>
        public static void AddDependencies_Calls(Project project) // TODO consider ifs?
        {
            for (int i = 0; i < project.Classes.Count; i++)
            {
                if (project.Classes[i].ParentNamespace.Name == "404" || project.Classes[i].Methods.Count == 0)
                {
                    continue; // Skip class.
                }

                for (int j = 0; j < project.Classes[i].Methods.Count; j++)
                {
                    List<XmlNode> methodList = XmlNodeFinder.FindCalledMethods(project.Classes[i].Methods[j].XML, project.Classes[i].Language);
                    for (int k = 0; k < methodList.Count; k++)
                    {
                        Method calledMethod = SrcML_Functions.FindMethodByName(methodList[k]["name"], project.Classes[i].Methods[j], project);
                        if (calledMethod.SourceCode != "UNKOWN" && !project.Classes[i].Methods[j].MethodCalls.Contains(calledMethod)) // Code is "UNKNOWN" when the function was not found.
                        {
                            project.Classes[i].Methods[j].MethodCalls.Add(calledMethod); // Add method.
                        }
                    }
                }
            }
        }

        #endregion

    }
}