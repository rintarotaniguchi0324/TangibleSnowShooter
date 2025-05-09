using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


public class GameSettingManager : MonoBehaviour
{

    [SerializeField] private GameFlowParameter gameFlowParameter;

    [SerializeField] private GamePlayerInfoMb[] gamePlayerInfoMbArray;

    [SerializeField] private Button startButton;

    private bool initialized = false;

    private void OnEnable()
    {
        for(int i = 0; i < gamePlayerInfoMbArray.Length; i++)
        {
            gamePlayerInfoMbArray[i].SetManager(this);
        }
        CheckWebcamDevices();
        startButton.onClick.AddListener(OnStartButtonClick);
        Initialize();
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(OnStartButtonClick);
    }

    private void Initialize()
    {
        if (initialized) return;

        UniTask loopUpdateTask = LoopUpdateWebcamPreviewAsync(destroyCancellationToken);

        initialized = true;
    }

    internal void CheckWebcamDevices()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        for(int i = 0; i < gamePlayerInfoMbArray.Length; i++)
        {
            List<string> deviceNameList = new List<string>();

            for(int j = 0; j < devices.Length; j++)
            {
                bool isNotSelected = true;
                for (int k = 0; k < gamePlayerInfoMbArray.Length; k++)
                {
                    if (k == i) continue;
                    if(devices[j].name == gamePlayerInfoMbArray[k].selectedWebcamDeviceName)
                    {
                        isNotSelected = false;
                        break;
                    }
                }
                if (isNotSelected)
                {
                    deviceNameList.Add(devices[j].name);
                }
            }

            gamePlayerInfoMbArray[i].SetWebcamDropdown(deviceNameList);
        }

    }

    internal void OnSelectWebcam()
    {
        CheckWebcamDevices();

        bool isPrepared = true;

        for (int i = 0; i < gamePlayerInfoMbArray.Length; i++)
        {
            if (string.IsNullOrEmpty(gamePlayerInfoMbArray[i].selectedWebcamDeviceName))
            {
                isPrepared = false;
            }
        }

        startButton.interactable = isPrepared;

    }

    private async UniTask LoopUpdateWebcamPreviewAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            for (int i = 0; i < gamePlayerInfoMbArray.Length; i++)
            {
                gamePlayerInfoMbArray[i].UpdateWebcamPreview();
                if (cancellationToken.IsCancellationRequested) return;
                await UniTask.WaitForEndOfFrame(this);
            }
        }
    }

    private void OnStartButtonClick()
    {
        gameFlowParameter.gamePlayerInfoArray = new GamePlayerInfo[gamePlayerInfoMbArray.Length];
        for (int i = 0; i < gamePlayerInfoMbArray.Length; i++)
        {
            gameFlowParameter.gamePlayerInfoArray[i] = gamePlayerInfoMbArray[i].gamePlayerInfo;
        }
        SceneManager.LoadScene(2);
    }

}
