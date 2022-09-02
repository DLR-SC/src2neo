using OOS;
using System;
using System.Threading.Tasks;

namespace Src2Neo
{
    /// <summary>
    /// This class contains all events that other classes can call and subscribe to.
    /// </summary>
    public class Src2NeoEvents
    {

        /// <summary>
        /// Called by the <see cref="Converter"/> when it starts.
        /// </summary>
        public static BasicEvent OnStart { get; set; }

        /// <summary>
        /// Called by the <see cref="Converter"/> when it stops.
        /// </summary>
        public static BasicEvent OnStop { get; set; }

        /// <summary>
        /// Called by the <see cref="Converter"/> when it makes progress (0f = no progress; 1f = finished);
        /// </summary>
        public static FloatEventAsync OnProgressAsync { get; set; }




        public static DataLoadingDone OnDataLoadingDone { get; set; }

        public static BasicEvent OnDataWritingDone { get; set; }


        public delegate void StringEvent(string value);
        public delegate Task FloatEventAsync(float value);
        public delegate void DataLoadingDone(Project project);
        public delegate void BasicEvent();
    }

}

