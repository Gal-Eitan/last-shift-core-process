using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RemoteCanvasFrameLoader : MonoBehaviour
{
    [SerializeField] private bool preserveAspect = false;

    private Image frameImage;

    private void Awake()
    {
        frameImage = GetComponent<Image>();
        frameImage.preserveAspect = preserveAspect;
        frameImage.raycastTarget = false; // מונע מהמסגרת לחסום לחיצות עכבר על כפתורים מתחתיה
        frameImage.enabled = false;

        StretchToFullScreen();
    }

    private void Start()
    {
        if (LiveOpsConfigManager.Instance != null)
        {
            LiveOpsConfigManager.Instance.OnConfigLoaded += HandleConfigLoaded;

            if (LiveOpsConfigManager.Instance.CurrentConfig != null)
            {
                HandleConfigLoaded(LiveOpsConfigManager.Instance.CurrentConfig);
            }
        }
    }

    private void OnDestroy()
    {
        if (LiveOpsConfigManager.Instance != null)
        {
            LiveOpsConfigManager.Instance.OnConfigLoaded -= HandleConfigLoaded;
        }
    }

    private void StretchToFullScreen()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; // (0, 0) - פינה שמאלית תחתונה
        rect.anchorMax = Vector2.one;  // (1, 1) - פינה ימנית עליונה
        rect.offsetMin = Vector2.zero; // Left = 0, Bottom = 0
        rect.offsetMax = Vector2.zero; // Right = 0, Top = 0
        rect.localScale = Vector3.one;
    }

    private void HandleConfigLoaded(LiveConfigData config)
    {
        if (config?.activeEvent == null || !config.activeEvent.isActive ||
            string.IsNullOrWhiteSpace(config.activeEvent.frameImageUrl))
        {
            frameImage.enabled = false;
            return;
        }

        StartCoroutine(DownloadAndApplyFrame(config.activeEvent.frameImageUrl));
    }

    private IEnumerator DownloadAndApplyFrame(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                texture.filterMode = FilterMode.Point; // שומר על חדות Pixel Art

                Sprite loadedSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );

                frameImage.sprite = loadedSprite;
                frameImage.enabled = true;
                Debug.Log($"[LiveOps] Canvas frame loaded ({texture.width}x{texture.height}) from: {url}");
            }
            else
            {
                Debug.LogWarning($"[LiveOps] Failed to load frame image ({request.error}) from: {url}");
                frameImage.enabled = false;
            }
        }
    }
}