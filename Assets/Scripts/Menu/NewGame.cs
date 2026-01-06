using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
	public TMPro.TMP_InputField nameInput;
	public Button confirmButton;
    public void CreateNewGame()
	{
		string playerName = nameInput.text;
		string path = Application.persistentDataPath + "/" + playerName + ".txt";

		File.WriteAllText(path, playerName);
		PlayerPrefs.SetString("CurrentSave", path);

		SceneManager.LoadScene("GameScene");
	}

	public void check()
	{
		if (nameInput.text.Length > 0)
		{
			confirmButton.interactable = true;
		}
		else
		{
            confirmButton.interactable = false;
        }
	}
}
