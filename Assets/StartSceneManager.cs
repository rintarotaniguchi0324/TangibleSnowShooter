using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class StartSceneManager : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.LoadScene(1);
        
        
    }
}
