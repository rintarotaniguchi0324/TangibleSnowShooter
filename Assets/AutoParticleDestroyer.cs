using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoParticleDestroyer : MonoBehaviour
{
    [SerializeField] private ParticleSystem referenceParticleSystem;

    private void OnValidate()
    {
        if (!referenceParticleSystem)
        {
            referenceParticleSystem = GetComponent<ParticleSystem>();
        }
    }

    void Update()
    {
        if (referenceParticleSystem)
        {
            if (!referenceParticleSystem.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
