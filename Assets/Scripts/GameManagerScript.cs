using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{

    internal static GameManagerScript instance;
    internal static bool isPaused = false, hasWon = false;
    internal static Action onWinAction;

    public GameObject backgroundTilesObject, frontTilesObject, selfRefPointObject, goBackBtn;
    public GameObject tilePrefab, maskTilePrefab, refPointPrefab, warningPrefab;
    public BlackCoverScript blackCoverScript;
    public SpriteRenderer borderSpriteRenderer;
    public AudioSource bgmAudoioSource;
    public TextMeshProUGUI timeText;

    private static List<RefPointScript> refPoints = new List<RefPointScript>();

    private static int[][] backgroundTiles = new int[][] { };
    private static int[][] targetTilePattern = new int[][] { };
    private static TileScript[][] tileObjects = new TileScript[][] { };

    private static int currentLevel = 1, playerScore = 0;
    private static int mapWidth = 55, mapHeight = 21;

    internal static long startTime = 0, endTime = 0;

    public void Awake()
    {
        Application.targetFrameRate = 60;

        instance = this;

        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        GenerateRandomBackgroundTiles();
        FindNewTargetPattern();

        CreateBackgroundTiles();
        CreateFrontTiles();
        LoadPatterns();
    }


    public void Update()
    {
        if (!isPaused)
        {
            UpdateBgmAudio();
        }

        UpdateTimer();
    }

    private void GenerateRandomBackgroundTiles()
    {
        mapWidth = 35 + currentLevel * 10;
        mapHeight = 11 + currentLevel * 10;

        backgroundTiles = new int[mapHeight][];
        System.Random rand = new System.Random(DateTimeOffset.Now.Millisecond);

        for (int i = 0; i < mapHeight; i++)
        {
            backgroundTiles[i] = new int[mapWidth];

            for (int j = 0; j < mapWidth; j++)
            {
                int tilesAround = 0;
                for (int x = Mathf.Max(i - 1, 0); x <= i; x++)
                {
                    for (int y = Mathf.Max(j - 1, 0); y <= j; y++)
                    {
                        if (backgroundTiles[x][y] == 1)
                        {
                            tilesAround++;
                        }
                    }
                }

                if (rand.Next(0, 100) <= 60 + tilesAround * 10)
                {
                    backgroundTiles[i][j] = 0;
                }
                else
                {
                    backgroundTiles[i][j] = 1;
                }
            }
        }
    }

    private void FindNewTargetPattern()
    {
        System.Random rand = new System.Random(DateTimeOffset.Now.Millisecond);
        
        bool foundValidPattern = false;
        int maxAttempts = 100;
        int attempts = 0;
        int minWidth = 1 + currentLevel, minHeight = 1 + currentLevel;

        while (!foundValidPattern && attempts < maxAttempts)
        {
            attempts++;

            int patternWidth = rand.Next(1 + currentLevel, 1 + currentLevel * 2), patternHeight = rand.Next(1 + currentLevel, 1 + currentLevel * 2);
            int startX = rand.Next(0, backgroundTiles[0].Length - patternWidth), startY = rand.Next(0, backgroundTiles.Length - patternHeight);

            int[][] originalPattern = new int[patternHeight][];
            for (int i = 0; i < patternHeight; i++)
            {
                originalPattern[i] = new int[patternWidth];
                for (int j = 0; j < patternWidth; j++)
                {
                    originalPattern[i][j] = backgroundTiles[startY + i][startX + j];
                }
            }

            bool hasAnyOne = false;
            for (int i = 0; i < patternHeight && !hasAnyOne; i++)
            {
                for (int j = 0; j < patternWidth && !hasAnyOne; j++)
                {
                    if (originalPattern[i][j] == 1)
                    {
                        hasAnyOne = true;
                    }
                }
            }

            if (!hasAnyOne)
            {
                continue;
            }

            int firstValidRow = patternHeight;
            for (int i = 0; i < patternHeight; i++)
            {
                bool hasOne = false;
                for (int j = 0; j < patternWidth; j++)
                {
                    if (originalPattern[i][j] == 1)
                    {
                        hasOne = true;
                        break;
                    }
                }
                if (hasOne)
                {
                    firstValidRow = i;
                    break;
                }
            }

            int lastValidRow = -1;
            for (int i = patternHeight - 1; i >= 0; i--)
            {
                bool hasOne = false;
                for (int j = 0; j < patternWidth; j++)
                {
                    if (originalPattern[i][j] == 1)
                    {
                        hasOne = true;
                        break;
                    }
                }
                if (hasOne)
                {
                    lastValidRow = i;
                    break;
                }
            }

            int firstValidCol = patternWidth;
            for (int j = 0; j < patternWidth; j++)
            {
                bool hasOne = false;
                for (int i = 0; i < patternHeight; i++)
                {
                    if (originalPattern[i][j] == 1)
                    {
                        hasOne = true;
                        break;
                    }
                }
                if (hasOne)
                {
                    firstValidCol = j;
                    break;
                }
            }

            int lastValidCol = -1;
            for (int j = patternWidth - 1; j >= 0; j--)
            {
                bool hasOne = false;
                for (int i = 0; i < patternHeight; i++)
                {
                    if (originalPattern[i][j] == 1)
                    {
                        hasOne = true;
                        break;
                    }
                }
                if (hasOne)
                {
                    lastValidCol = j;
                    break;
                }
            }

            int newHeight = lastValidRow - firstValidRow + 1;
            int newWidth = lastValidCol - firstValidCol + 1;

            if (newHeight < minHeight || newWidth < minWidth)
            {
                continue;
            }
            
            targetTilePattern = new int[newHeight][];
            for (int i = 0; i < newHeight; i++)
            {
                targetTilePattern[i] = new int[newWidth];
                for (int j = 0; j < newWidth; j++)
                {
                    targetTilePattern[i][j] = originalPattern[firstValidRow + i][firstValidCol + j];
                }
            }

            foundValidPattern = true;
        }

        /*Debug.Log("Target Pattern Dimensions: " + targetTilePattern.Length + "x" + targetTilePattern[0].Length);
        for (int i = 0; i < targetTilePattern.Length; i++)
        {
            string row = string.Join(",", targetTilePattern[i]);
            Debug.Log("Row " + i + ": " + row);
        }*/
    }

    private void CreateBackgroundTiles()
    {
        ClearChildObjects(backgroundTilesObject);

        tileObjects = new TileScript[backgroundTiles.Length][];

        for (int i = 0; i < backgroundTiles.Length; i++)
        {
            int[] ints = backgroundTiles[i];
            tileObjects[i] = new TileScript[ints.Length];

            for (int j = 0; j < ints.Length; j++)
            {
                if (ints[j] == 0) continue;

                TileScript ts = GameObject.Instantiate(tilePrefab, backgroundTilesObject.transform).GetComponent<TileScript>();
                ts.transform.localPosition = new Vector3(j, -i, 0);
                ts.gridIndex = new Vector2Int(j, i);

                tileObjects[i][j] = ts;
            }
        }

        backgroundTilesObject.transform.localPosition = new Vector3(-backgroundTiles[0].Length / 2 + 0.5f, backgroundTiles.Length / 2 - 0.5f, 0);
    }

    private void CreateFrontTiles()
    {
        ClearChildObjects(frontTilesObject);

        int maxRowLength = targetTilePattern.Length, maxColumnLength = 0;

        for (int i = 0; i < targetTilePattern.Length; i++)
        {
            int[] ints = targetTilePattern[i];

            maxColumnLength = Mathf.Max(maxColumnLength, ints.Length);

            for (int j = 0; j < ints.Length; j++)
            {
                if (ints[j] == 0) continue;

                GameObject go = GameObject.Instantiate(maskTilePrefab, frontTilesObject.transform);
                go.transform.localPosition = new Vector3(j, -i, 0);
            }
        }

        frontTilesObject.transform.localPosition = new Vector3(-maxColumnLength / 2 + 0.5f, maxRowLength / 2 - 0.5f, 0);
        selfRefPointObject.transform.localPosition = new Vector3(-maxColumnLength / 2 + 0.5f, maxRowLength / 2 - 0.5f, 0);

        borderSpriteRenderer.transform.localPosition = new Vector3((maxColumnLength % 2) * 0.5f, -(maxRowLength % 2) * 0.5f, 0);
        borderSpriteRenderer.size = new Vector2(maxColumnLength + 0.25f, maxRowLength + 0.25f);
    }

    private void LoadPatterns()
    {
        refPoints.Clear();

        List<Vector2Int> patternPositions = FindAllPatternPositions();

        foreach (Vector2Int v in patternPositions)
        {
            RefPointScript rps = GameObject.Instantiate(refPointPrefab, backgroundTilesObject.transform).GetComponent<RefPointScript>();
            rps.transform.localPosition = new Vector3(v.x, -v.y, 0);
            rps.gridIndex = v;

            refPoints.Add(rps);
        }
    }

    private List<Vector2Int> FindAllPatternPositions()
    {
        List<Vector2Int> patternPositions = new List<Vector2Int>();

        int patternHeight = targetTilePattern.Length;
        int patternWidth = targetTilePattern[0].Length;
        int backgroundHeight = backgroundTiles.Length;
        int backgroundWidth = backgroundTiles[0].Length;

        for (int row = 0; row <= backgroundHeight - patternHeight; row++)
        {
            for (int col = 0; col <= backgroundWidth - patternWidth; col++)
            {
                if (IsPatternMatch(row, col, patternHeight, patternWidth))
                {
                    patternPositions.Add(new Vector2Int(col, row));
                }
            }
        }

        return patternPositions;
    }

    private bool IsPatternMatch(int startRow, int startCol, int patternHeight, int patternWidth)
    {
        for (int i = 0; i < patternHeight; i++)
        {
            for (int j = 0; j < patternWidth; j++)
            {
                if (backgroundTiles[startRow + i][startCol + j] != targetTilePattern[i][j])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ClearChildObjects(GameObject o)
    {
        for (int i = o.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(o.transform.GetChild(i).gameObject);
        }
    }

    public void CheckPatternAtViewPosition()
    {
        Vector3 pos = ViewController.instance.transform.position;
        bool isWholeNumber = Mathf.Abs(Mathf.Round(pos.x) - pos.x) < 0.01f &&
                             Mathf.Abs(Mathf.Round(pos.y) - pos.y) < 0.01f;

        //Debug.Log($"ViewController position: {pos}, isWholeNumber: {isWholeNumber}");

        if (!isWholeNumber)
        {
            //Debug.Log("Not whole number");
            return;
        }

        Vector3 viewWorldPos = selfRefPointObject.transform.position;
        Vector3 backgroundOffset = backgroundTilesObject.transform.localPosition;

        Vector3 relativePos = viewWorldPos - backgroundOffset;
        int gridX = Mathf.RoundToInt(relativePos.x);
        int gridY = Mathf.RoundToInt(-relativePos.y);

        int patternHeight = targetTilePattern.Length;
        int patternWidth = targetTilePattern[0].Length;

        if (gridX < 0 || gridY < 0 ||
            gridX + patternWidth > mapWidth ||
            gridY + patternHeight > mapHeight)
        {
            return;
        }

        bool matched = IsPatternMatch(gridY, gridX, patternHeight, patternWidth);

        if (matched)
        {
            blackCoverScript.SetTargetColor(Color.black.WithAlpha(0));

            for (int i = gridY; i < gridY + patternHeight; i++)
            {
                for (int j = gridX; j < gridX + patternWidth; j++)
                {
                    if (tileObjects[i][j] != null)
                    {
                        tileObjects[i][j].SetTargetColor(Color.green);
                    }
                }
            }

            StartCoroutine(LoadNextLevelCo());
        }
        else
        {
            for (int i = 0; i < patternHeight; i++)
            {
                for (int j = 0; j < patternWidth; j++)
                {
                    int bgRow = gridY + i;
                    int bgCol = gridX + j;

                    //Debug.Log("backgroundTiles[" + bgRow + "][" + bgCol + "]: " + backgroundTiles[bgRow][bgCol]);
                    //Debug.Log("targetTilePattern[" + i + "][" + j + "]: " + targetTilePattern[i][j]);
                    if (backgroundTiles[bgRow][bgCol] != targetTilePattern[i][j])
                    {
                        WarningScript ws = GameObject.Instantiate(warningPrefab, frontTilesObject.transform).GetComponent<WarningScript>();
                        ws.transform.localPosition = new Vector3(j, -i, 0);
                        //Debug.Log("mismatch: " + i + ", " + j);
                    }
                }
            }
        }
    }

    private IEnumerator LoadNextLevelCo()
    {
        isPaused = true;

        yield return new WaitForSeconds(2f);

        playerScore += 10;

        if (currentLevel == 5)
        {
            hasWon = true;
            endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            blackCoverScript.SetTargetColor(Color.black);
            bgmAudoioSource.loop = false;
            goBackBtn.SetActive(true);

            StartCoroutine(LeaderboardManager.UploadRecordCo(endTime - startTime));

            if (onWinAction != null)
            {
                onWinAction.Invoke();
            }

            yield break;
        }

        currentLevel++;

        GenerateRandomBackgroundTiles();
        FindNewTargetPattern();
        CreateBackgroundTiles();
        CreateFrontTiles();
        LoadPatterns();

        blackCoverScript.SetTargetColor(Color.black.WithAlpha(240 / 255f));

        isPaused = false;
    }

    private void UpdateBgmAudio()
    {
        if (refPoints.Count == 0)
        {
            bgmAudoioSource.pitch = 1f;
            return;
        }

        Vector3 viewWorldPos = selfRefPointObject.transform.position;
        Vector3 backgroundOffset = backgroundTilesObject.transform.localPosition;
        Vector3 relativePos = viewWorldPos - backgroundOffset;
        int gridX = Mathf.RoundToInt(relativePos.x);
        int gridY = Mathf.RoundToInt(-relativePos.y);

        Vector2Int currentGridPos = new Vector2Int(gridX, gridY);

        float minDistance = float.MaxValue;
        foreach (RefPointScript rps in refPoints)
        {
            float distance = Vector2Int.Distance(currentGridPos, rps.gridIndex);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        float maxDistance = Mathf.Sqrt(mapWidth * mapWidth + mapHeight * mapHeight) * 0.5f;

        float normalizedDistance = Mathf.Clamp01(minDistance / maxDistance);

        float pitchFactor = 1f - normalizedDistance;

        float targetPitch = Mathf.Lerp(0.75f, 2.5f, pitchFactor);

        bgmAudoioSource.pitch = Mathf.Lerp(bgmAudoioSource.pitch, targetPitch, Time.deltaTime * 3f);
    }

    private void UpdateTimer()
    {
        long timeElapsed;
        if (hasWon)
        {
            timeElapsed = endTime - startTime;
        }
        else
        {
            timeElapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
        }

        timeText.text = GetFormattedGameTime(timeElapsed);
    }

    internal static string GetFormattedGameTime(long timeElapsed)
    {
        long totalSeconds = timeElapsed / 1000;
        long hours = totalSeconds / 3600;
        long minutes = (totalSeconds % 3600) / 60;
        long seconds = totalSeconds % 60;
        long millis = timeElapsed % 1000;
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds) + " <size=16>" + millis + "</size>";
    }

    public static void OnBackBtnClick()
    {
        SceneManager.LoadScene("StartScene");
    }

}

public static class ColorExtensions
{
    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}