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
	public string sceneToLoad = "MainScene";
	public float fillSpeed = 1.2f;

	[Header("Cài đặt Fake Load (Dừng hình)")]
	[Range(0.5f, 0.95f)]
	public float pausePoint = 0.85f; // Điểm dừng (0.85 tương đương 85%)
	public float pauseDuration = 2.0f; // Thời gian dừng (2 giây)
	public float timeToActive = 2f;
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
		if (loading_Container != null) {
			loading_Container .SetActive(true);
		}

		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
		operation.allowSceneActivation = false; // Tạm khóa không cho qua bài

		float currentFill = 0f;

		// ==========================================
		// GIAI ĐOẠN 1: Chạy mượt đến điểm dừng (85%)
		// ==========================================
		while (currentFill < pausePoint)
		{
			// Lấy tiến trình thật của máy, nhưng bị "khóa đỉnh" ở mức pausePoint
			float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
			float target = Mathf.Min(realProgress, pausePoint);

			currentFill = Mathf.MoveTowards(currentFill, target, fillSpeed * Time.deltaTime);
			UpdateUI(currentFill);

			yield return null;
		}

		// ==========================================
		// GIAI ĐOẠN 2: Nghỉ ngơi giải lao 2 giây
		// ==========================================
		// Trong lúc thanh UI đang đứng im khè người chơi, 
		// thì hệ thống ngầm vẫn đang tiếp tục load phần còn lại cho xong.
		yield return new WaitForSeconds(pauseDuration);

		// ==========================================
		// GIAI ĐOẠN 3: Chạy tốc biến lên 100%
		// ==========================================
		while (currentFill < 1f)
		{
			// Lúc này tháo khóa chặn, cho phép target vọt lên tận 1.0
			float target = Mathf.Clamp01(operation.progress / 0.9f);

			currentFill = Mathf.MoveTowards(currentFill, target, fillSpeed * Time.deltaTime);
			UpdateUI(currentFill);

			// Đầy bình thì qua bài!
			if (currentFill >= 1f)
			{
				operation.allowSceneActivation = true;
			}

			yield return null;
		}
	}

	// Hàm phụ để cập nhật UI cho gọn code
	private void UpdateUI(float value)
	{
		fillColor.fillAmount = value;
	}
}