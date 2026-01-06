using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class LoadGameMenu : MonoBehaviour
{
    public Transform content;
    public GameObject buttonPrefab;

    void OnEnable()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.txt");

        int index = 0;
        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            GameObject btn = Instantiate(buttonPrefab, content );
            btn.GetComponentInChildren<TextMeshProUGUI>().text = name;
            btn.GetComponent<Button>().onClick.AddListener(() => LoadGame(file));
            index++;
        }
    }

    void LoadGame(string path)
    {
        PlayerPrefs.SetString("CurrentSave", path);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
