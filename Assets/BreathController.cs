using TMPro;
using UnityEngine;

public class BreathControllerUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform goalCircle;
    public RectTransform innerCircle;
    public TMP_Text breathCueText;
    public TMP_Text breathCountText;
    // Reference to the existing Input Handler component
    public GameObject yarnInputHandler;

    [Header("Settings")]
    public float smoothTime = 3.0f;
    public float threshold = 0.05f;
    public int breathsNeeded = 3;

    [Header("Min/Max sizes")]
    public float minSize = 30f;
    private float maxSize;

    private int breathsCompleted = 0;
    private float innerCurrentSize;
    private float innerVelocity;

    private bool inhaleComplete = false;

    void Start()
    {
        maxSize = goalCircle.sizeDelta.x;
        innerCurrentSize = innerCircle.sizeDelta.x;
        breathCueText.text = "Hold E untuk tarik napas";
        yarnInputHandler.SetActive(false);
    }

    void Update()
    {
        bool isBreathingIn = Input.GetKey(KeyCode.E);
        float targetSize = isBreathingIn ? maxSize : minSize;

        innerCurrentSize = Mathf.SmoothDamp(
            innerCurrentSize,
            targetSize,
            ref innerVelocity,
            smoothTime
        );

        innerCircle.sizeDelta = new Vector2(innerCurrentSize, innerCurrentSize);

        if (isBreathingIn)
        {
            float diffGoal = Mathf.Abs(innerCurrentSize - maxSize);
            if (diffGoal < threshold)
            {
                inhaleComplete = true;
                breathCueText.text = "Lepas E untuk buang napas";
            }
        }
        else
        {
            breathCueText.text = "Hold E untuk tarik napas";

            float diffMin = Mathf.Abs(innerCurrentSize - minSize);
            if (inhaleComplete && diffMin < threshold)
            {
                breathsCompleted++;
                Debug.Log("Breath cycle completed: " + breathsCompleted);
                inhaleComplete = false;

                if (breathsCompleted >= breathsNeeded)
                {
                    Debug.Log("All breaths completed!");
                    yarnInputHandler.SetActive(true);
                    gameObject.SetActive(false); // Deactivate the BreathUI
                }

                breathCountText.text = "Hitungan napas: " + breathsCompleted + "/3";
            }
        }

    }
}
