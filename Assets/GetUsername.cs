using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetUsername : MonoBehaviour
{
    public void GetUsernameInput()
    {
        
        GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username = GameObject.Find("UsernameInput").GetComponent<TMPro.TMP_InputField>().text;
        GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username = System.Text.RegularExpressions.Regex.Replace(GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username, "[^a-zA-Z0-9]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);
        GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username = GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username.Substring(0, Mathf.Min(GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username.Length, 16));
        CloudsTransition();
    }
    void CloudsTransition()
    {
        GameObject.Find("CloudsTransition").GetComponent<Animator>().SetBool("Transition", true);
    }
}
