using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
	[Header("Giao diện UI")]
	public Image fillColor;
	public GameObject loading_Container;

	[Header("Cài đặt Load")]
	public string sceneToLoad = "SetName_Scene";
	public float fillSpeed = 1.2f;

	[Header("Cài đặt Fake Load (Dừng hình)")]
	[Range(0.5f, 0.95f)]
	public float pausePoint = 0.85f; // Điểm dừng (0.85 tương đương 85%)
	public float pauseDuration = 2.0f; // Thời gian dừng (2 giây)
	public float timeToActive = 2f;

	[Header("Hiệu ứng chuyển cảnh")]
	public Image fadeImage;
	public float fadeDuration = 0.5f;
	void Start()
	{
		fillColor.fillAmount = 0f;
		if(loading_Container != null){
			loading_Container.SetActive(false);
		}
		StartCoroutine(LoadSceneWithPause());
	}

	IEnumerator LoadSceneWithPause()
	{
		yield return new WaitForSeconds(timeToActive);
		if (loading_Container != null)
		{
			loading_Container.SetActive(true);
		}

		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
		operation.allowSceneActivation = false; // Tạm khóa không cho qua bài

		float currentFill = 0f;

		// GIAI ĐOẠN 1: Chạy mượt đến điểm dừng (85%)
		while (currentFill < pausePoint)
		{
			float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
			float target = Mathf.Min(realProgress, pausePoint);

			currentFill = Mathf.MoveTowards(currentFill, target, fillSpeed * Time.deltaTime);
			UpdateUI(currentFill);
			yield return null;
		}

		// GIAI ĐOẠN 2: Nghỉ ngơi giải lao
		yield return new WaitForSeconds(pauseDuration);

		// GIAI ĐOẠN 3: Chạy tốc biến lên 100%
		while (currentFill < 1f)
		{
			float target = Mathf.Clamp01(operation.progress / 0.9f);
			currentFill = Mathf.MoveTowards(currentFill, target, fillSpeed * Time.deltaTime);
			UpdateUI(currentFill);
			yield return null;
		}

		// =========================================================
		// GIAI ĐOẠN 4: KÉO RÈM ĐEN CHE MÀN HÌNH (FADE OUT)
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
				yield return null;
			}
		}

		// Ẩn thanh Loading cũ đi để không bị lôi sang màn mới
		if (loading_Container != null) loading_Container.SetActive(false);

		// [BÍ QUYẾT]: Bất tử hóa script này và tấm rèm đen để Coroutine không bị chết khi sang Scene mới!
		DontDestroyOnLoad(this.gameObject);
		if (fadeImage != null) DontDestroyOnLoad(fadeImage.canvas.gameObject);

		// Mở khóa cho sang bài
		operation.allowSceneActivation = true;

		// Chờ màn mới tải xong đồ đạc
		while (!operation.isDone)
		{
			yield return null;
		}

		// =========================================================
		// GIAI ĐOẠN 5: MỞ RÈM RA TẠI MÀN HÌNH MỚI (FADE IN)
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

		// Tự hủy object chứa script này vì đã hoàn thành nhiệm vụ
		Destroy(this.gameObject);
	}

	// Hàm phụ để cập nhật UI cho gọn code
	private void UpdateUI(float value)
	{
		fillColor.fillAmount = value;
	}
}