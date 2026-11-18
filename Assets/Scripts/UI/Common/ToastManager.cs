using UnityEngine;
using TMPro;
using System.Collections;

public class ToastManager : MonoBehaviour
{
	public static ToastManager instance;

	[Header("Animation Settings")]
	public float moveDistance = 150f;
	public float duration = 1.5f;

	void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	// 🌟 HÀM MỚI: Nhận vào ĐÚNG cái Panel bạn muốn và Câu chữ bạn muốn
	public void ShowToast(GameObject targetPanel, string message)
	{
		// Tắt mọi animation đang chạy (chống lỗi bấm liên tục bị bay tít lên trời)
		StopAllCoroutines();
		StartCoroutine(AnimateToast(targetPanel, message));
	}

	private IEnumerator AnimateToast(GameObject targetPanel, string message)
	{
		// Tự động lấy các "linh kiện" từ Panel bạn truyền vào
		CanvasGroup canvasGroup = targetPanel.GetComponent<CanvasGroup>();
		RectTransform rectTransform = targetPanel.GetComponent<RectTransform>();
		TextMeshProUGUI toastText = targetPanel.GetComponentInChildren<TextMeshProUGUI>();

		// Nếu Panel bị thiếu CanvasGroup thì code tự động thêm vào để không bị lỗi
		if (canvasGroup == null) canvasGroup = targetPanel.AddComponent<CanvasGroup>();

		// 1. Chuẩn bị dữ liệu
		toastText.text = message;
		targetPanel.SetActive(true);
		canvasGroup.alpha = 1f;

		// Ghi nhớ vị trí gốc của chính cái Panel đó
		Vector2 startPosition = rectTransform.anchoredPosition;
		Vector2 endPosition = startPosition + new Vector2(0, moveDistance);

		float stayTime = 1.2f;
		float animTime = 0.5f;

		// GIAI ĐOẠN 1: CHỜ NGƯỜI CHƠI ĐỌC
		yield return new WaitForSeconds(stayTime);

		// GIAI ĐOẠN 2: TRÔI LÊN VÀ MỜ DẦN
		float elapsed = 0f;
		while (elapsed < animTime)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / animTime;
			float easeT = t * t * t;

			rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easeT);
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, easeT);

			yield return null;
		}

		// 3. Dọn dẹp
		rectTransform.anchoredPosition = startPosition;
		canvasGroup.alpha = 0f;
		targetPanel.SetActive(false);
	}
}