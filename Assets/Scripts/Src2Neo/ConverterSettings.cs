using Src2Neo.SrcML;
using System.Collections.Generic;

namespace Src2Neo
{
    /// <summary>
    /// This class contains the settings used by the converters.
    /// </summary>
    public class ConverterSettings
    {
        public bool Include404Namespace { get; set; }
        public bool IncludeEmptyNamespaces { get; set; }
        public bool IncludeFields { get; set; }
        public bool IncludeSourceCode { get; set; }
        public string XmlPath { get; set; } // The location of the srcML file.
        public Dictionary<SrcElement, string[]> NodeTags { get; private set; } // TODO allow multiple tags per srcElement per language



        public ConverterSettings()
        {
            // Default values.
            Include404Namespace = true;
            IncludeEmptyNamespaces = true;
            IncludeFields = true;
            IncludeSourceCode = true;
            XmlPath = "404";

            // Default srcML node tags for [ c# | java | c++ | default ].
            NodeTags = new()
            {
                { SrcElement.Namespace, new string[4]{"namespace", "package", "namespace", "" } }, // e.g. "using System.Collections.Generic;"
                { SrcElement.ImportedNamespace, new string[4]{"using", "import", "using", "" } }, // e.g. "namespace SrcML { }"
                { SrcElement.Class, new string[4]{"class", "class", "class", "" } },
                { SrcElement.Interface, new string[4]{ "interface", "interface", "", "" } },
                { SrcElement.Enum, new string[4]{ "enum", "enum", "enum", "" } },
                { SrcElement.Method, new string[4]{ "function", "function", "function", "" } },
                { SrcElement.Field, new string[4]{ "decl", "decl", "decl", "" } },
                { SrcElement.Constructor, new string[4]{ "constructor", "constructor", "constructor", "" } }
            };
        }
    }
}
