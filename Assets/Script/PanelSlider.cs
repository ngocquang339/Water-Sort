using System.Collections;
using UnityEngine;

// Đảm bảo object gắn script này bắt buộc phải có RectTransform
[RequireComponent(typeof(RectTransform))]
public class PanelSlider : MonoBehaviour
{
	[Header("Position Settings")]
	public Vector2 hiddenPos = new Vector2(-1500f, 0f);
	public Vector2 centerPos = new Vector2(0f, 0f);

	[Header("Cài đặt Hiệu ứng")]
	public float slideDuration = 0.3f;

	private RectTransform rectTransform;
	private RankingManager rankingManager;
	private ShopManager shopManager;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	// Các hàm Public để script khác (như Shop hoặc Ranking) gọi tới
	public void OpenPanel()
	{
		// Bật panel lên (nếu đang bị tắt)
		gameObject.SetActive(true);
		StopAllCoroutines();
		StartCoroutine(SlideRoutine(centerPos));
	}

	public void ClosePanel()
	{
		// THÊM DÒNG NÀY: Nếu object đang tắt sẵn rồi thì bỏ qua luôn, không làm gì cả
		if (!gameObject.activeInHierarchy) return;
		StopAllCoroutines();
		StartCoroutine(SlideRoutine(hiddenPos));
	}

	// Cỗ máy di chuyển UI đã được đóng gói
	private IEnumerator SlideRoutine(Vector2 targetPos)
	{
		Vector2 startPos = rectTransform.anchoredPosition;
		float timePassed = 0f;

		while (timePassed < slideDuration)
		{
			timePassed += Time.deltaTime;
			float percent = timePassed / slideDuration;
			float smoothPercent = Mathf.SmoothStep(0, 1, percent);

			rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothPercent);
			yield return null;
		}

		rectTransform.anchoredPosition = targetPos;

		// Tự động tắt object đi nếu trượt ra ngoài màn hình để tiết kiệm RAM
		if (targetPos == hiddenPos)
		{
			gameObject.SetActive(false);
		}
	}
}