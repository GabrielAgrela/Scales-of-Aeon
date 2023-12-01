using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public AudioClip windout;
    public void ChangeToScene(string sceneToChangeTo)
    {
        GameObject.Find("CloudsTransition").GetComponent<Animator>().SetBool("Transition", true);
        GameObject.Find("CloudsTransition").GetComponent<AudioSource>().clip = windout;
        GameObject.Find("CloudsTransition").GetComponent<AudioSource>().Play();
        StartCoroutine(ChangeSceneAfterTransition(sceneToChangeTo));
    }

    IEnumerator ChangeSceneAfterTransition(string sceneToChangeTo)
    {
        yield return new WaitForSeconds(2f);
        AudioSource playerDataAudio = GameObject.Find("PlayerData").GetComponent<AudioSource>();

        if (sceneToChangeTo != "Login")
        {
            StartCoroutine(LerpVolume(playerDataAudio, playerDataAudio.volume, 0.1f, 2f));
        }
        else
        {
            GameObject.Find("PlayerData").GetComponent<AudioSource>().UnPause();
            StartCoroutine(LerpVolume(playerDataAudio, playerDataAudio.volume, 1f, 2f));
        }

        yield return new WaitForSeconds(2f); // Wait for volume lerp to finish
        SceneManager.LoadScene(sceneToChangeTo);
    }

    IEnumerator LerpVolume(AudioSource audioSource, float startVolume, float endVolume, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = endVolume;
    }
}
