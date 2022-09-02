using System.Collections;
using System.Collections.Generic;

namespace OOS
{
    public class Namespace
    {
        public List<Class> Classes { get; private set; } // [1,*]
        public List<Interface> Interfaces { get; private set; } // [0,*]
        public List<Enum> Enums { get; private set; } // [0,*]
        public Namespace Parent_Namespace { get; set; } // [0,1]
        public string Name { get; set; }

        public long Neo4JID { get; set; }


        public Namespace()
        {
            Classes = new List<Class>();
            Interfaces = new List<Interface>();
            Enums = new List<Enum>();
            Parent_Namespace = null;
            Neo4JID = -1;
        }





        /// <summary>
        /// Checks if a class with this name already exists in this namespace.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>True if a class with this name already exists.</returns>
        public bool ContainsClass(string className)
        {
            for (int i = 0; i < Classes.Count; i++)
            {
                if (Classes[i].Name == className)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a interface with this name already exists in this namespace.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <returns>True if a interface with this name already exists.</returns>
        public bool ContainsInterface(string interfaceName)
        {
            for (int i = 0; i < Classes.Count; i++)
            {
                if (Classes[i].Name == interfaceName)
                    return true;
            }
            return false;
        }



        #region Getter

        /// <summary>
        /// Adds the lines of code of all classes inside this namespace.
        /// </summary>
        /// <returns>Summarized lines of code of all classes inside this namespace.</returns>
        public int GetTotalLOC()
        {
            int loc = 0;
            for (int i = 0; i < Classes.Count; i++)
            {
                loc += Classes[i].LOC;
            }
            return loc;
        }

        /// <summary>
        /// Counts the number of functions inside this namespace.
        /// </summary>
        /// <returns>Number of functions inside this namespace.</returns>
        public int GetNumberOfFunctions()
        {
            int functions = 0;
            for (int i = 0; i < Classes.Count; i++)
            {
                functions += Classes[i].Methods.Count;
            }
            return functions;
        }

        #endregion

    }
}
