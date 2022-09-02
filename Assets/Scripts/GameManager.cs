using UnityEngine;
using Src2Neo;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The instance of this class.
    /// </summary>
    public static GameManager Instance { get; private set; }
        

    // Called by Unity on application start.
    void Awake()
    {
        Instance = this;
        Converter.ResetConverterSettings();
    }

    /// <summary>
    /// Call this to start the conversion.
    /// </summary>
    public void Convert ()
    {
#if UNITY_EDITOR
        // This code is only executed when run from the Unity Editor.
        // This is required, because the Drag&Drop script does not work in the Editor.
        Converter.Settings.XmlPath = EditorUtility.OpenFilePanel("Select srcML file", "", "xml");
#endif
        Converter.StartConversion();
    }
}
