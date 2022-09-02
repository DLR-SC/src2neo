/*
 * Based on https://github.com/Bunny83/UnityWindowsFileDrag-Drop 
 */

using System.Collections.Generic;
using UnityEngine;
using B83.Win32;
using Src2Neo;

public class FileDragAndDrop : MonoBehaviour
{
    void OnEnable()
    {
        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    // do something with the dropped file names. aPos will contain the 
    // mouse position within the window where the files has been dropped.
    void OnFiles(List<string> aFiles, POINT aPos)
    {   
        Converter.Settings.XmlPath = aFiles[0];
    }
}
