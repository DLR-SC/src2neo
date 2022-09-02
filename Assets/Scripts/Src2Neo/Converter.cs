using Src2Neo.SrcML;
using Src2Neo.OOSConv;
using Src2Neo.Neo4j;

namespace Src2Neo
{
    /// <summary>
    /// This class handels the srcML file to graph database conversion.
    /// First, the <see cref="SrcLoader"/> extracts <see cref="SrcUnit"/>s from the srcML file. Then, the <see cref="OOSConverter"/> 
    /// identifies all software components and their relationships. Finally, the <see cref="NeoWriter"/> writes the software structure 
    /// into a Neo4j graph database.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Settings used by the <see cref="Converter"/> on the next conversion.
        /// </summary>
        public static ConverterSettings Settings { get; private set; }

        private static bool _isRunning;

        
        public static void ResetConverterSettings()
        {
            Settings = new ConverterSettings();
            _isRunning = false;
        }
        
        public static void StartConversion()
        {
            if (_isRunning)
                return;

            if (Settings is null)
                ResetConverterSettings();

            ConvertAsync();
        }

        private static async void ConvertAsync()
        {
            _isRunning = true;
            Src2NeoEvents.OnStart?.Invoke();

            // Extract srcML Units.
            var srcML_Units = await SrcLoader.LoadAsync(Settings.XmlPath);

            // Convert srcML Units to OOS structure.
            var OOS_Project = await OOSConverter.ConvertAsync(srcML_Units);

            // Write OOS structure to Neo4j database.
            await NeoWriter.WriteAsync(OOS_Project);

            _isRunning = false;
            Src2NeoEvents.OnStop?.Invoke();
        }
    }
}
