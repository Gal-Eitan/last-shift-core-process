using TMPro;
using UnityEngine;

public class EventDesciptionDisplayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (LiveOpsConfigManager.Instance != null)
        {
            LiveOpsConfigManager.Instance.OnConfigLoaded += DisplayDescription;

            if (LiveOpsConfigManager.Instance.CurrentConfig != null)
            {
                DisplayDescription(LiveOpsConfigManager.Instance.CurrentConfig);
            }
        }
    }

    private void OnDestroy()
    {
        if (LiveOpsConfigManager.Instance != null)
        {
            LiveOpsConfigManager.Instance.OnConfigLoaded -= DisplayDescription;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void DisplayDescription(LiveConfigData config)
    {
        string text= $"<mark=#000000>{config.activeEvent.eventTitle}</mark>";

        gameObject.GetComponent<TextMeshProUGUI>().SetText(text);
    }
}
