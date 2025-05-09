using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayerView : MonoBehaviour
{
    [SerializeField] private RawImage m_webcamRawImage;
    public RawImage webcamRawImage { get { return m_webcamRawImage; } }

    [SerializeField] private RawImage m_gameCameraRawImage;
    public RawImage gameCameraRawImage { get { return m_gameCameraRawImage; } }

    [SerializeField] private WaitingCoverController m_cover;
    public WaitingCoverController cover { get { return m_cover; } }

    [SerializeField] private RawImage hitPointBarRawImage;
    [SerializeField] private RectTransform hitPointBarRectTransform;

    private float hitPointBarMaxSize;
    private void OnEnable()
    {
        hitPointBarMaxSize = hitPointBarRectTransform.sizeDelta.x;
    }

    public void DisplayHitPoint(float hitPointRate)
    {
        hitPointBarRectTransform.sizeDelta = new Vector2(hitPointBarMaxSize * hitPointRate, hitPointBarRectTransform.sizeDelta.y);
        if(hitPointRate > 0.5f)
        {
            hitPointBarRawImage.color = Color.green;
        }
        else if(hitPointRate > 0.2f)
        {
            hitPointBarRawImage.color = Color.yellow;
        }
        else
        {
            hitPointBarRawImage.color = Color.red;
        }

    }


}
