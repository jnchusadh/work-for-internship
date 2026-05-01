using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class GenerateGUID : MonoBehaviour
{
    [SerializeField]
    private string gUID = "";


    public string GUID
    {
        get
        {
            return gUID;
        }
        set
        {
            gUID = value;
        }
    }
    private void Awake()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if(gUID == "")
            {
                gUID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
