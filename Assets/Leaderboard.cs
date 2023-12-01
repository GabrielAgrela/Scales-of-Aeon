using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public GameObject database;
    public List<PlayerInfo> topPlayers = new List<PlayerInfo>();
    // Start is called before the first frame update
    void Start()
    {
        
        DataBase dataBaseComponent = database.GetComponent<DataBase>();
        topPlayers.Clear();
        dataBaseComponent.OnLoginSuccessEvent += OnLoginSuccess;
        dataBaseComponent.LogInAdmin();
    }

    private void OnLoginSuccess()
    {
        database.GetComponent<DataBase>().GetTopScoringPlayers(12);
    }

    public class PlayerInfo
    {
        public string Username { get; set; }
        public int Score { get; set; }
        public string Catastrophe { get; set; }
    }
}
