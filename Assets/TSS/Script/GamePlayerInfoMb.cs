using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenCvSharp;
using Cysharp.Threading.Tasks;

public class GamePlayerInfoMb : MonoBehaviour
{

    [SerializeField] private int playerNumber;
    [SerializeField] private TMP_InputField playerNameInputField;
    private GameSettingManager gameSettingManager;

    internal GamePlayerInfo gamePlayerInfo
    {
        get
        {
            GamePlayerInfo info = new GamePlayerInfo();
            info.playerNumber = playerNumber;
            info.playerName = playerNameInputField.text;
            info.cameraDeviceName = selectedWebcamDeviceName;
            return info;
        }
    }

    [SerializeField] private TMP_Dropdown webcamDropdown;
    internal string selectedWebcamDeviceName;

    [SerializeField] private RawImage webcamPreviewRawImage;
    private Texture2D webcamPreviewT2;
    private WebCamTexture webcamTexture;

    private bool initialized = false;


    private void OnEnable()
    {
        Initialize();
        webcamDropdown.onValueChanged.AddListener(OnWebcamDropdownValueChanged);
    }

    private void OnDisable()
    {
        webcamDropdown.onValueChanged.RemoveListener(OnWebcamDropdownValueChanged);
        if (webcamTexture)
        {
            webcamTexture.Stop();
        }
    }

    private void Initialize()
    {
        if (initialized) return;

        webcamPreviewRawImage.texture = webcamTexture;
        webcamPreviewT2 = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        webcamPreviewRawImage.texture = webcamPreviewT2;
        initialized = true;
    }

    internal void SetManager(GameSettingManager manager)
    {
        this.gameSettingManager = manager;
    }

    internal void SetWebcamDropdown(List<string> deviceNameList)
    {
        webcamDropdown.ClearOptions();

        List<string> strOptions = new List<string>();

        strOptions.Add("Select web camera");

        strOptions.AddRange(deviceNameList);

        webcamDropdown.AddOptions(strOptions);

        bool selectedIsExist = false;

        for(int i = 0; i < webcamDropdown.options.Count; i++)
        {
            if(webcamDropdown.options[i].text == selectedWebcamDeviceName)
            {
                webcamDropdown.SetValueWithoutNotify(i);
                selectedIsExist = true;
                break;
            }
        }

        if (!selectedIsExist)
        {
            webcamDropdown.value = 0;
        }

    }

    private void OnWebcamDropdownValueChanged(int value)
    {
        if(value == 0)
        {
            selectedWebcamDeviceName = null;
        }
        else
        {
            selectedWebcamDeviceName = webcamDropdown.options[value].text;
        }
        gameSettingManager.OnSelectWebcam();
        ChangeWebcam(webcamDropdown.options[value].text);
    }

    internal void ChangeWebcam(string deviceName)
    {

        bool isValidDeviceName = false;
        WebCamDevice[] devices = WebCamTexture.devices;
        for(int i = 0; i < devices.Length; i++)
        {
            if(deviceName == devices[i].name)
            {
                isValidDeviceName = true;
                break;
            }
        }

        if (webcamTexture)
        {
            if(webcamTexture.deviceName != deviceName)
            {
                if (webcamTexture.isPlaying)
                {
                    webcamTexture.Stop();
                }
                Destroy(webcamTexture);
            }
        }

        if (isValidDeviceName)
        {
            webcamTexture = new WebCamTexture(deviceName, 960, 540, 32);

            webcamTexture.Play();
        }
    }
    /*
    internal async UniTask UpdateWebcamPreviewAsync(CancellationToken cancellationToken)
    {
        
        if (webcamTexture)
        {
            Debug.Log($"Update with {webcamTexture.deviceName}, {webcamTexture.width}, {webcamTexture.height}");
            Mat webcamMat = OpenCvSharp.Unity.TextureToMat(webcamTexture);

            webcamPreviewT2.Reinitialize(webcamTexture.width, webcamTexture.height);

            OpenCvSharp.Unity.MatToTexture(webcamMat, webcamPreviewT2);
        }
        
    }
    */
    internal void UpdateWebcamPreview()
    {
        if (webcamTexture)
        {
            Mat webcamMat = OpenCvSharp.Unity.TextureToMat(webcamTexture);

            webcamPreviewT2.Reinitialize(webcamTexture.width, webcamTexture.height);

            OpenCvSharp.Unity.MatToTexture(webcamMat, webcamPreviewT2);
        }
    }

}

[Serializable]
public class GamePlayerInfo
{
    public int playerNumber;
    public string playerName;
    public string cameraDeviceName;
}