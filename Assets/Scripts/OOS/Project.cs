using System.Collections.Generic;

namespace OOS
{

    public class Project
    {
        // Content
        public List<Namespace> Namespaces { get; private set; } // [0,*]
        public List<Class> Classes { get; private set; } // [1,*]
        public List<Interface> Interfaces { get; private set; } // [0,*]
        public List<Enum> Enums { get; private set; } // [0,*]


        public Project()
        {
            Namespaces = new List<Namespace>();
            Classes = new List<Class>();
            Interfaces = new List<Interface>();
            Enums = new List<Enum>();

            Namespaces.Add(new Namespace // This namespace contains all classes without namespace.
            {
                Name = ""
            });

            Namespaces.Add(new Namespace // This namespace contains all classes which namespace could not be resolved.
            {
                Name = "404"
            });
        }


        // ##########
        // Getter
        // ##########

        public Namespace GetNamespaceByName(string namespaceName)
        {
            for (int i = 0; i < Namespaces.Count; i++)
            {
                if (Namespaces[i].Name == namespaceName)
                    return Namespaces[i];
            }
            return null;
        }




        // ##########
        // Helper Methods
        // ##########

        /// <summary>
        /// Prints prjects statistics, inluding namepsace count, class count, interface count, and function count.
        /// </summary>
        public string GetProjectInformation()
        {
            int objectCount = 0;
            int functionCount = 0;
            for (int i = 0; i < Classes.Count; i++)
            {
                functionCount += Classes[i].Methods.Count;

                objectCount += Classes[i].Variables.Count;
                for (int j = 0; j < Classes[i].Methods.Count; j++)
                {
                    objectCount += Classes[i].Methods[j].Variables.Count;
                }
            }
            for (int i = 0; i < Enums.Count; i++)
            {
                objectCount += Enums[i].Members.Count;
            }

            return "<b>This project has " + (Namespaces.Count - 1) + " Namespaces, " + Classes.Count + " Classes, " + Interfaces.Count + " Interfaces, " + Enums.Count + " Enums, " + functionCount + " Methods, and " + objectCount + " Variables.</b>";
        }

        /// <summary>
        /// Returns the total LOC of all namespaces in this project.
        /// </summary>
        /// <returns>Total lines of code (LOC).</returns>
        public int GetTotalLOC()
        {
            int loc = 0;
            for (int i = 0; i < Namespaces.Count; i++)
            {
                loc += Namespaces[i].GetTotalLOC();
            }
            return loc;
        }
    }
}


