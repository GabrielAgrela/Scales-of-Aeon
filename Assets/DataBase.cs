using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    public string username = "testUser";
    public string UI = "0000";

    public delegate void OnLoginSuccessDelegate();
    public event OnLoginSuccessDelegate OnLoginSuccessEvent;
    public Leaderboard leaderboard;

    

    void Start()
    {
        username=GameObject.Find("PlayerData").GetComponent<LoginSceneData>().username;
        // Initialize PlayFab with your title ID
        PlayFabSettings.staticSettings.TitleId = "";
        // You should also ensure the player is logged in at this point
        UI = UnityEngine.Random.Range(0, 9) +""+ UnityEngine.Random.Range(0, 9) +""+ UnityEngine.Random.Range(0, 9) +""+ UnityEngine.Random.Range(0, 9);
        CreateUserIfNotExists();
    }
    

    public IEnumerator InitializeAccount()
    {
        yield return new WaitForSeconds(5);
        UI = UnityEngine.Random.Range(0, 9) +""+ UnityEngine.Random.Range(0, 9) +""+ UnityEngine.Random.Range(0, 9) +""+ UnityEngine.Random.Range(0, 9);
        CreateUserIfNotExists();
    }

    public void LogInAdmin()
    {
        var request = new LoginWithCustomIDRequest { CustomId = "admin", CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnAdminLoginSuccess,OnAdminLoginFailure);
    }
    private void OnAdminLoginFailure(PlayFabError error)
    {
        Debug.Log("not Logged in player with PlayFabId: " + error);
        OnLoginSuccessEvent?.Invoke();
        // Now that the player is logged in, you can get and set user data
    }

    private void OnAdminLoginSuccess(LoginResult result)
    {
        leaderboard = GameObject.Find("LeaderBoard").GetComponent<Leaderboard>();
        Debug.Log("Logged in player with PlayFabId: " + result.PlayFabId);
        OnLoginSuccessEvent?.Invoke();
        // Now that the player is logged in, you can get and set user data
    }


    public void CreateUserIfNotExists()
    {
        var request = new LoginWithCustomIDRequest { CustomId = username+UI, CreateAccount = false };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnCreateUser);
    }

    private void OnCreateUser(PlayFabError error)
    {
        if (error.Error == PlayFabErrorCode.AccountNotFound)
        {
            var createRequest = new RegisterPlayFabUserRequest 
            { 
                Username = username + UI, 
                DisplayName = username + UI,
                Email = username+UI+"@example.com", 
                Password = ""
            };
            PlayFabClientAPI.RegisterPlayFabUser(createRequest, OnRegisterSuccess, OnRegisterFailure);
        }
        else
        {
            Debug.LogError("Error logging in player: " + error.GenerateErrorReport());
        }
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        SaveNewHighScore(0);
        Debug.Log("New user created with PlayFabId: " + result.PlayFabId);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError("Error creating new user: " + error.GenerateErrorReport());
        if (error.Error == PlayFabErrorCode.EmailAddressNotAvailable)
        {
            StartCoroutine(InitializeAccount());
            
        }
    }


    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Logged in player with PlayFabId: " + result.PlayFabId);
        // Now that the player is logged in, you can get and set user data
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Error logging in player: " + error.GenerateErrorReport());
    }

    public Task<bool> IsTimeMoreThanXSeconds(long secs)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("timestamp"))
            {
                long storedTimestamp = long.Parse(result.Data["timestamp"].Value);
                long currentServerTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                bool isMoreThanXSeconds = (currentServerTimestamp - storedTimestamp) > secs;
                tcs.SetResult(isMoreThanXSeconds);
            }
            else
            {
                tcs.SetResult(false); // No timestamp found or unable to parse the timestamp
            }
            
        }, error =>
        {
            Debug.LogError("Error retrieving user data: " + error.GenerateErrorReport());
            tcs.SetResult(false);
        });

        return tcs.Task;
    }

    
    public async void GetTopScoringPlayers(int numPlayers)
    {
        var leaderboardRequest = new GetLeaderboardRequest
        {
            StatisticName = "score",
            StartPosition = 0,
            MaxResultsCount = numPlayers
        };

        PlayFabClientAPI.GetLeaderboard(leaderboardRequest, OnGetLeaderboardSuccess, OnGetLeaderboardFailure);
    }
    private async void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        var tasks = new List<Task>();
        foreach (var player in result.Leaderboard)
        {
            tasks.Add(GetUserCatastrophe(player.PlayFabId, player.DisplayName, player.StatValue));
        }

        await Task.WhenAll(tasks);

        // sort top players by score
        leaderboard.topPlayers.Sort((x, y) => y.Score.CompareTo(x.Score));

        GameObject entries = GameObject.Find("Entries");
        GameObject.Find("Loading").SetActive(false);
        int i = 0;
        // print top players
        foreach (var player in leaderboard.topPlayers)
        {
            try
            {
                // if player.Username is bigger than 2 and doesnt contain the word "test"
                if (player.Username.Length > 2 && !player.Username.Contains("test"))
                {
                    entries.transform.GetChild(i).transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = player.Username.Substring(0, player.Username.Length - 4) + "#" + player.Username.Substring(player.Username.Length - 4);
                    entries.transform.GetChild(i).transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = player.Score.ToString();
                    entries.transform.GetChild(i).transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = player.Catastrophe;
                }
                else
                {
                    i--;
                }
                    
            }
            catch (System.Exception)
            {
                i--;
                print("no numbers in this username");
            }
                


            
            i++;
        }
    }

    private void OnGetLeaderboardFailure(PlayFabError error)
    {
        Debug.LogError("Error retrieving leaderboard: " + error.GenerateErrorReport());
    }

    private async Task GetUserCatastrophe(string playFabId, string username, int score)
    {
        var userDataRequest = new GetUserDataRequest { PlayFabId = playFabId };

        var tcs = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetUserData(userDataRequest, (userDataResult) =>
        {
            var playerInfo = new Leaderboard.PlayerInfo { Username = username, Score = score };

            if (userDataResult.Data != null && userDataResult.Data.TryGetValue("catastrophe", out var userDataRecord))
            {
                playerInfo.Catastrophe = userDataRecord.Value;
            }
            else
            {
                playerInfo.Catastrophe = "Unknown";
            }

            leaderboard.topPlayers.Add(playerInfo);
            tcs.SetResult(true);

        }, (error) =>
        {
            Debug.LogError("Error retrieving user data: " + error.GenerateErrorReport());
            tcs.SetResult(false);
        });

        await tcs.Task;
    }


    public void SaveNewHighScore(int score, string catastrophe="-")
    {
        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"score", score.ToString()},
                {"timestamp", ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString()},
                {"catastrophe", catastrophe}
            },
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(updateRequest, result =>
        {
            Debug.Log("High score and timestamp saved successfully.");
        }, error =>
        {
            Debug.LogError("Error saving high score and timestamp: " + error.GenerateErrorReport());
        });

        var updateLeaderboardRequest = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "score", Value = score }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(updateLeaderboardRequest, leaderboardResult =>
        {
            Debug.Log("High score saved successfully on the leaderboard.");
        }, error =>
        {
            Debug.LogError("Error saving high score on the leaderboard: " + error.GenerateErrorReport());
        });
        
    }

    
}