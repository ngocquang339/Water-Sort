using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public GameObject dailyReward;
	public GameObject darkBackground;
	[Header("Cài đặt Shop UI")]
	public RectTransform shopPanel;
	public float slideDuration = 0.3f; // Thời gian trượt 

	// Tọa độ điểm giấu và điểm hiện
	private Vector2 hiddenPos = new Vector2(-1500f, 862f); // Giấu bên trái
	private Vector2 centerPos = new Vector2(0f, 0f);     // Ngay giữa màn hình

	// Hàm này gắn vào nút bấm mở Shop
	void Start(){
		dailyReward.SetActive(true);
		darkBackground.SetActive(true);
	}
	public void OpenShop()
	{
		StopAllCoroutines(); // Dừng các hiệu ứng trượt cũ nếu đang chạy dở
		StartCoroutine(SlidePanel(shopPanel, centerPos, slideDuration));
	}

	// Hàm này gắn vào nút bấm tắt Shop (dấu X)
	public void CloseShop()
	{
		StopAllCoroutines();
		StartCoroutine(SlidePanel(shopPanel, hiddenPos, slideDuration));
	}

	// Cỗ máy di chuyển UI
	private IEnumerator SlidePanel(RectTransform panel, Vector2 targetPos, float duration)
	{
		Vector2 startPos = panel.anchoredPosition;
		float timePassed = 0f;

		while (timePassed < duration)
		{
			timePassed += Time.deltaTime;
			float percent = timePassed / duration;

			// Dùng hàm SmoothStep để hiệu ứng trượt có đà (nhanh ở giữa, chậm dần lúc dừng)
			float smoothPercent = Mathf.SmoothStep(0, 1, percent);

			panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothPercent);
			yield return null;
		}

		panel.anchoredPosition = targetPos; // Chốt vị trí cuối cùng
	}
}