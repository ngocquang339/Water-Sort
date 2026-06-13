using System.Collections;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.HDROutputUtils;

public class SetNameManager : MonoBehaviour
{
    public TMP_InputField userName;
    public GameObject SetNamePopup;

	[Header("Hiệu ứng chuyển cảnh")]
	public Image fadeImage;
	public float fadeDuration = 0.5f;

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
		// Đợi game lưu tên lên mạng xong mới cho chuyển màn (hoặc gọi chuyển màn luôn cũng được)
		LeaderboardManager.Instance.SubmitPlayerName(newName);
		// Sửa toàn bộ các lệnh gọi fade ở mọi nơi trong game thành dạng này:
		LoadingSceneSmooth.Instance.StartCoroutine(LoadingSceneSmooth.Instance.LoadSceneSmooth("MainScene"));
	}
}
