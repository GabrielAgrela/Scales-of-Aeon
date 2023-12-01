using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class OpenAIAPI : MonoBehaviour
{
    private string apiURL = "https://api.openai.com/v1/engines/davinci-codex/completions";

    private string apiKey = "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";

    void Start()
    {
        // Ensure that the API key is set.
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key is not set. Please set the OPENAI_API_KEY environment variable.");
            return;
        }

        // Example JSON request
        string jsonRequest = "{\"prompt\": \"Hello, world!\", \"max_tokens\": 5}";
        StartCoroutine(SendRequestToOpenAI(jsonRequest));
    }

    IEnumerator SendRequestToOpenAI(string jsonRequest)
    {
        Debug.Log("Sending web request to OpenAI API...");

        var request = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        // Check the result of the request for both success and failure cases.
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error while sending request: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}
