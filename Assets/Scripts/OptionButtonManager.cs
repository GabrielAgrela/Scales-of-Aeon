using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionButtonManager : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private Animator scaleBalanceAnimator; // Reference to the Animator

    [SerializeField]
    private const float TiltIntensity = 1f;

    private float currentTilt = 0f;
    private float targetTilt = 0f;
    private float tiltSpeed = 0.5f; // This will increase to create acceleration
    private float acceleration = 1.0f; // Adjust this to change the rate of acceleration

    private CursorController _cursorController;

    void Start()
    {
        _cursorController = FindObjectOfType<CursorController>();
        if (leftButton == null || rightButton == null || scaleBalanceAnimator == null)
        {
            Debug.LogError("Missing references in OptionButtonManager");
        }
        // Setup event listeners for the left and right buttons
        AddEventTriggerListener(leftButton.gameObject, EventTriggerType.PointerEnter, () => UpdateTilt(-TiltIntensity));
        AddEventTriggerListener(leftButton.gameObject, EventTriggerType.PointerExit, () => UpdateTilt(0));

        AddEventTriggerListener(rightButton.gameObject, EventTriggerType.PointerEnter, () => UpdateTilt(TiltIntensity));
        AddEventTriggerListener(rightButton.gameObject, EventTriggerType.PointerExit, () => UpdateTilt(0));
    }

    void Update()
    {
        if (currentTilt != targetTilt)
        {
            // Increase the tilt speed over time to create acceleration
            tiltSpeed += acceleration * Time.unscaledDeltaTime;

            // Move the currentTilt towards the targetTilt at the accelerating speed
            currentTilt = Mathf.MoveTowards(currentTilt, targetTilt, tiltSpeed * Time.unscaledDeltaTime);

            // Update the Animator's Blend parameter with the current tilt value
            scaleBalanceAnimator.SetFloat("Blend", currentTilt);
        }
    }

    void OnEnable()
    {
        ResetTilt();
        // If you want to ensure that the Animator updates immediately, you can add this line:
    }

    void OnDisable()
    {
        ResetTilt();
        // The following line is commented out as it might be redundant when the GameObject is disabled.
        // scaleBalanceAnimator.SetFloat("Blend", 0f);
    }

    private void UpdateTilt(float tiltValue)
    {
        targetTilt = tiltValue;

        // Reset tilt speed to 0 whenever a new tilt is initiated
        tiltSpeed = 0f;
    }

    private void ResetTilt()
    {
        // Call this method when you want to reset the tilt to neutral (0)
        targetTilt = 0f;
        tiltSpeed = 0f;
        currentTilt = 0f; // Instantly reset the current tilt or smoothly animate it back to 0 as needed
        scaleBalanceAnimator.SetFloat("Blend", currentTilt);
    }

    private void AddEventTriggerListener(GameObject target, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry { eventID = eventType };
        eventTrigger.callback.AddListener((data) =>
        {
            action();
            if (_cursorController != null)
            {
                if (eventType == EventTriggerType.PointerEnter)
                {
                    _cursorController.SetClickPointer();
                }
                else if (eventType == EventTriggerType.PointerExit)
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    _cursorController.ResetPointer();
                }
            }
        });
        trigger.triggers.Add(eventTrigger);
    }
}