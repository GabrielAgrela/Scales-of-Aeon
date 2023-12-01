using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

public class DilemmaFC : MonoBehaviour
{
    [SerializeField] private List<Message> messages;
    private string apiKey = "";
    private string url = "https://api.openai.com/v1/chat/completions";

    [SerializeField] private int maxDilemmaWords = 30;
    [SerializeField] private int maxOptionWords = 5;

    [TextArea(3, 10)] public string dilemma;
    [TextArea(3, 10)] public string option1;
    [TextArea(3, 10)] public string option2;
    [TextArea(3, 10)] public string optionChosen;

    private PromptOption lastPromptOption1;
    private PromptOption promptOption1;
    

    private PromptOption lastPromptOption2;
    private PromptOption promptOption2;
    
     private List<string> topics = new List<string> { "Religion", "Military", "People" };
     public GameMaster gameMaster;

    private void Start()
    {
        InitializeMessages();
        CreateDilemma();
    }

    private void InitializeMessages()
    {
        messages = new List<Message>
        {
            CreateMessage("system", "Your task is to create dilemmas that are intricate and non-obvious. #Lore Background: In the mythical city of Scaleden, Geckonians, intelligent gecko-like creatures, coexist with the cosmic Scales of Aeon, an ancient artifact dictating their reality. This city blends the tangible with the mystical, deeply valuing balance as governed by the Scales. Their leader, the Aeonarch, is both a political and spiritual figure, with each decision impacting the city's destiny."),
            CreateMessage("assistant", "Dilemma : first round, no dilemma yet"),
            CreateMessage("user", "")
        };
    }

    private Message CreateMessage(string role, string content)
    {
        return new Message { role = role, content = content };
    }

    private void InitializePromptOptions()
    {
        // Clear existing PromptOptions
        promptOption1 = new PromptOption();
        promptOption2 = new PromptOption();

        // Shuffle topics to randomize the assignment of favors and unfavors
        System.Random random = new System.Random();
        int n = topics.Count;
        while (n > 1) 
        {
            n--;
            int k = random.Next(n + 1);
            string value = topics[k];
            topics[k] = topics[n];
            topics[n] = value;
        }

        // Assign one favor and one unfavor for each PromptOption
        promptOption1.AddFavor(topics[0]);
        promptOption1.AddUnfavor(topics[1]);
        promptOption2.AddFavor(topics[1]); // Ensure promptOption2 does not favor the same topic as promptOption1
        promptOption2.AddUnfavor(topics[2]); // Ensure promptOption2 does not unfavor the same topic as favor of promptOption1

        // Remaining topic assignment, if any, can be done here with additional logic
    }

    private void AddRandomFavorsAndUnfavors(PromptOption promptOption, List<string> availableTopics, Dictionary<string, bool> usedTopics)
    {
        System.Random random = new System.Random();

        while (availableTopics.Count > 0)
        {
            int favorUnfavorChoice = random.Next(2); // Randomly choose between favor (0) and unfavor (1)

            // Choose a random topic that has not been used yet
            int topicIndex = random.Next(availableTopics.Count);
            string topic = availableTopics[topicIndex];
            
            // Add favor or unfavor based on random choice, and record the action
            if (favorUnfavorChoice == 0)
            {
                promptOption.AddFavor(topic);
                if (usedTopics != null) usedTopics[topic] = true; // Track favored topic
            }
            else
            {
                promptOption.AddUnfavor(topic);
                if (usedTopics != null) usedTopics[topic] = false; // Track unfavorably used topic
            }

            // Remove the used topic from the list
            availableTopics.RemoveAt(topicIndex);
        }
    }


    public void CreateDilemma(int chosenOption = 10)
    {
        if (chosenOption == 1)
        {
            gameMaster.IncrementPointsPerCategoryPerDecision(string.Join("", promptOption1.favor));
        }
        else if (chosenOption == 2)
        {
            gameMaster.IncrementPointsPerCategoryPerDecision(string.Join("", promptOption2.favor));
        }

        InitializePromptOptions();
        UpdateMessages(chosenOption);
        string jsonRequest = CreateJsonRequest();
        print(jsonRequest);
        StartCoroutine(SendWebRequest(jsonRequest));
    }

    private void UpdateMessages(int chosenOption)
    {
        optionChosen = chosenOption == 1 ? option1 : option2;
        messages[1].content = $"{messages[1].content}. Option chosen: {optionChosen}";
        messages[2].content = CreateMessageContent();
    }

    private string CreateMessageContent()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Continue the narrative based on your last answer and the option chosen by creating a complex and engaging dilemma between the factions Religion, Military and People based on an new event for the Aeonarch to contemplate, encapsulated in around ")
        .Append(maxDilemmaWords)
        .Append(" words. The dilemma should allude to the intricate balance of power, belief, and the populace's well-being, ")
        .Append("embedded within the fabric of Scaleden's society. ")
        .Append("The narrative should be rich, hinting at the consequences of different paths without directly revealing them.")
        .Append("Following the dilemma, present two separate options, each in around ")
        .Append(maxOptionWords)
        .Append(" words, representing divergent choices that resonate with the themes suggested by the PromptOptions. ")
        .Append("option1 should subtly reflect that it favours to '")
        .Append(string.Join(" and ", promptOption1.favor))
        .Append("' while subtly reflecting that it unfavours '")
        .Append(string.Join(" and ", promptOption1.unfavor))
        .Append("'. option2 should subtly reflect that it favours to '")
        .Append(string.Join(" and ", promptOption2.favor))
        .Append("' while subtly reflecting that it unfavours '")
        .Append(string.Join(" and ", promptOption2.unfavor))
        .Append("'. Make it clear it is a continuation to the narrative of the previous answer and option chosen. ");

        return sb.ToString();
    }





    private string CreateJsonRequest()
    {
        var functions = new[]
        {
            new
            {
                name = "return_dilemma_and_option",
                description = "Create dilemmas and options based on the provided narrative and constraints.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        dilemma = new
                        {
                            type = "string",
                            description = "The creative and short dilemma narrative."
                        },
                        option1 = new
                        {
                            type = "string",
                            description = "Creative option 1 narrative."
                        },
                        option2 = new
                        {
                            type = "string",
                            description = "Creative option 2 narrative."
                        }
                    },
                    required = new[] { "dilemma", "option1", "option2" }
                }
            }
        };

        var requestObject = new
        {
            model = "gpt-3.5-turbo-1106",
            temperature = 0.9,
            max_tokens = 2048,
            messages = messages.Select(m => new { role = m.role, content = m.content }).ToList(),
            functions,
            function_call = new { name = "return_dilemma_and_option" }
        };

        return JsonConvert.SerializeObject(requestObject, Formatting.Indented);
    }

    private IEnumerator SendWebRequest(string jsonRequest)
    {
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequest)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            
            ProcessResponse(request.downloadHandler.text);
        }
    }

    private void ProcessResponse(string jsonResponseText)
    {
        var jsonResponse = JSON.Parse(jsonResponseText);
        var functionCallResponse = JSON.Parse(jsonResponse["choices"][0]["message"]["function_call"]["arguments"]);
        messages[1].content = "dilemma: " + functionCallResponse["dilemma"];
        dilemma = functionCallResponse["dilemma"];
        option1 = functionCallResponse["option1"];
        option2 = functionCallResponse["option2"];
        print(functionCallResponse);
        UpdateUI(dilemma, option1, option2);
    }

    private void UpdateUI(string dilemmaText, string option1Text, string option2Text)
    {
        GameObject.Find("Dilemma").GetComponent<TextMeshProUGUI>().text = dilemmaText;
        GameObject.Find("Option1").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option1Text;
        GameObject.Find("Option2").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option2Text;
        GameObject.Find("PromptOption1").GetComponent<TextMeshProUGUI>().text = "Favors: " + string.Join(", ", promptOption1.favor) + "; Unfavors: " + string.Join(", ", promptOption1.unfavor);
        GameObject.Find("PromptOption2").GetComponent<TextMeshProUGUI>().text = "Favors: " + string.Join(", ", promptOption2.favor) + "; Unfavors: " + string.Join(", ", promptOption2.unfavor);
    }
}