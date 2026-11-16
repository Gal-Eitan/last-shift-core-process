
using TMPro;
using UnityEngine;

public class ItemInteractor : MonoBehaviour
{
    private Collider2D other_collider;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI item_name_view;

    [Header("Item Holding")]
    [SerializeField]
    private Transform HoldItemPoint;

    [SerializeField]
    private float dropDistance = 0.5f;

    private GameObject itemHeld;

    private void Start()
    {
        other_collider = null;
        itemHeld = null;

        GetComponent<NursePlayerController>()
            .onInteract.AddListener(Interact);
    }

    private void OnDestroy()
    {
        NursePlayerController controller =
            GetComponent<NursePlayerController>();

        if (controller != null)
            controller.onInteract.RemoveListener(Interact);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ItemToHold"))
        {
            other_collider = other;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == other_collider)
        {
            other_collider = null;
        }
    }

    public void Interact()
    {
        if (itemHeld != null)
        {
            DropItem();
        }
        else
        {
            holdItem();
        }
    }

    public void holdItem()
    {
        if (itemHeld != null)
            return;

        if (other_collider == null || HoldItemPoint == null)
            return;

        ItemHolder item = other_collider.GetComponent<ItemHolder>();

        if (item == null)
            return;

        itemHeld = item.gameObject;

        // Attach item to the player
        itemHeld.transform.SetParent(HoldItemPoint, false);
        itemHeld.transform.localPosition = Vector3.zero;
        itemHeld.transform.localRotation = Quaternion.identity;

        // Disable collider while carried
        Collider2D itemCollider = itemHeld.GetComponent<Collider2D>();

        if (itemCollider != null)
            itemCollider.enabled = false;

        // Update UI
        if (item_name_view != null)
            item_name_view.SetText(item.getName());

        other_collider = null;
    }

    public void DropItem()
    {
        if (itemHeld == null)
            return;

        GameObject droppedItem = itemHeld;

        // Detach from player
        droppedItem.transform.SetParent(null, true);

        // Drop near the player
        Vector3 dropPosition =
            HoldItemPoint.position +
            HoldItemPoint.right * dropDistance;

        droppedItem.transform.position = dropPosition;

        // Restore collider
        Collider2D itemCollider =
            droppedItem.GetComponent<Collider2D>();

        if (itemCollider != null)
            itemCollider.enabled = true;

        itemHeld = null;

        // Clear UI
        if (item_name_view != null)
            item_name_view.SetText("");

        other_collider = null;
    }

    public GameObject GetHeldItem()
    {
        return itemHeld;
    }

public void RemoveHeldItem()
{
    if (itemHeld == null)
        return;

    Destroy(itemHeld);
    itemHeld = null;

    if (item_name_view != null)
        item_name_view.SetText("");

    Debug.Log("Delivered item removed from player.");
}

}
