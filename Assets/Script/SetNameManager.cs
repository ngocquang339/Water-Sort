using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetNameManager : MonoBehaviour
{
    public TMP_InputField userName;
    public GameObject SetNamePopup;

    public void SetName(){
        string newName = userName.text.Trim();
		if (!string.IsNullOrEmpty(newName))
		{
			PlayerPrefs.SetString("Player_Username", newName);
			PlayerPrefs.Save();
			Debug.Log("Player name set to: " + newName);
		}
		else
		{
			Debug.LogWarning("Username cannot be empty.");
		}
		moveScene();
	}

	public void moveScene(){
		SceneManager.LoadScene("MainScene");
	}
}
