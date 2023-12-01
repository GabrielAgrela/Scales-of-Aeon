using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class TextToSpeech : MonoBehaviour
{
    private string subscriptionKey = "";
    private string region = "westeurope";
    private string textToSynthesize = "Hello World";
    private string endpoint = "https://westeurope.tts.speech.microsoft.com/cognitiveservices/v1";

    public AudioSource audioSource;

    public Dilemma dilemmaScript;

    public void startTTSCoroutine()
    {
        StartCoroutine(tts());
    }

    public IEnumerator tts()
    {
        print("pqp2");
        yield return new WaitForSeconds(1);
        textToSynthesize = dilemmaScript.dilemma + "... Your options are: " + dilemmaScript.option1 +" or "+ dilemmaScript.option2;
        string accessToken;
        string tokenUrl = "https://" + region + ".api.cognitive.microsoft.com/sts/v1.0/issueToken";

        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm("https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issueToken", ""))
        {
        webRequest.SetRequestHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
        yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error fetching token: " + webRequest.error);
                yield break;
            }

            accessToken = webRequest.downloadHandler.text;
        }
        print("pqp3");
        using (UnityWebRequest webRequest = new UnityWebRequest(endpoint, "POST"))
        {
            string requestBody = $"<speak version='1.0' xmlns:mstts='http://www.w3.org/2001/mstts' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='Male' name='en-US-DavisNeural'> <mstts:express-as style='unfriendly'>{textToSynthesize}</mstts:express-as></voice></speak>";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerAudioClip("", AudioType.MPEG);
            webRequest.SetRequestHeader("Content-Type", "application/ssml+xml");
            webRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
            webRequest.SetRequestHeader("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
            
            webRequest.SetRequestHeader("User-Agent", "YourAppName");

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error in Text to Speech: " + webRequest.error + accessToken);
                audioSource.clip = null;
            }
            else
            {
                print("here3");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
                audioSource.clip = clip;
            }
        }
    }

    private AudioClip ConvertToAudioClip(byte[] wavFile)
{
    // WAV file header offsets and lengths
    int headerSize = 44; // default WAV header size

    // Check if the header is the correct size
    if (wavFile.Length < headerSize)
    {
        throw new Exception("Invalid WAV file (header too small)");
    }

    // Extract data from header
    int sampleRate = BitConverter.ToInt32(wavFile, 24);
    int bitsPerSample = BitConverter.ToInt16(wavFile, 34);
    int audioDataLength = BitConverter.ToInt32(wavFile, 40);

    // Determine the number of audio channels
    int channelCount = BitConverter.ToInt16(wavFile, 22);

    // Check if the bit depth is supported
    if (bitsPerSample != 16)
    {
        throw new Exception("Unsupported bit depth: " + bitsPerSample);
    }

    // Calculate the number of samples in the data
    int sampleCount = audioDataLength / (bitsPerSample / 8);

    // Create a new float array for Unity's AudioClip
    float[] audioData = new float[sampleCount];

    // Convert the raw audio data to Unity's audio data format (float array)
    int wavOffset = headerSize;
    int floatArrayOffset = 0;

    while (wavOffset < wavFile.Length && floatArrayOffset < sampleCount)
    {
        // Extract the 16-bit sample as a float
        short sample = BitConverter.ToInt16(wavFile, wavOffset);
        audioData[floatArrayOffset] = sample / 32768f;

        wavOffset += 2; // move the offset by 2 bytes for the next sample
        floatArrayOffset++;
    }

    // Create an AudioClip with the converted data
    AudioClip audioClip = AudioClip.Create("GeneratedAudioClip", sampleCount, channelCount, sampleRate, false);
    audioClip.SetData(audioData, 0);

    return audioClip;
}

}
