using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml;
using System;

namespace Src2Neo.SrcML
{
    /// <summary>
    /// This class extracts all srcML Units from a scrML XML file.
    /// </summary>
    public static class SrcLoader
    {
        public static XmlNamespaceManager XmlNsManager { private set; get; } // XML namespace manager needed for XPath expressions.

        /// <summary>
        /// Load a srcML XML file and extract all srcML units.
        /// </summary>
        public static async Task<SrcUnit[]> LoadAsync(string xmlFilePath)
        {
            // Load the xml file.
            var doc = ReadFile(xmlFilePath);

            // Extract all srcML units (i.e. files) from the xml file.
            var srcUnits = ExtractUnitsParallel(doc.DocumentElement);

            // Update UI.
            await Src2NeoEvents.OnProgressAsync?.Invoke(0.05f);

            return srcUnits.ToArray();
        }

        /// <summary>
        /// Loads a <see cref="XmlDocument"/> located at the <paramref name="xmlFilePath"/>.
        /// </summary>
        private static XmlDocument ReadFile (string xmlFilePath)
        {
            // Create new XmlDocument.
            var doc = new XmlDocument()
            {
                PreserveWhitespace = true
            };

            // Try load the xml file.
            try
            {
                doc.Load(xmlFilePath);
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new Exception("SrcLoader: Loading the xml file at " + xmlFilePath + " failed!");
            }

            // Load XML Namespace.
            XmlNsManager = new XmlNamespaceManager(doc.NameTable);
            XmlNsManager.AddNamespace("def", "http://www.srcML.org/srcML/src"); // Add the default SrcML namespace.

            return doc;
        }

        /// <summary>
        /// Create a <see cref="SrcUnit"/> object for each "unit" tag in the srcML file.
        /// When a <see cref="SrcUnit"/> is created, it sets up pointers to relevant xml nodes (imports, namespace, classes, interfaces, enums, functions, objects).
        /// </summary>
        private static ConcurrentBag<SrcUnit> ExtractUnitsParallel(XmlNode xmlRoot)
        {
            var unitNodes = XmlNodeFinder.FindUnits(xmlRoot); // Get all <unit> nodes.

            if (unitNodes is null || unitNodes.Count == 0)
                throw new Exception("SrcLoader: Did not find <unit> tags inside the xml file. Please make shure you are using a srcML file");

            var srcUnits = new ConcurrentBag<SrcUnit>();

            Parallel.For(0, unitNodes.Count, i =>
            {
                if (!string.IsNullOrEmpty(unitNodes[i].InnerText))
                {
                    srcUnits.Add(new SrcUnit(unitNodes[i]));
                }
            });

            return srcUnits;
        }
    }
}
