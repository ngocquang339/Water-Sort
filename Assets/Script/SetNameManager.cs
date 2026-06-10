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
		StartCoroutine(LoadSceneSmooth());
	}


	IEnumerator LoadSceneSmooth()
	{
		// =========================================================
		// 1. KÉO RÈM ĐEN TRƯỚC (Che mắt người chơi)
		// =========================================================
		if (fadeImage != null)
		{
			fadeImage.raycastTarget = true; // Chặn bấm bậy
			float timer = 0f;
			Color c = fadeImage.color;
			while (timer < fadeDuration)
			{
				timer += Time.deltaTime;
				c.a = Mathf.Clamp01(timer / fadeDuration);
				fadeImage.color = c;
				yield return null; // Lúc này CPU rảnh rỗi 100% nên Fade cực kỳ mượt
			}
		}

		// [BÍ QUYẾT]: Bất tử hóa script này và tấm rèm đen để Coroutine không bị chết
		DontDestroyOnLoad(this.gameObject);
		if (fadeImage != null) DontDestroyOnLoad(fadeImage.canvas.gameObject);

		// =========================================================
		// 2. BẮT ĐẦU LOAD NGẦM LÚC MÀN HÌNH ĐÃ TỐI ĐEN
		// =========================================================
		AsyncOperation operation = SceneManager.LoadSceneAsync("MainScene");
		operation.allowSceneActivation = false;

		// Chờ load xong dữ liệu (tiến trình đạt 90%)
		while (operation.progress < 0.9f)
		{
			yield return null;
		}

		// Mở khóa cho sang bài
		operation.allowSceneActivation = true;

		// Chờ Unity dọn dẹp màn cũ, vẽ màn mới (đây là lúc nó giật nhất, nhưng màn hình đang đen nên không sao)
		while (!operation.isDone)
		{
			yield return null;
		}

		// =========================================================
		// 3. MỞ RÈM RA TẠI MÀN HÌNH MỚI (FADE IN)
		// =========================================================
		if (fadeImage != null)
		{
			float timer = 0f;
			Color c = fadeImage.color;
			while (timer < fadeDuration)
			{
				timer += Time.deltaTime;
				c.a = 1f - Mathf.Clamp01(timer / fadeDuration);
				fadeImage.color = c;
				yield return null;
			}

			// Dọn dẹp tấm rèm
			Destroy(fadeImage.canvas.gameObject);
		}

		// Tự hủy object chứa script
		Destroy(this.gameObject);
	}
}
