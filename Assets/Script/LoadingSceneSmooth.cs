using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneSmooth : MonoBehaviour
{
	public static LoadingSceneSmooth Instance;

	[Header("Hiệu ứng chuyển cảnh")]
	public Image fadeImage;
	public float fadeDuration = 0.5f;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			// Bất tử hóa cả cục script và Canvas chứa rèm đen
			DontDestroyOnLoad(gameObject);
			if (fadeImage != null) DontDestroyOnLoad(fadeImage.canvas.gameObject);
		}
		else
		{
			// Nếu bị trùng lặp khi load lại màn, hủy script mới và dọn luôn cái Canvas rác của nó
			Destroy(gameObject);
			if (fadeImage != null && fadeImage.canvas != null) Destroy(fadeImage.canvas.gameObject);
			return;
		}
	}
	// HÀM 1: DÙNG CHO CÁC NÚT BẤM (TỰ LOAD NGẦM, TỰ CHUYỂN)
	public IEnumerator LoadSceneSmooth(string sceneName)
	{
		yield return StartCoroutine(FadeOut());

		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
		operation.allowSceneActivation = false;

		while (operation.progress < 0.9f) yield return null;

		operation.allowSceneActivation = true;
		while (!operation.isDone) yield return null;

		yield return StartCoroutine(FadeIn());
	}

	// HÀM 2: DÙNG RIÊNG CHO LOADING_MANAGER (Chỉ kéo rèm và mở khóa)
	public IEnumerator TransitionWithOperation(AsyncOperation pendingOperation)
	{
		yield return StartCoroutine(FadeOut());

		// Mở khóa cái tiến trình mà LoadingManager đã load sẵn từ trước
		pendingOperation.allowSceneActivation = true;
		while (!pendingOperation.isDone) yield return null;

		yield return StartCoroutine(FadeIn());
	}

	// ========================================================
	// CÁC HÀM PHỤ TRỢ FADE ĐỂ DÙNG CHUNG CHO GỌN CODE
	// ========================================================
	private IEnumerator FadeOut()
	{
		if (fadeImage != null)
		{
			fadeImage.gameObject.SetActive(true);
			fadeImage.raycastTarget = true;
			float timer = 0f;
			Color c = fadeImage.color;
			while (timer < fadeDuration)
			{
				timer += Time.deltaTime;
				c.a = Mathf.Clamp01(timer / fadeDuration);
				fadeImage.color = c;
				yield return null;
			}
		}
	}

	private IEnumerator FadeIn()
	{
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
			fadeImage.raycastTarget = false;

			// [QUAN TRỌNG] Đừng dùng Destroy! Tắt nó đi để dành cho lần chuyển scene tiếp theo
			fadeImage.gameObject.SetActive(false);
		}
	}
}