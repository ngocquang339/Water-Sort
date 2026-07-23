using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
	public static LoadingManager Instance;
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

	private string playerName;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			// Nếu bị trùng lặp khi load lại
			Destroy(gameObject);
		}
	}

	void Start()
	{
		fillColor.fillAmount = 0f;
		if(loading_Container != null){
			loading_Container.SetActive(false);
		}
		playerName = PlayerPrefs.GetString("Player_Username", "");
		StartCoroutine(LoadSceneWithPause());
	}

	IEnumerator LoadSceneWithPause()
	{
		yield return new WaitForSeconds(timeToActive);
		if (loading_Container != null)
		{
			loading_Container.SetActive(true);
		}
		AsyncOperation operation;
		if (playerName == ""){
			operation = SceneManager.LoadSceneAsync(sceneToLoad);
		}
		else{
			operation = SceneManager.LoadSceneAsync("MainScene");
		}
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

		// Thêm "LoadingSceneSmooth.Instance." vào ngay trước chữ StartCoroutine
		LoadingSceneSmooth.Instance.StartCoroutine(LoadingSceneSmooth.Instance.TransitionWithOperation(operation));
	}

	// Hàm phụ để cập nhật UI cho gọn code
	private void UpdateUI(float value)
	{
		fillColor.fillAmount = value;
	}
}