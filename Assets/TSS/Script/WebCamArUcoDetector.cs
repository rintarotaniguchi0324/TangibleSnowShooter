using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using Cysharp.Threading.Tasks;

public class WebCamArUcoDetector : MonoBehaviour
{
    [SerializeField] private string testDeviceName = "FHD Camera";

    private string specifiedDeviceName;

    private WebCamTexture webCamTexture;

    [SerializeField] private RawImage previewRawImage;
    private Texture2D previewTexture;



    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        webCamTexture.Stop();
        
    }

    private bool initialized = false;
    /// <summary>
    /// When the instance is first enabled, initialize the instance.
    /// </summary>
    private void Initialize()
    {
        if (initialized) return;
        SetDeviceName();
        webCamTexture = new WebCamTexture(specifiedDeviceName, 1920, 1080, 32);
        webCamTexture.Play();
        if (previewRawImage)
        {
            previewRawImage.texture = webCamTexture;
        }
        
        foreach(var device in WebCamTexture.devices)
        {
            Debug.Log(device.name);
        }

        initialized = true;
    }

    /// <summary>
    /// Set a camera device name before initialize.
    /// </summary>
    /// <param name="deviceName"></param>
    public void SetDeviceName(string deviceName = null)
    {
        if (string.IsNullOrEmpty(deviceName))
        {
            deviceName = testDeviceName;
        }

        specifiedDeviceName = testDeviceName;
    }

    

    
}
