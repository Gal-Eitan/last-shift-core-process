
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(BasePatient))]
public class PatientItemReceiver : MonoBehaviour
{
    private BasePatient patient;

    private void Awake()
    {
        patient = GetComponent<BasePatient>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryReceiveItem(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryReceiveItem(other);
    }

    private void TryReceiveItem(Collider2D other)
    {
        if (!patient.HasActiveRequest)
            return;

        ItemInteractor player =
            other.GetComponentInParent<ItemInteractor>();

        if (player == null)
            return;

        GameObject heldItem = player.GetHeldItem();

        if (heldItem == null)
            return;

        WorldItem worldItem =
            heldItem.GetComponent<WorldItem>();

        if (worldItem == null)
            return;

        ItemData deliveredItem = worldItem.Data;

        if (patient.FulfillRequest(deliveredItem))
        {
            player.RemoveHeldItem();

            Debug.Log(
                $"{patient.name} received {deliveredItem.itemName}"
            );
        }
    }
}
