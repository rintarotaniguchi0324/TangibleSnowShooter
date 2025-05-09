using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp.Util;
using OpenCvSharp.Aruco;
using Cysharp.Threading.Tasks;

internal class GamePlayerController : MonoBehaviour
{

    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public GamePlayerView gamePlayerView;

    [SerializeField] private MarkerGroupInfo boardInfo;
    [SerializeField] private MarkerGroupInfo shooterInfo;
    [SerializeField] private MarkerGroupInfo shieldInfo;

    [SerializeField] private TangibleObject shooterPrefab;
    [SerializeField] private TangibleObject shieldPrefab;
    
    [SerializeField] private AudioClip audioClipPrepareCompleted;

    private TangibleObject shooterObject;
    private TangibleObject shieldObject;

    private bool shooterIsPrepared = false;
    private bool shieldIsPrepared = false;
    //private Transform shooterTransform;
    //private Transform shieldTransform;

    private GamePlayerInfo gamePlayerInfo;

    private WebCamTexture webcamTexture;
    private bool webcamTextureIsActive { get
        {
            if (webcamTexture)
            {
                if (webcamTexture.width > 16 && webcamTexture.height > 16)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        } 
    }

    Texture2D outputT2;

    //Game param
    [SerializeField] private int maxHitPoint = 10000;
    internal int hitPoint;
    private bool gameIsActive = false;

    //Keep values
    private Vector3 shooterPosition;
    private Vector3 shooterEuler;
    bool shooterUpdateFlag = false;

    private Vector3 shieldPosition;
    private Vector3 shieldEuler;

    bool shieldUpdateFlag = false;

    //Mats
    Mat webcamMat;

    private Mat calibrationMat = null;

    public bool calibrationIsCompleted { get { return calibrationMat != null; } }

    //Debug
    private GameObject[] debugObject = new GameObject[50];
    [SerializeField] private RawImage debugRawImage;
    private Texture2D debugTexture2D;

    private void OnDisable()
    {
        if (webcamTexture)
        {
            webcamTexture.Stop();
            
        }

        if (gamePlayerView)
        {
            Destroy(gamePlayerView.gameObject);
        }
    }

    private void Update()
    {
        if (shooterUpdateFlag)
        {
            shooterObject.transform.localPosition = shooterPosition * 100;
            shooterObject.transform.localEulerAngles = shooterEuler;
            shooterUpdateFlag = false;
        }

        if (shieldUpdateFlag)
        {
            shieldObject.transform.localPosition = shieldPosition * 100;
            shieldObject.transform.localEulerAngles = shieldEuler;
            shieldUpdateFlag = false;
        }
    }

    public async UniTask UpdateFromManagerAsync(CancellationToken cancellationToken)
    {
        webcamMat = OpenCvSharp.Unity.TextureToMat(webcamTexture);

        CvAruco.DetectMarkers(webcamMat, gameManager.arucoDictionary,
            out Point2f[][] corners, out int[] ids, DetectorParameters.Create(), out Point2f[][] rejectedImgPoints);

        CvAruco.DrawDetectedMarkers(webcamMat, corners, ids);

        outputT2.Reinitialize(webcamMat.Cols, webcamMat.Rows);

        OpenCvSharp.Unity.MatToTexture(webcamMat, outputT2);

        await UniTask.SwitchToThreadPool();

        Point2f[] rawCenters = new Point2f[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            rawCenters[i] = MarkerCalculator.GetMarkerCenter(corners[i]);
        }



        if (calibrationMat == null)
        {
            calibrationMat = GetBoardPerspectiveTransform(corners, ids);
            if (calibrationMat != null)
            {
                Debug.Log($"[{calibrationMat.Get<float>(0, 0)},{calibrationMat.Get<float>(0, 1)},{calibrationMat.Get<float>(0, 2)}]");
            }
        }

        if (calibrationMat != null)
        {
            if (rawCenters.Length > 0)
            {
                //Perspected transformed center.
                Point2f[] ptCenters = Cv2.PerspectiveTransform(rawCenters, calibrationMat);

                MarkerGroupPose shooterPose = shooterInfo.GetGroupPose(ptCenters, ids);
                if (shooterPose != null)
                {
                    shooterPosition = new Vector3(-shooterPose.position.X, 0, shooterPose.position.Y);
                    shooterEuler = -Vector3.up * shooterPose.rotation * Mathf.Rad2Deg;
                    shooterIsPrepared = true;
                    shooterUpdateFlag = true;
                }

                MarkerGroupPose shieldPose = shieldInfo.GetGroupPose(ptCenters, ids);
                if (shieldPose != null)
                {
                    shieldPosition = new Vector3(-shieldPose.position.X, 0, shieldPose.position.Y);
                    shieldEuler = -Vector3.up * shieldPose.rotation * Mathf.Rad2Deg;
                    shieldIsPrepared = true;
                    shieldUpdateFlag = true;
                }

            }

        }

        await UniTask.SwitchToMainThread();
        
    }


    public void SetupFromManager(GameManager gameManager, GamePlayerInfo gamePlayerInfo, GamePlayerView gamePlayerView)
    {
        this.gameManager = gameManager;
        this.gamePlayerInfo = gamePlayerInfo;
        this.gamePlayerView = gamePlayerView;

        hitPoint = maxHitPoint;

        webcamTexture = new WebCamTexture(gamePlayerInfo.cameraDeviceName, 960, 540, 32);

        webcamTexture.Play();

        outputT2 = new Texture2D(960, 540, TextureFormat.RGBA32, false);

        if (gamePlayerView)
        {
            if (gamePlayerView.webcamRawImage)
            {
                gamePlayerView.webcamRawImage.texture = outputT2;
            }
        }

        shooterObject = Instantiate(shooterPrefab);
        shooterObject.SetupFromPlayerController(this);
        shooterObject.transform.SetParent(transform);

        shieldObject = Instantiate(shieldPrefab);
        shieldObject.SetupFromPlayerController(this);
        shieldObject.transform.SetParent(transform);

        gamePlayerView.gameCameraRawImage.texture = shooterObject.instanceRenderTexture;

    }

    public async UniTask PrepareFromManagerAsync(CancellationToken cancellationToken)
    {
        gamePlayerView.cover.Activate("ボードをかくにんちゅう", 64);

        await UniTask.WaitUntil(() => calibrationIsCompleted, cancellationToken: cancellationToken);

        gamePlayerView.cover.Activate("シューターとシールドをかくにんちゅう", 64);

        await UniTask.WaitUntil(() => shooterIsPrepared, cancellationToken: cancellationToken);
        await UniTask.WaitUntil(() => shieldIsPrepared, cancellationToken: cancellationToken);

        gameManager.audioSourceSe.PlayOneShot(audioClipPrepareCompleted);
        gamePlayerView.cover.Activate("OK", 128, Color.green);

        await UniTask.Delay(1200, cancellationToken: cancellationToken);

    }

    internal void ActivateFromManager()
    {
        shooterObject.Activate();
        shieldObject.Activate();
        gameIsActive = true;
    }

    internal void DeactivateFromManager()
    {
        shooterObject.Deactivate();
        shieldObject.Deactivate();
        gameIsActive = false;
    }

    public void OnGetDamage(int damage)
    {
        if (!gameIsActive) return;
        hitPoint -= damage;
        if(hitPoint <= 0)
        {
            hitPoint = 0;
        }
        gamePlayerView.DisplayHitPoint((float)hitPoint / (float)maxHitPoint);
        if (hitPoint <= 0)
        {
            gameManager.RequestFinish();
        }
    }


    private Mat GetBoardPerspectiveTransform(Point2f[][] corners, int[] ids)
    {
        int[] boardIds = new int[4]
        {
            boardInfo.setMarkers[0].id,
            boardInfo.setMarkers[1].id,
            boardInfo.setMarkers[2].id,
            boardInfo.setMarkers[3].id,
        };
        Point2f[] dst = new Point2f[4]
        {
            boardInfo.setMarkers[0].relatedPosition,
            boardInfo.setMarkers[1].relatedPosition,
            boardInfo.setMarkers[2].relatedPosition,
            boardInfo.setMarkers[3].relatedPosition,
        };

        bool[] inputSigns = new bool[4]
        {
            false,
            false,
            false,
            false,
        };

        Point2f[] src = new Point2f[4];
        for (int i = 0; i < ids.Length; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                if(ids[i] == boardIds[j])
                {
                    src[j] = MarkerCalculator.GetMarkerCenter(corners[i]);
                    inputSigns[j] = true;
                }
            }
        }

        if(inputSigns[0] && inputSigns[1] && inputSigns[2] && inputSigns[3])
        {
            return Cv2.GetPerspectiveTransform(src, dst);
        }
        else
        {
            return null;
        }
        
    }




}
