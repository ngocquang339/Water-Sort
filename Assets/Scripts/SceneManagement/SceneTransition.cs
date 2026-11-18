using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
	public static SceneTransition instance;

	[Header("UI Transition")]
	public Image fadeImage;
	public float fadeDuration = 0.5f;

	void Awake()
	{
		// Biến object này thành "Bất tử" để nó sống xuyên qua các màn chơi
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	// GỌI HÀM NÀY ĐỂ CHUYỂN MÀN
	public void LoadSceneSmooth(string sceneName)
	{
		StartCoroutine(FadeAndLoad(sceneName));
	}

	private IEnumerator FadeAndLoad(string sceneName)
	{
		// 1. Kéo rèm che màn hình (Fade Out)
		fadeImage.raycastTarget = true; // Chặn người chơi bấm bậy lúc đang load
		float timer = 0f;
		Color c = fadeImage.color;

		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;
			c.a = Mathf.Clamp01(timer / fadeDuration);
			fadeImage.color = c;
			yield return null;
		}

		// 2. Load màn mới ngầm ở background (chống giật)
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}

		// 3. Mở rèm ra (Fade In)
		timer = 0f;
		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;
			c.a = 1f - Mathf.Clamp01(timer / fadeDuration);
			fadeImage.color = c;
			yield return null;
		}

		fadeImage.raycastTarget = false; // Trả lại tương tác cho người chơi
	}
}