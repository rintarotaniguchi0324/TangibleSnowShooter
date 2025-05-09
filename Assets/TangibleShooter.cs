using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TangibleShooter : TangibleObject
{

    [SerializeField] private BulletController bulletPrefab;
    [SerializeField] private float shootSpan = 0.3f;

    [SerializeField] private AudioClip audioClipShoot;

    private bool isActive = false;

    private float remainSpan = 0;

    public override void Activate()
    {
        isActive = true;
    }
    public override void Deactivate()
    {
        isActive = false;
    }

    public override void OnHit(int power)
    {
        playerController.OnGetDamage(power);
    }

    private void Update()
    {
        if (isActive)
        {
            if(remainSpan <= 0)
            {
                Shoot();
                remainSpan += shootSpan;
            }
            remainSpan -= Time.deltaTime;
        }
    }

    private void Shoot()
    {
        BulletController bullet = Instantiate(bulletPrefab);
        bullet.transform.position = transform.position + transform.forward + Vector3.up;
        bullet.transform.forward = transform.forward;
        bullet.SetTangibleObject(this);
        bullet.SetIgnoreColliders(myColliders);
        playerController.gameManager.audioSourceSe.PlayOneShot(audioClipShoot);
    }

    
}
