using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangibleObject : MonoBehaviour
{

    [SerializeField] private Camera m_playerCamera;
    public Camera playerCamera { get { return m_playerCamera; } }

    [SerializeField] private RenderTexture originalRenderTexture;
    private RenderTexture m_instanceRenderTexture;

    internal GamePlayerController playerController;

    [SerializeField] protected Collider[] myColliders;

    public RenderTexture instanceRenderTexture { get { return m_instanceRenderTexture; } }

    internal void SetupFromPlayerController(GamePlayerController playerController)
    {
        this.playerController = playerController;
        if (playerCamera)
        {
            m_instanceRenderTexture = Instantiate(originalRenderTexture);
            playerCamera.targetTexture = instanceRenderTexture;
        }
    }

    public virtual void Activate()
    {

    }
    public virtual void Deactivate()
    {

    }

    public virtual void OnHit(int power)
    {

    }

}
