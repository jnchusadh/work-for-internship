using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryTextBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTop1 = null;
    [SerializeField] private TextMeshProUGUI textTop2 = null;
    [SerializeField] private TextMeshProUGUI textTop3 = null;
    [SerializeField] private TextMeshProUGUI textBottom1 = null;
    [SerializeField] private TextMeshProUGUI textBottom2 = null;
    [SerializeField] private TextMeshProUGUI textBottom3 = null;

    public void SetText(string text1, string text2, string text3, string text4, string text5, string text6)
    {
        textTop1.text = text1;
        textTop2.text = text2;
        textTop3.text = text3;
        textBottom1.text = text4;
        textBottom2.text = text5;
        textBottom3.text = text6;
    }
}
