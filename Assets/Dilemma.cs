using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using Random = UnityEngine.Random;
using static Category;
using UnityEngine.UI;

public class Dilemma : MonoBehaviour
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

    private List<string> topics;
    public GameMaster gameMaster;
    public DataBase dataBase;

    [SerializeField] private GameObject dilemmaGameObject;
    [SerializeField] private GameObject option1Button;
    [SerializeField] private GameObject option2Button;

    [SerializeField] private GameBalanceManager gameBalanceManager;

    public GameObject TTS;

    private Queue<DilemmaData> preloadedDilemmas = new Queue<DilemmaData>();
    public GameObject backgroundImageCard;

    private void Awake()
    {
        topics = System.Enum.GetNames(typeof(CategoryType)).ToList();
        foreach (var topic in topics)
        {
            Debug.Log(topic);
        }
        gameBalanceManager = GameObject.Find("GameMaster")?.GetComponent<GameBalanceManager>();
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

        var dilemmaCategories = gameBalanceManager.SelectDilemmaCategories();

        // Assign one favor and one unfavor for each PromptOption
        promptOption1.AddFavor(dilemmaCategories.Item1.name);
        promptOption1.AddUnfavor(dilemmaCategories.Item2.name);
        promptOption2.AddFavor(dilemmaCategories.Item2.name); // Ensure promptOption2 does not favor the same topic as promptOption1
        promptOption2.AddUnfavor(dilemmaCategories.Item1.name); // Ensure promptOption2 does not unfavor the same topic as favor of promptOption1

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

    public async void CreateDilemma(int chosenOption = 10)
    {
        GameObject playerData = GameObject.Find("PlayerData");
        
        if (playerData != null)
        {
            AudioSource audioSource = playerData.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }
        if (chosenOption != 10)
        {
            if (!await dataBase.IsTimeMoreThanXSeconds(10))
            {
                return;
            }
            if (chosenOption == 1)
            {
                Debug.Log(promptOption1);
                Debug.Log(string.Join("", promptOption1.favor));
                Debug.Log(string.Join("", promptOption1.unfavor));
                gameMaster.IncrementPointsPerCategoryPerDecision(string.Join("", promptOption1.favor));
                gameMaster.DecrementPointsPerCategoryPerDecision(string.Join("", promptOption1.unfavor));
            }
            else if (chosenOption == 2)
            {
                Debug.Log(string.Join("", promptOption1.favor));
                Debug.Log(string.Join("", promptOption1.unfavor));
                gameMaster.IncrementPointsPerCategoryPerDecision(string.Join("", promptOption2.favor));
                gameMaster.DecrementPointsPerCategoryPerDecision(string.Join("", promptOption2.unfavor));
            }
        }

        CloseCardUI();
        InitializePromptOptions();
        UpdateMessages(chosenOption);
        string jsonRequest = CreateJsonRequest();
        CoroutineHelper.Instance.StartHelperCoroutine(SendWebRequest(jsonRequest));
        gameMaster.IncrementScore();
    }

    public void CloseCardUI()
    {
        GameObject.FindGameObjectWithTag("Card")?.SetActive(false);
        Time.timeScale = 1; // Resume game time
    }

    private void UpdateMessages(int chosenOption)
    {
        optionChosen = chosenOption == 1 ? option1 : option2;
        messages[1].content = $"{messages[1].content}. Event chosen: {optionChosen}";
        messages[2].content = CreateMessageContent();
    }

    private string CreateMessageContent()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("It is of utmost importance that you continue the narrative based on your last dilemma and the event chosen by creating a complex and engaging dilemma between the factions based on an new event for the Aeonarch to contemplate, encapsulated in around ")
        .Append(maxDilemmaWords)
        .Append(" words. The dilemma should allude to the intricate balance of power, belief, and the populace's well-being, ")
        .Append("embedded within the fabric of Scaleden's society. ")
        .Append("The narrative should be rich, without revealing the consequences.")
        .Append("Following the dilemma, present two events, each in around ")
        .Append(maxOptionWords)
        .Append(" words, representing divergent choices: ")
        .Append("option1 should be an event that favours faction '")
        .Append(string.Join(" and ", promptOption1.favor))
        .Append("' while it unfavours faction '")
        .Append(string.Join(" and ", promptOption1.unfavor))
        .Append("'. option2 should be an event that favours faction '")
        .Append(string.Join(" and ", promptOption2.favor))
        .Append("' while it unfavours faction '")
        .Append(string.Join(" and ", promptOption2.unfavor))
        .Append("'. Make it clear it is a continuation to the narrative of the previous answer and the event chosen and the options have to be related to the dilemma. Provide JSON as {dilemma, option1 and option2}");

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

            temperature = 1,
            max_tokens = 2048,
            response_format = new { type = "json_object" },
            messages = messages.Select(m => new { role = m.role, content = m.content }).ToList(),
            /*functions,
            function_call = new { name = "return_dilemma_and_option" }*/
        };

        return JsonConvert.SerializeObject(requestObject, Formatting.Indented);
    }

    private IEnumerator SendWebRequest(string jsonRequest, bool isPreloading = false)
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
            if (isPreloading)
            {
                PreloadDilemma(request.downloadHandler.text);
            }
            else
            {
                ProcessResponse(request.downloadHandler.text);
            }
        }
    }

    private void PreloadDilemma(string jsonResponseText)
    {
        var jsonResponse = JSON.Parse(jsonResponseText);
        var functionCallResponse = JSON.Parse(jsonResponse["choices"][0]["message"]["content"]);

        DilemmaData newDilemma = new DilemmaData
        {
            DilemmaText = functionCallResponse["dilemma"],
            Option1Text = functionCallResponse["option1"],
            Option2Text = functionCallResponse["option2"]
        };

        preloadedDilemmas.Enqueue(newDilemma);

    }

    private void ProcessResponse(string jsonResponseText)
    {
        var jsonResponse = JSON.Parse(jsonResponseText);
        var functionCallResponse = JSON.Parse(jsonResponse["choices"][0]["message"]["content"]);
        messages[1].content = "dilemma: " + functionCallResponse["dilemma"];
        dilemma = functionCallResponse["dilemma"];
        option1 = functionCallResponse["option1"];
        option2 = functionCallResponse["option2"];
        UpdateUI(dilemma, option1, option2);
        print("pqp1");
        TTS.GetComponent<TextToSpeech>().startTTSCoroutine();
        //StartCoroutine(transform.parent.transform.parent.GetComponent<TextToSpeech>().TTS());
    }

    private void UpdateUI(string dilemmaText, string option1Text, string option2Text)
    {
        UpdateTextComponent(dilemmaGameObject, dilemmaText);
        UpdateTextComponent(option1Button, option1Text);
        UpdateTextComponent(option2Button, option2Text);

        UpdatePromptOptionText("PromptOption1", promptOption1);
        UpdatePromptOptionText("PromptOption2", promptOption2);

        SetImageFromCategory();
    }

    private void UpdateTextComponent(GameObject gameObject, string text)
    {
        var textComponent = gameObject.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }

    private void UpdatePromptOptionText(string gameObjectName, PromptOption promptOption)
    {
        var textComponent = GameObject.Find(gameObjectName)?.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"Favors: {string.Join(", ", promptOption.favor)}; Unfavors: {string.Join(", ", promptOption.unfavor)}";
        }
    }

    private void SetImageFromCategory()
    {
        var promptOption = Random.Range(0, 2) == 0 ? promptOption1 : promptOption2;
        Debug.Log($"{promptOption}.favors " + string.Join(", ", promptOption.favor));
        var sprites = GameObject.Find(string.Join(", ", promptOption.favor))?.GetComponent<Category>().backgroundImages;
        if (sprites == null) return;

        var randomSprite = sprites[Random.Range(0, sprites.Length)];
        backgroundImageCard.GetComponent<UnityEngine.UI.Image>().sprite = randomSprite;
    }

    public void PreloadDilemmas(int numberOfDilemmas)
    {
        for (int i = 0; i < numberOfDilemmas; i++)
        {
            InitializePromptOptions();
            UpdateMessages(10); // Use default value for initial loading
            string jsonRequest = CreateJsonRequest();
            CoroutineHelper.Instance.StartHelperCoroutine(SendWebRequest(jsonRequest, isPreloading: true));
        }
    }

    public void DisplayNextDilemma()
    {
        if (preloadedDilemmas.Count > 0)
        {
            DilemmaData nextDilemma = preloadedDilemmas.Dequeue();
            UpdateUI(nextDilemma.DilemmaText, nextDilemma.Option1Text, nextDilemma.Option2Text);
        }
        else
        {
            CreateDilemma();
        }
    }
    private class DilemmaData
    {
        public string DilemmaText;
        public string Option1Text;
        public string Option2Text;
        // Add any other relevant fields
    }
}
