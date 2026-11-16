
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("UI Bars")]
    [SerializeField] private List<GameObject> emptyBars;

    [SerializeField] private GameObject EnergyBar;
    [SerializeField] private GameObject DistanceFromDissmisalBar;
    [SerializeField] private GameObject WakefulnessBar;

    [Header("Initial Stats")]
    [SerializeField, Range(0f, 100f)]
    private float Energy_initial_percent = 100f;

    [SerializeField, Range(0f, 100f)]
    private float Wakefulness_initial_percent = 100f;

    [SerializeField, Range(0f, 100f)]
    private float DistanceFromDissmisal_initial_percent = 100f;

    [Header("Drain Rates")]
    [SerializeField, Min(0f)]
    private float energyDrainRate = 1f;

    [SerializeField, Min(0f)]
    private float wakeFulnessDrainRate = 1f;

    [Header("Input")]
    [SerializeField]
    private InputActionReference moveAction;

    [Header("Game Over")]
    public UnityEvent onGameOver;

    private bool gameOverTriggered = false;

    private const float BAR_HEIGHT = 20f;

    [SerializeField]
    private float full_bar_width = 200f;

    private void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Enable();
    }

    private void Start()
    {
        emptyBars.ForEach(b =>
            setSize(b, full_bar_width, BAR_HEIGHT));

        setSize(EnergyBar,
            full_bar_width * Energy_initial_percent / 100f,
            BAR_HEIGHT);

        setSize(DistanceFromDissmisalBar,
            full_bar_width * DistanceFromDissmisal_initial_percent / 100f,
            BAR_HEIGHT);

        setSize(WakefulnessBar,
            full_bar_width * Wakefulness_initial_percent / 100f,
            BAR_HEIGHT);
    }

    
private void Update()
{
    if (gameOverTriggered)
        return;

    if (moveAction != null && moveAction.action != null)
    {
        Vector2 movement =
            moveAction.action.ReadValue<Vector2>();

        if (movement.sqrMagnitude > 0.0001f)
        {
            changeWidthByPercentOfFull(
                EnergyBar,
                -energyDrainRate * Time.deltaTime
            );

            changeWidthByPercentOfFull(
                WakefulnessBar,
                -wakeFulnessDrainRate * Time.deltaTime
            );
        }
    }

    CheckGameOver();
}


    // Restore energy by a percentage of the full bar
    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f)
            return;

        changeWidthByPercentOfFull(EnergyBar, amount);
    }

    // Restore wakefulness by a percentage of the full bar
    public void RestoreWakefulness(float amount)
    {
        if (amount <= 0f)
            return;

        changeWidthByPercentOfFull(WakefulnessBar, amount);
    }

    private void setSize(GameObject bar, float width, float height)
    {
        bar.GetComponent<RectTransform>().sizeDelta =
            new Vector2(width, height);
    }

    private void changeWidthByPercentOfFull(GameObject bar, float percent)
    {
        RectTransform rect = bar.GetComponent<RectTransform>();

        float width = rect.sizeDelta.x;
        float height = rect.sizeDelta.y;

        width += full_bar_width * percent / 100f;
        width = Mathf.Clamp(width, 0f, full_bar_width);

        setSize(bar, width, height);
    }


private void CheckGameOver()
{
    if (gameOverTriggered)
        return;

    float energy =
        EnergyBar.GetComponent<RectTransform>().sizeDelta.x;

    float wakefulness =
        WakefulnessBar.GetComponent<RectTransform>().sizeDelta.x;

    float dismissal =
        DistanceFromDissmisalBar.GetComponent<RectTransform>().sizeDelta.x;

    // Game over if any critical stat reaches zero
    if (energy <= 0f ||
        wakefulness <= 0f ||
        dismissal <= 0f)
    {
        TriggerGameOver();
    }
}

public void TriggerGameOver()
{
    if (gameOverTriggered)
        return;

    gameOverTriggered = true;

    Debug.Log("Game Over! Player stats depleted.");

    onGameOver?.Invoke();
}

}
