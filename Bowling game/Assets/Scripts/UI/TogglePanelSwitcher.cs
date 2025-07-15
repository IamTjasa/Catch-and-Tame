using UnityEngine;
using UnityEngine.UI;

public class TogglePanelSwitcher : MonoBehaviour
{
    public GameObject ballPanel;
    public GameObject indexPanel;
    private bool showingBall = true;

    void Start()
    {
        // Nastavi zaèetno stanje
        ballPanel.SetActive(true);
        indexPanel.SetActive(false);

        // Dodaj listener na gumb
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(SwitchPanels);
        }
    }

    void SwitchPanels()
    {
        showingBall = !showingBall;

        ballPanel.SetActive(showingBall);
        indexPanel.SetActive(!showingBall);
    }
}
