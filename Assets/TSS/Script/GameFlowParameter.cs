using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameFlowParameter", menuName = "GameFlowParameter")]
public class GameFlowParameter : ScriptableObject
{
    public GamePlayerInfo[] gamePlayerInfoArray;

    [ContextMenu("Check Web Cameras")]
    private void ChecWebcamToDebugLog()
    {
        var devices = WebCamTexture.devices;
        for(int i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);
        }

    }
}
