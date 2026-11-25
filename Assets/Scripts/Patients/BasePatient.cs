
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BasePatient : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Key debugRequestKey = Key.R;
    
    [Header("Patient Requests")]
    [SerializeField]
    private List<WeightedItem<ItemData>> requestableItems = new();

    [Header("Request Timer")]
    [SerializeField, Min(0.1f)]
    private float minRequestInterval = 5f;

    [SerializeField, Min(0.1f)]
    private float maxRequestInterval = 15f;

    [Header("Events")]
    public UnityEvent onRequestCreated;
    public UnityEvent onRequestFulfilled;

    


    [Header("UI")]
    [SerializeField] private ItemBubble itemBubble;

    public ItemData CurrentRequest { get; private set; }

    public bool HasActiveRequest => CurrentRequest != null;

    private float requestTimer;

    private void Awake()
{
    Debug.Log($"BasePatient Awake: {gameObject.name}");
}

private void OnEnable()
{
    Debug.Log($"BasePatient enabled: {gameObject.name}");
}


    private void Start()
    {
        if (LiveOpsConfigManager.Instance != null)
    {
        LiveOpsConfigManager.Instance.OnConfigLoaded += UpdateRequestsWeightsMultipiers;
        
        if (LiveOpsConfigManager.Instance.CurrentConfig != null)
            UpdateRequestsWeightsMultipiers(LiveOpsConfigManager.Instance.CurrentConfig);
    }
        Debug.Log($"BasePatient Start: {gameObject.name}");

        if (itemBubble != null)
        {
            itemBubble.HideBubble();
        }

        ResetRequestTimer();
    }

    private void OnDestroy()
    {
        if (LiveOpsConfigManager.Instance != null)
            LiveOpsConfigManager.Instance.OnConfigLoaded -= UpdateRequestsWeightsMultipiers;
    }
    private void Update()
    {
        if (Keyboard.current != null &&
        Keyboard.current[debugRequestKey].wasPressedThisFrame)
        {
            DebugGenerateRequest();
        }
        if (HasActiveRequest)
            return;

        requestTimer -= Time.deltaTime;

        if (requestTimer <= 0f)
            GenerateRequest();
    }

    private void GenerateRequest()
    {
        if (requestableItems.Count == 0)
        {
            Debug.LogWarning(
                $"{gameObject.name} has no requestable items!"
            );

            ResetRequestTimer();
            return;
        }

        //int index = Random.Range(0, requestableItems.Count);

        //CurrentRequest = requestableItems[index].item;
        CurrentRequest=WeightedRandom.Choose<ItemData>(requestableItems);

        if (CurrentRequest == null)
        {
            ResetRequestTimer();
            return;
        }

        Debug.Log(
            $"{gameObject.name} requests {CurrentRequest.itemName}"
        );

        onRequestCreated?.Invoke();
    }

    public bool FulfillRequest(ItemData deliveredItem)
    {
        if (!HasActiveRequest || deliveredItem == null)
            return false;

        if (deliveredItem != CurrentRequest)
            return false;

        Debug.Log(
            $"{gameObject.name} received {deliveredItem.itemName}"
        );

        CurrentRequest = null;

        onRequestFulfilled?.Invoke();

        ResetRequestTimer();

        return true;
    }

    private void ResetRequestTimer()
    {
        float min = Mathf.Max(0.1f, minRequestInterval);
        float max = Mathf.Max(min, maxRequestInterval);

        requestTimer = Random.Range(min, max);
    }

    private void OnValidate()
    {
        minRequestInterval = Mathf.Max(0.1f, minRequestInterval);

        maxRequestInterval = Mathf.Max(
            minRequestInterval,
            maxRequestInterval
        );
    }
    public void DebugGenerateRequest()
{
    if (HasActiveRequest)
    {
        Debug.Log(
            $"{gameObject.name} already has a request: " +
            CurrentRequest.itemName
        );
        return;
    }

    Debug.Log($"{gameObject.name}: Debug request triggered!");

    GenerateRequest();
}

    public void UpdateRequestsWeightsMultipiers(LiveConfigData data)
    {
        Dictionary<string,float> itemRequestsMultipier=data.activeEvent.modifiers.itemsRequestsMultiplier;
        foreach (WeightedItem<ItemData> weightedItem in requestableItems)
        {
            string itemName=weightedItem.item.itemName;
            if (itemRequestsMultipier.ContainsKey(itemName))
            {
                float multiplyBy=itemRequestsMultipier[itemName];
                weightedItem.weight*=multiplyBy;
                Debug.Log($"{itemName} request weight multiplied by {multiplyBy}");
            }
        }
        
    } 
}
