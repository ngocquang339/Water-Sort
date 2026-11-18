using UnityEngine;
using TMPro;
public class LevelUIText : MonoBehaviour
{
	public TextMeshProUGUI levelNumber;
	void Awake()
	{
		UpdateLevelText();
	}
	public void UpdateLevelText(){
		levelNumber.text = PlayerPrefs.GetInt("CurrentLevel", 1).ToString();
	}
}
