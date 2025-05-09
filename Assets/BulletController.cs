using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BulletController : MonoBehaviour
{
    
    [SerializeField] private float speed = 60f;
    [SerializeField] private float timeToDestroy = 5;
    [SerializeField] private Collider[] myColliders = new Collider[0];

    [SerializeField] private int power = 100;

    [SerializeField] private ParticleSystem breakParticlePrefab;
    [SerializeField] private AudioClip audioClipHit;

    private TangibleObject tangibleObject;

    private float countToDestroy = 0;

    private Collider[] ignoreColliders = new Collider[0];

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.4f, 31);
        if(colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider hitCollider = colliders[i];
                bool ignore = false;
                for(int j = 0; j < ignoreColliders.Length; j++)
                {
                    Collider ignoreCollider = ignoreColliders[j];
                    if(hitCollider == ignoreCollider)
                    {
                        ignore = true;
                        break;
                    }
                }
                if (!ignore)
                {
                    TangibleObject hitTangibleObject = colliders[i].GetComponentInParent<TangibleObject>();
                    if(hitTangibleObject)
                    {
                        tangibleObject.playerController.gameManager.audioSourceSe.PlayOneShot(audioClipHit);
                        hitTangibleObject.OnHit(power);

                        ParticleSystem breakParticle = Instantiate(breakParticlePrefab);
                        breakParticle.transform.position = transform.position;

                        Destroy(gameObject);
                        return;
                    }
                }

            }

            
        }


        countToDestroy += Time.deltaTime;
        if(countToDestroy >= timeToDestroy)
        {
            
            Destroy(gameObject);
        }
    }

    public void SetTangibleObject(TangibleObject tangibleObject)
    {
        this.tangibleObject = tangibleObject;
    }

    public void SetIgnoreColliders(Collider[] ignoreColliders)
    {
        this.ignoreColliders = new Collider[ignoreColliders.Length + myColliders.Length];
        
        for(int i = 0; i < myColliders.Length; i++)
        {
            this.ignoreColliders[i] = myColliders[i];
        }

        for(int i = 0; i < ignoreColliders.Length; i++)
        {
            this.ignoreColliders[i + myColliders.Length] = ignoreColliders[i];
        }

    }

}
