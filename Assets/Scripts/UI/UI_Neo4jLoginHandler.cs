using UnityEngine;
using UnityEngine.UI;

public class UI_Neo4jLoginHandler : MonoBehaviour
{
    public InputField Neo4jURI;
    public InputField Neo4jDbName;
    public InputField Neo4jUsername;
    public InputField Neo4jPassword;

    public void UpdateNeo4jLoginData ()
    {
        Src2Neo.Neo4j.ConnectionSettings.URI = Neo4jURI.text;
        Src2Neo.Neo4j.ConnectionSettings.DB_Name = Neo4jDbName.text;
        Src2Neo.Neo4j.ConnectionSettings.Username = Neo4jUsername.text;
        Src2Neo.Neo4j.ConnectionSettings.Password = Neo4jPassword.text;
    }
}
