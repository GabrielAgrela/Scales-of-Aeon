using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


// we should not transfer data between classes like this but i dont have time 
public class LoginSceneData : MonoBehaviour
{
    public static LoginSceneData Instance { get; private set; }

    public string username;
    public GameObject userfield;

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    
}