using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class WaitingCoverController : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI mainTmp;


    private string text;



    public void Activate(string text, float? fontSize = null, Color? color = null)
    {
        mainTmp.text = text;
        if(color is Color checkedColor)
        {
            mainTmp.color = checkedColor;
        }

        if(fontSize is float checkedFontSize)
        {
            mainTmp.fontSize = checkedFontSize;
        }

        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    

}
