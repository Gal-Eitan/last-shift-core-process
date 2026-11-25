using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class DemoEventModifiers
{
    public float nurseSpeedMultiplier = 1.0f;

    public Dictionary<string, float> itemsRequestsMultiplier = new Dictionary<string, float>();
}

[Serializable]
public class ActiveEventData
{
    public string eventId = "NONE";
    public bool isActive = false;
    public string eventTitle = "";
    public string eventDescription = "";
    public string themeTintHex = "#FFFFFF";
    public string frameImageUrl = "";
    public DemoEventModifiers modifiers = new DemoEventModifiers();
}

[Serializable]
public class LiveConfigData
{
    public string configVersion = "1.1.0";
    public ActiveEventData activeEvent = new ActiveEventData();
}

public class LiveOpsConfigManager : MonoBehaviour
{
    public static LiveOpsConfigManager Instance { get; private set; }

    [Header("Remote Live Ops JSON Endpoint")]
    [SerializeField] private string remoteConfigUrl = "https://api.github.com/repos/Gal-Eitan/nurse-idea/contents/live-config-for-showing.json";
    [SerializeField] private string localFallbackFileName = "live-config.json";
    [SerializeField] private float requestTimeoutSeconds = 5f;

    public LiveConfigData CurrentConfig { get; private set; }
    public event Action<LiveConfigData> OnConfigLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(FetchLiveConfig());
    }

    public IEnumerator FetchLiveConfig()
    {
        bool isGitHubApi = remoteConfigUrl.Contains("api.github.com");
        string requestUrl = remoteConfigUrl;

        if (!isGitHubApi)
        {
            string separator = remoteConfigUrl.Contains("?") ? "&" : "?";
            requestUrl = $"{remoteConfigUrl}{separator}t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        }

        Debug.Log($"[LiveOps] Fetching from: {requestUrl}");

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            request.SetRequestHeader("Pragma", "no-cache");

            if (isGitHubApi)
            {
                request.SetRequestHeader("Accept", "application/vnd.github.v3.raw");
                request.SetRequestHeader("User-Agent", "UnityLiveOpsClient");
            }

            request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ApplyJsonConfig(request.downloadHandler.text, "Remote Server");
            }
            else
            {
                Debug.LogWarning($"[LiveOps] Remote fetch failed ({request.error}). Loading local fallback...");
                LoadLocalFallbackConfig();
            }
        }
    }

    private void LoadLocalFallbackConfig()
    {
        string localPath = Path.Combine(Application.streamingAssetsPath, localFallbackFileName);
        if (File.Exists(localPath))
        {
            string json = File.ReadAllText(localPath);
            ApplyJsonConfig(json, "Local StreamingAssets");
        }
        else
        {
            CurrentConfig = new LiveConfigData();
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
    }

    private void ApplyJsonConfig(string json, string source)
    {
        try
        {
            CurrentConfig = JsonUtility.FromJson<LiveConfigData>(json);

            if (CurrentConfig.activeEvent == null || !CurrentConfig.activeEvent.isActive)
            {
                if (CurrentConfig.activeEvent == null)
                    CurrentConfig.activeEvent = new ActiveEventData();
                CurrentConfig.activeEvent.modifiers = new DemoEventModifiers();
            }
            else
            {
                if (CurrentConfig.activeEvent.modifiers == null)
                    CurrentConfig.activeEvent.modifiers = new DemoEventModifiers();

                ParseItemsDictionary(json, CurrentConfig.activeEvent.modifiers.itemsRequestsMultiplier);
            }

            Debug.Log($"[LiveOps] Loaded '{CurrentConfig.activeEvent.eventId}' (items={CurrentConfig.activeEvent.modifiers.itemsRequestsMultiplier.Count}) from {source}.");
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LiveOps] JSON parse error: {ex.Message}\n--- RAW JSON RECEIVED ---\n{json}");
            CurrentConfig = new LiveConfigData();
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
    }

    private void ParseItemsDictionary(string json, Dictionary<string, float> targetDict)
{
    targetDict.Clear();
    Match blockMatch = Regex.Match(json, "\"itemsRequestsMultiplier\"\\s*:\\s*\\{([^}]*)\\}");
    if (!blockMatch.Success) return;

    MatchCollection entries = Regex.Matches(blockMatch.Groups[1].Value, "\"([^\"]+)\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)");
    foreach (Match entry in entries)
    {
        if (float.TryParse(entry.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float multiplier))
        {
            targetDict[entry.Groups[1].Value] = multiplier;
        }
    }
}
}