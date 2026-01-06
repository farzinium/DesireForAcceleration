using UnityEngine;

public class BackButton : MonoBehaviour
{
    [SerializeField] GameObject currentPanel, backPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Back()
    {
        currentPanel.SetActive(false);
        backPanel.SetActive(true);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

}
