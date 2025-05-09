using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using Cysharp.Threading.Tasks;


public class GameManager : MonoBehaviour
{
    [SerializeField] private GameFlowParameter gameFlowParameter;
    [SerializeField] private GameObject gamePlayerPrefab;
    [SerializeField] private GameObject gamePlayerViewPrefab;

    [SerializeField] private RectTransform uiViewParent;
    [SerializeField] private WaitingCoverController cover;

    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private AudioSource m_audioSourceBgm;
    internal AudioSource audioSourceBgm { get { return m_audioSourceBgm; } }
    [SerializeField] private AudioSource m_audioSourceSe;
    internal AudioSource audioSourceSe { get { return m_audioSourceSe; } }
    [SerializeField] private AudioClip audioClipBattleBgm;
    [SerializeField] private AudioClip audioClipResultBgm;
    [SerializeField] private AudioClip audioClipCountSe;
    [SerializeField] private AudioClip audioClipStartSe;
    [SerializeField] private AudioClip audioClipFinishSe;


    


    [SerializeField] private Dictionary m_arucoDictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
    public Dictionary arucoDictionary { get { return m_arucoDictionary; } }

    private GamePlayerInfo[] gamePlayerInfoArray;
    private GamePlayerController[] gamePlayerControllerArray;

    private CancellationTokenSource cts;


    private bool initialized = false;



    private void OnEnable()
    {
        Initialize();

        restartButton.onClick.AddListener(RestartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(RestartGame);
        exitButton.onClick.RemoveListener(ExitGame);
    }

    private void Initialize()
    {
        if (initialized) return;

        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        UniTask t = InitializeAsync(cts.Token);

        initialized = true;
    }

    private async UniTask InitializeAsync(CancellationToken cancellationToken)
    {
        gamePlayerInfoArray = gameFlowParameter.gamePlayerInfoArray;
        gamePlayerControllerArray = new GamePlayerController[gamePlayerInfoArray.Length];
        for (int i = 0; i < gamePlayerControllerArray.Length; i++)
        {
            GamePlayerController gamePlayerController = Instantiate(gamePlayerPrefab).GetComponent<GamePlayerController>();
            gamePlayerControllerArray[i] = gamePlayerController;

            GamePlayerInfo gamePlayerInfo = gamePlayerInfoArray[i];

            GamePlayerView gamePlayerView = Instantiate(gamePlayerViewPrefab, uiViewParent).GetComponent<GamePlayerView>();

            SetPlayerPosition(gamePlayerController.transform, i, gamePlayerControllerArray.Length);

            gamePlayerController.SetupFromManager(this, gamePlayerInfo, gamePlayerView);
        }

        m_audioSourceBgm.clip = audioClipBattleBgm;
        m_audioSourceBgm.Play();

        UniTask t = LoopPlayerUpdateAsync(cancellationToken);

        List<UniTask> prepareTaskList = new List<UniTask>();

        for (int i = 0; i < gamePlayerControllerArray.Length; i++)
        {
            prepareTaskList.Add(gamePlayerControllerArray[i].PrepareFromManagerAsync(cancellationToken));
        }

        await UniTask.WhenAll(prepareTaskList);

        for (int i = 0; i < gamePlayerControllerArray.Length; i++)
        {
            gamePlayerControllerArray[i].gamePlayerView.cover.Deactivate();
        }

        audioSourceSe.PlayOneShot(audioClipCountSe);
        cover.Activate("3");
        await UniTask.Delay(1000, cancellationToken: cancellationToken);
        audioSourceSe.PlayOneShot(audioClipCountSe);
        cover.Activate("2");
        await UniTask.Delay(1000, cancellationToken: cancellationToken);
        audioSourceSe.PlayOneShot(audioClipCountSe);
        cover.Activate("1");
        await UniTask.Delay(1000, cancellationToken: cancellationToken);
        audioSourceSe.PlayOneShot(audioClipStartSe);
        cover.Activate("Start!!");
        for (int i = 0; i < gamePlayerControllerArray.Length; i++)
        {
            gamePlayerControllerArray[i].ActivateFromManager();
        }
        await UniTask.Delay(1000, cancellationToken: cancellationToken);
        cover.Deactivate();
    }

    public void RequestFinish()
    {
        UniTask t = FinishProcessAsync(cts.Token);
    }

    private async UniTask FinishProcessAsync(CancellationToken cancellationToken)
    {



        int winnerPlayer = 0;
        for (int i = 0; i < gamePlayerControllerArray.Length; i++)
        {
            gamePlayerControllerArray[i].DeactivateFromManager();
            if(gamePlayerControllerArray[i].hitPoint > gamePlayerControllerArray[winnerPlayer].hitPoint)
            {
                winnerPlayer = i;
            }
        }

        audioSourceBgm.Stop();

        audioSourceSe.PlayOneShot(audioClipFinishSe);

        cover.Activate("Finish!!");

        await UniTask.Delay(3000, cancellationToken: cancellationToken);

        audioSourceBgm.clip = audioClipResultBgm;
        audioSourceBgm.Play();

        cover.Deactivate();

        for (int i = 0; i < gamePlayerControllerArray.Length; i++)
        {
            if (i == winnerPlayer)
            {
                gamePlayerControllerArray[i].gamePlayerView.cover.Activate("WIN", 128, Color.yellow);
            }
            else
            {
                gamePlayerControllerArray[i].gamePlayerView.cover.Activate("LOSE", 128, Color.blue);
            }
        }

        restartButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);

    }


    private async UniTask LoopPlayerUpdateAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            for (int i = 0; i < gamePlayerControllerArray.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                await gamePlayerControllerArray[i].UpdateFromManagerAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
                await UniTask.WaitForEndOfFrame(this);
            }
        }
    }


    private void SetPlayerPosition(Transform playerTransform, int playerIndex, int allPlayerCount)
    {

        if(playerIndex == 0)
        {
            playerTransform.localPosition = new Vector3(0, 0, -9.5f);
            playerTransform.localEulerAngles = Vector3.zero;
        }
        else
        {
            playerTransform.localPosition = new Vector3(0, 0, 9.5f);
            playerTransform.localEulerAngles = Vector3.up * 180;
        }

    }
    
    private void RestartGame()
    {
        SceneManager.LoadScene(2);
    }

    private void ExitGame()
    {
        SceneManager.LoadScene(1);
    }

}
