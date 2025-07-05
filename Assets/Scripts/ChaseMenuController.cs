using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChaseMenuController : MonoBehaviour
{
    public Button cobaLagiButton;
    public Button keluarButton;

    private Button[] buttons;
    private int currentIndex = 0;

    void Start()
    {
        // Put both buttons in an array for easy indexing
        buttons = new Button[] { cobaLagiButton, keluarButton };

        // Make sure the first button is selected at start
        HighlightButton(currentIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = buttons.Length - 1;

            HighlightButton(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex++;
            if (currentIndex >= buttons.Length)
                currentIndex = 0;

            HighlightButton(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            // Press the button if Enter or Space is pressed
            buttons[currentIndex].onClick.Invoke();
        }
    }

    void HighlightButton(int index)
    {
        // Use Unity's EventSystem to highlight/select the button
        EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
    }
}
