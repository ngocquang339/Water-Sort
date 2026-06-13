using UnityEngine;
using System.Collections;

public class BottomNavManager : MonoBehaviour
{
	[Header("UI Elements")]
	public RectTransform indicator; // Kéo object Indicator vào đây

	[Header("Settings")]
	public float slideDuration = 0.3f; // Thời gian trượt (giây)

	[Header("Danh sách các Quản lý")]
	public ShopManager shopManager;
	public RankingManager rankingManager;

	private Coroutine moveCoroutine;

	// Hàm này sẽ được gọi khi bấm vào 1 trong 3 nút
	public void OnClickTab(RectTransform targetButton)
	{	
		Debug.Log("Thực hiện hiệu ứng chuyển tab");
		// Dừng hiệu ứng cũ nếu đang trượt lở dở
		if (moveCoroutine != null) StopCoroutine(moveCoroutine);

		// Bắt đầu trượt tới tọa độ X của nút vừa bấm
		moveCoroutine = StartCoroutine(MoveIndicator(targetButton.anchoredPosition.x));
	}

	private IEnumerator MoveIndicator(float targetX)
	{
		float elapsedTime = 0f;
		float startX = indicator.anchoredPosition.x;

		while (elapsedTime < slideDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / slideDuration;

			// Công thức Ease Out Cubic cho cảm giác trượt mượt mà
			float easeT = 1f - Mathf.Pow(1f - t, 3f);

			// Cập nhật vị trí X của Indicator
			Vector2 newPos = indicator.anchoredPosition;
			newPos.x = Mathf.Lerp(startX, targetX, easeT);
			indicator.anchoredPosition = newPos;

			yield return null;
		}

		// Chốt hạ vị trí chính xác khi kết thúc
		Vector2 finalPos = indicator.anchoredPosition;
		finalPos.x = targetX;
		indicator.anchoredPosition = finalPos;
	}
	// 1. GẮN VÀO NÚT HOME
	public void SelectHomeTab()
	{
		// Về sảnh thì ép tắt cả 2 bảng (Nếu chúng đang tắt sẵn thì hàm if ở Bước 1 sẽ đỡ cho ta)
		shopManager.ClickCloseShop();
		rankingManager.ClickCloseRanking();

		Debug.Log("Chuyển về màn hình chính");
	}

	// 2. GẮN VÀO NÚT SHOP
	public void SelectShopTab()
	{
		// Tắt Ranking (nếu đang mở) và Mở Shop
		rankingManager.ClickCloseRanking();
		shopManager.ClickOpenShop();

		Debug.Log("Chuyển sang Shop");
	}

	// 3. GẮN VÀO NÚT RANKING
	public void SelectRankingTab()
	{
		// Tắt Shop (nếu đang mở) và Mở Ranking
		shopManager.ClickCloseShop();
		rankingManager.ClickOpenRanking();

		Debug.Log("Chuyển sang Ranking");
	}

}