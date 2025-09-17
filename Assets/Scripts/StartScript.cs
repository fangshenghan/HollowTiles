using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{

    internal static StartScript instance;

    public GameObject leaderboardObject;
    public TMP_InputField leaderboardField, playerNameField;

    public void Awake()
    {
        instance = this;

        LeaderboardManager.InitializePlayerInfo();
        StartCoroutine(LeaderboardManager.FetchLeaderboardCo());
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    internal void DisplayLeaderboard()
    {
        leaderboardObject.SetActive(true);

        string leaderboardText = "";

        for (int i = 0; i < LeaderboardManager.leaderboardData.Count; i++)
        {
            var player = LeaderboardManager.leaderboardData[i];
            leaderboardText += $"{i + 1}. {player.playerName} - {GameManagerScript.GetFormattedGameTime(player.gameTime)}\n";
        }

        leaderboardField.text = leaderboardText;
        playerNameField.text = LeaderboardManager.playerName;
    }

    public void OnPlayerNameEndEdit()
    {
        string newName = playerNameField.text.Trim();
        if (!string.IsNullOrEmpty(newName) && newName != LeaderboardManager.playerName)
        {
            LeaderboardManager.playerName = newName;
            PlayerPrefs.SetString("PlayerName", newName);
            PlayerPrefs.Save();
        }
    }


}
