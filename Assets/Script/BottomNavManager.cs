using UnityEngine;
using System.Collections;

public class BottomNavManager : MonoBehaviour
{
	[Header("UI Elements")]
	public RectTransform indicator; // Kéo object Indicator vào đây

	[Header("Settings")]
	public float slideDuration = 0.3f; // Thời gian trượt (giây)

	private Coroutine moveCoroutine;

	// Hàm này sẽ được gọi khi bấm vào 1 trong 3 nút
	public void OnClickTab(RectTransform targetButton)
	{
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
}