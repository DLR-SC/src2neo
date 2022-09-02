using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Neo4j.Driver;
using OOS;

namespace Src2Neo.Neo4j
{
    /// <summary>
    /// This class writes a OOS <see cref="Project"/> to a Neo4J database using the .Net Neo4J driver.
    /// The data is added via cypher queries (heavily focused on using Neo4J Node IDs to minimize the number of queries).
    /// </summary>
    public static class NeoWriter
    {        
        private static IDriver _driver; // Driver used to communicate with the Neo4j graph db.
        private static IAsyncSession _session; // Session with a specific database of the Neo4j graph db (e.g., used to send Cypher queries).
        private static IResultCursor _cursor; // Cursor used to reciefe answers from the Neo4j graph db.


        /// <summary>
        /// Overwrite the Neo4j db with the <see cref="Project"/> data.
        /// </summary>
        public static async Task WriteAsync(Project project)
        {
            Console.Write("<color=green><b>Starting Neo4J Writing</b></color>");

            if (!ConnectionSettings.IsComplete())
                throw new Exception("Neo4j_Writer.cs: Neo4j login data is not complete!");

            _driver = GraphDatabase.Driver(ConnectionSettings.URI, AuthTokens.Basic(ConnectionSettings.Username, ConnectionSettings.Password));  // UIManager.Instance.GetNeo4jURI(), AuthTokens.Basic(UIManager.Instance.GetNeo4jUsername(), UIManager.Instance.GetNeo4jPassword()));
            _session = _driver.AsyncSession(o => o.WithDatabase(ConnectionSettings.DB_Name)); //UIManager.Instance.GetNeo4jDbName()));

            string query = "";
            string debugText = "";

            try
            {
                await _session.RunAsync("MATCH (n) DETACH DELETE n"); // DELETE EVERYTHING IN THE DATABASE.

                //
                // NAMESPACE-CONTAINS->CLASS
                // NAMESPACE-CONTAINS->INTERFACE
                // NAMESPACE-CONTAINS->ENUM
                //
                Console.WriteLine("Writing NAMESPACE-CONTAINS->CLASS & NAMESPACE-CONTAINS->INTERFACE & NAMESPACE-CONTAINS->ENUM");
                for (int i = 0; i < project.Namespaces.Count; i++)
                {
                    if (!Converter.Settings.Include404Namespace && project.Namespaces[i].Name == "404")
                    {
                        project.Namespaces[i].Neo4JID = -1;

                        foreach (var _class in project.Namespaces[i].Classes)
                        {
                            _class.Neo4JID = -1;
                        }
                        foreach (var _interface in project.Namespaces[i].Interfaces)
                        {
                            _interface.Neo4JID = -1;
                        }
                        foreach (var _enum in project.Namespaces[i].Enums)
                        {
                            _enum.Neo4JID = -1;
                        }
                        continue;
                    }
                    if (!Converter.Settings.IncludeEmptyNamespaces && project.Namespaces[i].Classes.Count == 0)
                    {
                        project.Namespaces[i].Neo4JID = -1;
                        continue;
                    }

                    query += "CREATE " + NamespaceToCypher(project.Namespaces[i], "n") + "\n";
                    // "Create (n)-[CONTAINS]->Class"
                    for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                    {
                        query += "CREATE (n)-[:" + CypherSettings.NamespaceContainsClassTag + "]->" + ClassToCypher(project.Namespaces[i].Classes[j], "c" + j) + "\n";
                    }
                    // "Create (n)-[CONTAINS]->Interface"
                    for (int j = 0; j < project.Namespaces[i].Interfaces.Count; j++)
                    {
                        query += "CREATE (n)-[:" + CypherSettings.NamespaceContainsInterfaceTag + "]->" + InterfaceToCypher(project.Namespaces[i].Interfaces[j], "i" + j) + "\n";
                    }
                    // "Create (n)-[CONTAINS]->Enum"
                    for (int j = 0; j < project.Namespaces[i].Enums.Count; j++)
                    {
                        query += "CREATE (n)-[:" + CypherSettings.NamespaceContainsEnumTag + "]->" + EnumToCypher(project.Namespaces[i].Enums[j], "e" + j) + "\n";
                    }
                    query += "RETURN [n";
                    // ", class1, class2 ..."
                    for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                    {
                        query += ", c" + j;
                    }
                    // ", interface1, interface2 ..."
                    for (int j = 0; j < project.Namespaces[i].Interfaces.Count; j++)
                    {
                        query += ", i" + j;
                    }
                    // ", enum1, enum2 ..."
                    for (int j = 0; j < project.Namespaces[i].Enums.Count; j++)
                    {
                        query += ", e" + j;
                    }
                    query += "] AS list";

                    _cursor = await _session.RunAsync(query);
                    var NodeList = await _cursor.SingleAsync(record => record["list"].As<IList<INode>>()); // Store return nodes in a list.
                    await _cursor.ConsumeAsync();

                    // Store namespace, class and interface Neo4J ids.
                    project.Namespaces[i].Neo4JID = NodeList[0].Id;
                    for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                    {
                        project.Namespaces[i].Classes[j].Neo4JID = NodeList[1 + j].Id;
                    }
                    for (int j = 0; j < project.Namespaces[i].Interfaces.Count; j++)
                    {
                        project.Namespaces[i].Interfaces[j].Neo4JID = NodeList[1 + project.Namespaces[i].Classes.Count + j].Id;
                    }
                    for (int j = 0; j < project.Namespaces[i].Enums.Count; j++)
                    {
                        project.Namespaces[i].Enums[j].Neo4JID = NodeList[1 + project.Namespaces[i].Classes.Count + project.Namespaces[i].Interfaces.Count + j].Id;
                    }

                    query = "";
                }
                Console.Write("NAMESPACE-CONTAINS->CLASS & NAMESPACE-CONTAINS->INTERFACE & NAMESPACE-CONTAINS->ENUM Done!");
                await Src2NeoEvents.OnProgressAsync?.Invoke(0.6f); // UIManager.Instance.UpdateLoadingProgressAsync(0.6f);


                if (Converter.Settings.IncludeFields)
                {
                    //
                    // CLASS-CONTAINS->FUNCTION
                    // CLASS-CONTAINS->OBJECT
                    //
                    Console.WriteLine("Writing CLASS - CONTAINS->FUNCTION & CLASS - CONTAINS->OBJECT");
                    for (int i = 0; i < project.Namespaces.Count; i++)
                    {
                        if (project.Namespaces[i].Neo4JID == -1)
                        {
                            continue;
                        }

                        for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                        {
                            query = "MATCH (c:" + CypherSettings.ClassTypeTag + ")\n" + "WHERE id(c)= " + project.Namespaces[i].Classes[j].Neo4JID + "\n";
                            // "CREATE (c)-[CONTAINS]->(:Method)"
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Methods.Count; k++)
                            {
                                query += "CREATE (c)-[:" + CypherSettings.ClassContainsFunctionTag + "]->" + FunctionToCypher(project.Namespaces[i].Classes[j].Methods[k], "f" + k) + "\n";
                            }
                            // "CREATE (c)-[CONTAINS]->(:Object)"
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Variables.Count; k++)
                            {
                                query += "CREATE (c)-[:" + CypherSettings.ClassContainsObjectTag + "]->" + ObjectToCypher(project.Namespaces[i].Classes[j].Variables[k], "o" + k) + "\n";
                            }
                            query += "RETURN [c";
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Methods.Count; k++)
                            {
                                query += ", f" + k;
                            }
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Variables.Count; k++)
                            {
                                query += ", o" + k;
                            }
                            query += "] AS list"; // RETURN [c, k0, k1, k2, o0, o1, o2, o3, o4, o5, o6] AS list

                            _cursor = await _session.RunAsync(query);
                            var NodeList = await _cursor.SingleAsync(record => record["list"].As<IList<INode>>()); // Store class, function and object nodes in a list.
                            await _cursor.ConsumeAsync();

                            // Store function & object Neo4J ids.
                            //project.Namespaces[i].Classes[j].Neo4JID = NodeList[0].Id;
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Methods.Count; k++)
                            {
                                project.Namespaces[i].Classes[j].Methods[k].Neo4jID = NodeList[1 + k].Id;
                            }
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Variables.Count; k++)
                            {
                                project.Namespaces[i].Classes[j].Variables[k].Neo4JID = NodeList[1 + project.Namespaces[i].Classes[j].Methods.Count + k].Id;
                            }

                            query = "";
                        }
                    }
                    Console.Write("CLASS-CONTAINS->FUNCTION & CLASS-CONTAINS->OBJECT Done!");
                    await Src2NeoEvents.OnProgressAsync?.Invoke(0.65f); //UIManager.Instance.UpdateLoadingProgressAsync(0.65f);
                }

                //
                // CLASS-IMPORTS->NAMESPACE
                //
                Console.WriteLine("Writing CLASS-IMPORTS->NAMESPACE");
                for (int i = 0; i < project.Namespaces.Count; i++)
                {
                    if (project.Namespaces[i].Neo4JID == -1)
                    {
                        continue;
                    }
                    for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                    {
                        if (project.Namespaces[i].Classes[j].ImportedNamespaces.Count == 0)
                            continue; // Skip if the class imports nothing.

                        string match = "MATCH (c: " + CypherSettings.ClassTypeTag + ")";
                        string where = "WHERE id(c)= " + project.Namespaces[i].Classes[j].Neo4JID + " ";
                        string create = "CREATE ";
                        for (int k = 0; k < project.Namespaces[i].Classes[j].ImportedNamespaces.Count; k++)
                        {
                            match += ", (n" + k + ": " + CypherSettings.NamespaceTypeTag + ")";
                            where += " AND id(n" + k + ")= " + project.Namespaces[i].Classes[j].ImportedNamespaces[k].Neo4JID + " ";
                            create += "(c)-[:" + CypherSettings.ClassImportsNamespaceTag + "]->(n" + k + ")" + (k == project.Namespaces[i].Classes[j].ImportedNamespaces.Count - 1 ? "" : ", ");
                        }
                        query = match + "\n" + where + "\n" + create;

                        await _session.RunAsync(query);

                        query = "";
                    }
                }
                Console.Write("CLASS-IMPORTS->NAMESPACE Done!");
                await Src2NeoEvents.OnProgressAsync?.Invoke(0.7f); //UIManager.Instance.UpdateLoadingProgressAsync(0.7f);


                //
                // CLASS-EXTENDS->CLASS
                //
                Console.WriteLine("Writing CLASS-EXTENDS->CLASS");
                for (int i = 0; i < project.Namespaces.Count; i++)
                {
                    if (project.Namespaces[i].Neo4JID == -1)
                    {
                        continue;
                    }
                    for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                    {
                        if (project.Namespaces[i].Classes[j].BaseClasses.Count == 0)
                            continue; // Skip if the class.

                        string match = "MATCH (c: " + CypherSettings.ClassTypeTag + ")";
                        string where = "WHERE id(c)= " + project.Namespaces[i].Classes[j].Neo4JID + " ";
                        string create = "CREATE ";
                        for (int k = 0; k < project.Namespaces[i].Classes[j].BaseClasses.Count; k++)
                        {
                            match += ", (n" + k + ": " + CypherSettings.ClassTypeTag + ")";
                            where += " AND id(n" + k + ")= " + project.Namespaces[i].Classes[j].BaseClasses[k].Neo4JID + " ";
                            create += "(c)-[:" + CypherSettings.ClassInheritsFromClassTag + "]->(n" + k + ")" + (k == project.Namespaces[i].Classes[j].BaseClasses.Count - 1 ? "" : ", ");
                        }
                        query = match + "\n" + where + "\n" + create;

                        await _session.RunAsync(query);

                        query = "";
                    }
                }
                Console.Write("CLASS-EXTENDS->CLASS Done!");
                await Src2NeoEvents.OnProgressAsync?.Invoke(0.75f); //UIManager.Instance.UpdateLoadingProgressAsync(0.75f);


                //
                // CLASS-IMPLEMENTS->INTERFACE
                //
                Console.WriteLine("Writing CLASS-IMPLEMENTS->INTERFACE");
                for (int i = 0; i < project.Namespaces.Count; i++)
                {
                    if (project.Namespaces[i].Neo4JID == -1)
                    {
                        continue;
                    }
                    for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                    {
                        if (project.Namespaces[i].Classes[j].ImplementedInterfaces.Count == 0)
                            continue; // Skip if the class.

                        string match = "MATCH (c: " + CypherSettings.ClassTypeTag + ")";
                        string where = "WHERE id(c)= " + project.Namespaces[i].Classes[j].Neo4JID + " ";
                        string create = "CREATE ";
                        for (int k = 0; k < project.Namespaces[i].Classes[j].ImplementedInterfaces.Count; k++)
                        {
                            match += ", (i" + k + ": " + CypherSettings.InterfaceTypeTag + ")";
                            where += " AND id(i" + k + ")= " + project.Namespaces[i].Classes[j].ImplementedInterfaces[k].Neo4JID + " ";
                            create += "(c)-[:" + CypherSettings.ClassImplementsInterfaceTag + "]->(i" + k + ")" + (k == project.Namespaces[i].Classes[j].ImplementedInterfaces.Count - 1 ? "" : ", ");
                        }
                        query = match + "\n" + where + "\n" + create;

                        await _session.RunAsync(query);

                        query = "";
                    }
                }
                Console.Write("CLASS-IMPLEMENTS->INTERFACE Done!");
                await Src2NeoEvents.OnProgressAsync?.Invoke(0.8f); //UIManager.Instance.UpdateLoadingProgressAsync(0.8f);


                if (Converter.Settings.IncludeFields)
                {
                    //
                    // FUNCTION-CONTAINS->OBJECT
                    //
                    Console.WriteLine("Writing FUNCTION-CONTAINS->OBJECT");
                    for (int i = 0; i < project.Namespaces.Count; i++)
                    {
                        if (project.Namespaces[i].Neo4JID == -1)
                        {
                            continue;
                        }
                        for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                        {
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Methods.Count; k++)
                            {
                                if (project.Namespaces[i].Classes[j].Methods.Count == 0 || project.Namespaces[i].Classes[j].Methods[k].Variables.Count == 0)
                                    continue; // Skip if class has no functions or function has no objects.

                                query = "MATCH (f:" + CypherSettings.FunctionTypeTag + ")\n WHERE id(f)= " + project.Namespaces[i].Classes[j].Methods[k].Neo4jID + "\n";
                                for (int l = 0; l < project.Namespaces[i].Classes[j].Methods[k].Variables.Count; l++)
                                {
                                    query += "CREATE (f)-[:" + CypherSettings.FunctionContainsObjectTag + "]->" + ObjectToCypher(project.Namespaces[i].Classes[j].Methods[k].Variables[l], "o" + l) + "\n";
                                }
                                // RETURN [0, 1, 2, 3, 4, 5, 6, 7, 8, 9] AS list
                                query += "RETURN [";
                                for (int l = 0; l < project.Namespaces[i].Classes[j].Methods[k].Variables.Count; l++)
                                {
                                    query += "o" + l + (l == project.Namespaces[i].Classes[j].Methods[k].Variables.Count - 1 ? "" : ", ");
                                }
                                query += "] AS list";

                                _cursor = await _session.RunAsync(query);
                                var NodeList = await _cursor.SingleAsync(record => record["list"].As<IList<INode>>());
                                await _cursor.ConsumeAsync();

                                // Assign Neo4J IDs
                                project.Namespaces[i].Classes[j].Neo4JID = NodeList[0].Id;
                                for (int l = 0; l < project.Namespaces[i].Classes[j].Methods[k].Variables.Count; l++)
                                {
                                    project.Namespaces[i].Classes[j].Methods[k].Variables[l].Neo4JID = NodeList[l].Id;
                                }
                                query = "";
                            }
                        }
                    }
                    Console.Write("FUNCTION-CONTAINS->OBJECT Done!");
                    await Src2NeoEvents.OnProgressAsync?.Invoke(0.85f); //UIManager.Instance.UpdateLoadingProgressAsync(0.85f);


                    //
                    // ENUM-CONTAINS->OBJECT
                    //
                    Console.WriteLine("Writing ENUM-CONTAINS->OBJECT");
                    for (int i = 0; i < project.Enums.Count; i++)
                    {
                        if (project.Enums[i].Neo4JID == -1)
                        {
                            continue;
                        }

                        query = "MATCH (e:" + CypherSettings.EnumTypeTag + ")\n WHERE id(e)= " + project.Enums[i].Neo4JID + "\n";
                        for (int j = 0; j < project.Enums[i].Members.Count; j++)
                        {
                            query += "CREATE (e)-[:" + CypherSettings.EnumContainsMemberTag + "]->" + ObjectToCypher(project.Enums[i].Members[j], "o" + j) + "\n";
                        }
                        query += "RETURN [";
                        for (int j = 0; j < project.Enums[i].Members.Count; j++)
                        {
                            query += "o" + j + (j == project.Enums[i].Members.Count - 1 ? "" : ", ");
                        }
                        query += "] AS list";

                        _cursor = await _session.RunAsync(query);
                        var NodeList = await _cursor.SingleAsync(record => record["list"].As<IList<INode>>());
                        await _cursor.ConsumeAsync();

                        // Assign Neo4J IDs
                        for (int j = 0; j < project.Enums[i].Members.Count; j++)
                        {
                            project.Enums[i].Members[j].Neo4JID = NodeList[j].Id;
                        }
                        query = "";

                    }
                    Console.Write("ENUM-CONTAINS->OBJECT Done!");
                    await Src2NeoEvents.OnProgressAsync?.Invoke(0.9f); //UIManager.Instance.UpdateLoadingProgressAsync(0.9f);


                    //
                    // OBJECT-TYPEOF->CLASS
                    //
                    Console.WriteLine("Writing OBJECT-TYPEOF->CLASS");
                    for (int i = 0; i < project.Classes.Count; i++)
                    {
                        if (project.Classes[i].Neo4JID == -1)
                        {
                            continue;
                        }

                        string _match = "";
                        string _create = "";

                        // Varibles on class level
                        for (int j = 0; j < project.Classes[i].Variables.Count; j++)
                        {
                            if (project.Classes[i].Variables[j].Neo4JID == -1 || project.Classes[i].Variables[j].Type == null || project.Classes[i].Variables[j].Type.Neo4JID == -1) // Object or class was not loaded into the Neo4J database.
                                continue;

                            _match += "MATCH (o" + j + ":" + CypherSettings.ObjectTypeTag + ")\n WHERE id(o" + j + ")= " + project.Classes[i].Variables[j].Neo4JID + "\n";
                            _match += "MATCH (c" + j + ":" + CypherSettings.ClassTypeTag + ")\n WHERE id(c" + j + ")= " + project.Classes[i].Variables[j].Type.Neo4JID + "\n";
                            _create += "CREATE (o" + j + ")-[:" + CypherSettings.ObjectTypeOfTag + "]->(c" + j + ")" + "\n";
                        }

                        // Variables on function level
                        for (int j = 0; j < project.Classes[i].Methods.Count; j++)
                        {
                            for (int k = 0; k < project.Classes[i].Methods[j].Variables.Count; k++)
                            {
                                if (project.Classes[i].Methods[j].Variables[k].Neo4JID == -1 || project.Classes[i].Methods[j].Variables[k].Type == null || project.Classes[i].Methods[j].Variables[k].Type.Neo4JID == -1) // Object or class was not loaded into the Neo4J database.
                                    continue;

                                _match += "MATCH (o" + j + "o" + k + ":" + CypherSettings.ObjectTypeTag + ")\n WHERE id(o" + j + "o" + k + ")= " + project.Classes[i].Methods[j].Variables[k].Neo4JID + "\n";
                                _match += "MATCH (c" + j + "c" + k + ":" + CypherSettings.ClassTypeTag + ")\n WHERE id(c" + j + "c" + k + ")= " + project.Classes[i].Methods[j].Variables[k].Type.Neo4JID + "\n";
                                _create += "CREATE (o" + j + "o" + k + ")-[:" + CypherSettings.ObjectTypeOfTag + "]->(c" + j + "c" + k + ")" + "\n";
                            }
                        }

                        if (_match == "" || _create == "")
                            continue;

                        query = _match + _create;
                        _cursor = await _session.RunAsync(query);
                        await _cursor.ConsumeAsync();
                    }
                    Console.Write("OBJECT-TYPEOF->CLASS Done!");
                    await Src2NeoEvents.OnProgressAsync?.Invoke(0.95f); //UIManager.Instance.UpdateLoadingProgressAsync(0.95f);
                }

                if (Converter.Settings.IncludeFields)
                {
                    //
                    // FUNCTION-CALLS->FUNCTION
                    //
                    Console.WriteLine("Writing FUNCTION-CALLS->FUNCTION");
                    for (int i = 0; i < project.Namespaces.Count; i++)
                    {
                        for (int j = 0; j < project.Namespaces[i].Classes.Count; j++)
                        {
                            for (int k = 0; k < project.Namespaces[i].Classes[j].Methods.Count; k++)
                            {
                                if (project.Namespaces[i].Classes[j].Methods.Count == 0 || project.Namespaces[i].Classes[j].Methods[k].MethodCalls.Count == 0)
                                    continue;

                                string match = "MATCH (f:" + CypherSettings.FunctionTypeTag + ")";
                                string where = "WHERE id(f)= " + project.Namespaces[i].Classes[j].Methods[k].Neo4jID;
                                string create = "";

                                for (int l = 0; l < project.Namespaces[i].Classes[j].Methods[k].MethodCalls.Count; l++)
                                {
                                    match += ", (f" + l + ": " + CypherSettings.FunctionTypeTag + ")";
                                    where += " AND id(f" + l + ")= " + project.Namespaces[i].Classes[j].Methods[k].MethodCalls[l].Neo4jID;
                                    create += "CREATE (f)-[:" + CypherSettings.FunctionCallsFunctionTag + "]->(f" + l + ")\n";
                                }
                                query = match + "\n" + where + "\n" + create;

                                await _session.RunAsync(query);
                                query = "";
                            }
                        }
                    }
                    Console.Write("FUNCTION-CALLS->FUNCTION Done!");
                }
                await Src2NeoEvents.OnProgressAsync?.Invoke(1f); //UIManager.Instance.UpdateLoadingProgressAsync(1f);


                // Print how many namespaces, classes and functions were created.
                debugText = "<color=green><b>Added ";
                _cursor = await _session.RunAsync("MATCH (n:Namespace) RETURN count(n) as cn"); // Only one "count" call per cypher query possible.
                debugText += await _cursor.SingleAsync(record => record["cn"].As<string>());
                await _cursor.ConsumeAsync();
                debugText += " Namespaces, ";
                _cursor = await _session.RunAsync("MATCH (c:Class) RETURN count(c) as cc");
                debugText += await _cursor.SingleAsync(record => record["cc"].As<string>());
                await _cursor.ConsumeAsync();
                debugText += " Classes, and ";
                _cursor = await _session.RunAsync("MATCH (i:Interface) RETURN count(i) as ci");
                debugText += await _cursor.SingleAsync(record => record["ci"].As<string>());
                await _cursor.ConsumeAsync();
                debugText += " Interfaces, and ";
                _cursor = await _session.RunAsync("MATCH (e:Enum) RETURN count(e) as ce");
                debugText += await _cursor.SingleAsync(record => record["ce"].As<string>());
                await _cursor.ConsumeAsync();
                debugText += " Enums, and ";
                _cursor = await _session.RunAsync("MATCH (f:Method) RETURN count(f) as cf");
                debugText += await _cursor.SingleAsync(record => record["cf"].As<string>());
                await _cursor.ConsumeAsync();
                debugText += " Methods, and ";
                _cursor = await _session.RunAsync("MATCH (o:Object) RETURN count(o) as co");
                debugText += await _cursor.SingleAsync(record => record["co"].As<string>());
                await _cursor.ConsumeAsync();
                debugText += " Variables to Neo4J!</b></color>";
                Console.Write(debugText);
            }
            finally
            {
                await _session.CloseAsync();
            }
            await _driver.CloseAsync();

            Console.Write("Writing done!");
            Console.WriteLine(debugText);
        }




        // ##############
        // Helper Methods
        // ##############

        private static string NamespaceToCypher(Namespace _namespace, string variable = "")
        {
            return "( " + variable + ": " + CypherSettings.NamespaceTypeTag + " { " + CypherSettings.NamespaceNameTag + ": '" + _namespace.Name + "'})";
        }

        private static string ClassToCypher(Class _class, string variable = "")
        {
            return "( " + variable + ": " + CypherSettings.ClassTypeTag + " { " + CypherSettings.ClassNameTag + ": '" + _class.Name + "', LOC: '" + _class.LOC + "', AccessModifier: '" + _class.AccessModifier + "', Specifier: '" + ListToString(_class.Specifier) + "', FileName: '" + EscapeString(_class.FileLocation) + "'})";
        }

        private static string InterfaceToCypher(Interface _interface, string variable = "")
        {
            return "( " + variable + ": " + CypherSettings.InterfaceTypeTag + " { " + CypherSettings.InterfaceNameTag + ": '" + _interface.Name + "', LOC: '" + _interface.LOC + "', AccessModifier: '" + _interface.AccessModifier + "', FileName: '" + EscapeString(_interface.FileLocation) + "'})";
        }

        private static string EnumToCypher(OOS.Enum _enum, string variable = "")
        {
            return "( " + variable + ": " + CypherSettings.EnumTypeTag + " { " + CypherSettings.EnumNameTag + ": '" + _enum.Name + "', LOC: '" + _enum.LOC + "', AccessModifier: '" + _enum.AccessModifier + "', FileName: '" + EscapeString(_enum.Filename) + "'})";
        }

        private static string FunctionToCypher(Method _function, string variable = "")
        {
            return "(" + variable + ": " + CypherSettings.FunctionTypeTag + " { " + CypherSettings.FunctionNameTag + ": '" + _function.Name + "', LOC: '" + _function.LOC + "', AccessModifier: '" + _function.AccessModifier + (Converter.Settings.IncludeSourceCode ? "', Code: '" + EscapeString(_function.SourceCode) : "") + "'})";
        }

        private static string ObjectToCypher(Variable _object, string variable = "")
        {
            return "(" + variable + ": " + CypherSettings.ObjectTypeTag + " { " + CypherSettings.ObjectNameTag + ": '" + _object.Name + "', AccessModifier: '" + _object.AccessModifier + "'})"; // TODO add type
        }

        /// <summary>
        /// Converts a string to a Neo4J cypher query save string. This is e.g. needed for storing code (that might contain symbols like ') in the Neo4J database.
        /// </summary>
        /// <param name="stringToEscape"></param>
        /// <returns></returns>
        private static string EscapeString(string stringToEscape)
        {
            if (string.IsNullOrEmpty(stringToEscape))
                return "";

            int limit = 2000;

            if (stringToEscape.Length < limit) // Needed, because there is a max length EscapeDataString can handle.
            {
                return Uri.EscapeDataString(stringToEscape);
            }
            else
            {
                //return "Code was too long!";
                StringBuilder sb = new StringBuilder();
                int loops = stringToEscape.Length / limit;

                for (int i = 0; i <= loops; i++)
                {
                    if (i < loops)
                    {
                        sb.Append(Uri.EscapeDataString(stringToEscape.Substring(limit * i, limit)));
                    }
                    else
                    {
                        sb.Append(Uri.EscapeDataString(stringToEscape.Substring(limit * i)));
                    }
                }
                return sb.ToString();
            }
        }

        private static string ListToString(List<string> list)
        {
            string r = "";
            foreach (var item in list)
            {
                r += item + " ";
            }
            return r;
        }
    }

}
