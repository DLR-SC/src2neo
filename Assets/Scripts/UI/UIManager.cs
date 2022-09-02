using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Src2Neo;

/// <summary>
/// This class stores and manages all UI elements of the application.
/// </summary>
public class UIManager : MonoBehaviour
{
    // UI Elements
    public Text SrcMlDragAndDropText;
    public Button StartButton;
    public Text DebugTextLeft;
    public Text DebugTextRight;
    public Slider ProgressSlider;

    #region Unity Functions

    private void Awake()
    {
        Src2NeoEvents.OnProgressAsync += UpdateLoadingProgressAsync;
        Src2NeoEvents.OnStart += DisableStartButton;
        Src2NeoEvents.OnStop += EnableStartButton;
    }

    private void Start()
    {
#if UNITY_EDITOR
        // This code is only executed when run from the Unity Editor.
        // This is required, because the Drag&Drop script does not work in the Editor.
        StartButton.interactable = true;
#endif
    }

    #endregion

    #region UI Input Handling

    private void EnableStartButton()
    {
        StartButton.interactable = true;
    }

    private void DisableStartButton ()
    {
        StartButton.interactable = false;
    }

    /// <summary>
    /// Called by UI Button OnClick().
    /// </summary>
    public void OnStartButtonPressed()
    {
        GameManager.Instance.Convert();
    }

    #endregion

    /// <summary>
    /// Called when a new xml path was set.
    /// </summary>
    public void SetSrcMlPath(string path)
    {
        SrcMlDragAndDropText.text = path;
        SrcMlDragAndDropText.fontSize = 20;
        SrcMlDragAndDropText.fontStyle = FontStyle.Bold;

        StartButton.interactable = true;
    }

    /// <summary>
    /// Prints a green debug log on the left side of the application.
    /// </summary>
    /// <param name="log">Text that should be displayed.</param>
    public void SetSrcMlDebugText(string log)
    {
        DebugTextLeft.text = log;
    }

    /// <summary>
    /// Prints a blue debug log on the right side of the application.
    /// </summary>
    /// <param name="log">Text that should be displayed.</param>
    public void SetNeo4jDebugText(string log)
    {
        DebugTextRight.text = log;
    }

    /// <summary>
    /// Sets the loading screen to a specific length.
    /// </summary>
    /// <param name="progress">Progress value [0,1].</param>
    public void SetLoadingProgress(float progress)
    {
        ProgressSlider.value = progress;
        Debug.Log("UIManager: Progress at " + progress);
    }

    public async Task UpdateLoadingProgressAsync (float progress)
    {
        SetLoadingProgress(progress);
        await Task.Delay(10); // Wait for UI to update.
    }


}
