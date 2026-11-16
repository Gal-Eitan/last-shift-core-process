
using UnityEngine;

public class ItemBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BasePatient patient;
    [SerializeField] private SpriteRenderer itemIcon;

    private SpriteRenderer bubbleRenderer;

    private void Awake()
    {
        bubbleRenderer = GetComponent<SpriteRenderer>();
        HideBubble();
    }

    public void ShowBubble()
    {
        if (bubbleRenderer != null)
            bubbleRenderer.enabled = true;

        if (itemIcon != null)
            itemIcon.enabled = true;
    }

    public void HideBubble()
    {
        if (bubbleRenderer != null)
            bubbleRenderer.enabled = false;

        if (itemIcon != null)
            itemIcon.enabled = false;
    }

    public void SetItem(ItemData item)
    {
        if (itemIcon == null)
            return;

        itemIcon.sprite = item != null ? item.icon : null;
    }

    // Connect this to BasePatient.onRequestCreated
    public void OnRequestCreated()
    {
        if (patient == null || patient.CurrentRequest == null)
            return;

        SetItem(patient.CurrentRequest);
        ShowBubble();
    }

    // Connect this to BasePatient.onRequestFulfilled
    public void OnRequestFulfilled()
    {
        HideBubble();
        SetItem(null);
    }
}
