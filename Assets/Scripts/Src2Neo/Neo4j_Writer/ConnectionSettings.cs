namespace Src2Neo.Neo4j
{
    /// <summary>
    /// This class contains the information used to connect to the Neo4j db.
    /// </summary>
    public static class ConnectionSettings
    {
        public static string URI { get; set; } // The URI of the Neo4j db.
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string DB_Name { get; set; }

        public static bool IsComplete ()
        {
            return !string.IsNullOrEmpty(URI) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(DB_Name);
        }
    }
}
