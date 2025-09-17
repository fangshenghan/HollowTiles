using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardManager
{

    internal static string playerName;

    internal static string playerID;

    internal static List<PlayerInfo> leaderboardData = new List<PlayerInfo>();

    internal static void InitializePlayerInfo()
    {
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            playerID = PlayerPrefs.GetString("PlayerID");
        }
        else
        {
            playerID = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("PlayerID", playerID);
            PlayerPrefs.Save();
        }

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            playerName = "Player-" + UnityEngine.Random.Range(100_000, 1_000_000);
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }

        Debug.Log($"Initialized Player Info: ID={playerID}, Name={playerName}");
    }

    internal static bool IsPlayerInfoValid()
    {
        return !string.IsNullOrEmpty(playerID) && !string.IsNullOrEmpty(playerName);
    }

    internal static IEnumerator FetchLeaderboardCo()
    {
        UnityWebRequest uwr = UnityWebRequest.Get("https://server.sharkmc.cn:8087/hollowTiles/getLeaderboards");

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError ||
            uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching leaderboard: " + uwr.error);
            Debug.LogError("Response Code: " + uwr.responseCode);
        }
        else
        {
            try
            {
                string jsonResponse = uwr.downloadHandler.text;

                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(WrapJsonArray(jsonResponse));
                leaderboardData = new List<PlayerInfo>(response.items);

                StartScript.instance.DisplayLeaderboard();
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing leaderboard JSON: " + e.Message);
                Debug.LogError("JSON Response: " + uwr.downloadHandler.text);
            }
        }

        uwr.Dispose();
    }

    internal static IEnumerator UploadRecordCo(long gameTime)
    {
        if (!IsPlayerInfoValid())
        {
            yield break;
        }

        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        PlayerInfo playerRecord = new PlayerInfo(playerID, playerName, currentTimestamp, gameTime);

        string jsonData = JsonUtility.ToJson(playerRecord);

        Debug.Log("Upload json: " + jsonData);

        UnityWebRequest uwr = new UnityWebRequest("https://server.sharkmc.cn:8087/hollowTiles/uploadRecord", "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
        uwr.downloadHandler = new DownloadHandlerBuffer();

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError ||
            uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error uploading record: " + uwr.error);
            Debug.LogError("Response Code: " + uwr.responseCode);
            Debug.LogError("Response Text: " + uwr.downloadHandler.text);
        }
        else
        {
            Debug.Log("Record uploaded successfully!");
            Debug.Log("Server response: " + uwr.downloadHandler.text);
        }

        uwr.Dispose();
    }

    private static string WrapJsonArray(string jsonArray)
    {
        return "{\"items\":" + jsonArray + "}";
    }

}