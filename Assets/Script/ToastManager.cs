using UnityEngine;
using TMPro;
using System.Collections;

public class ToastManager : MonoBehaviour
{
	public static ToastManager instance;

	[Header("UI Elements")]
	public GameObject toastPanel;      
	public TextMeshProUGUI toastText;   
	public CanvasGroup canvasGroup;     
	public RectTransform rectTransform; 

	[Header("Animation Settings")]
	public float moveDistance = 150f;  
	public float duration = 1.5f;      

	private Vector2 startPosition;    
	private Coroutine currentAnim;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		// Lưu lại vị trí xuất phát mặc định trên màn hình
		if (rectTransform != null)
		{
			startPosition = rectTransform.anchoredPosition;
		}

		if (toastPanel != null) toastPanel.SetActive(false);
	}

	public void ShowToast(string message)
	{
		// Nếu đang có 1 thông báo khác đang trôi, dừng nó lại ngay để chạy thông báo mới
		if (currentAnim != null)
		{
			StopCoroutine(currentAnim);
		}

		currentAnim = StartCoroutine(AnimateToast(message));
	}

	private IEnumerator AnimateToast(string message)
	{
		Debug.Log("Chạy animation");
		// 1. Chuẩn bị dữ liệu và hiện ngay lập tức
		toastText.text = message;
		toastPanel.SetActive(true);
		canvasGroup.alpha = 1f; // Sáng rõ 100%
		rectTransform.anchoredPosition = startPosition; // Nằm im tại điểm xuất phát

		Vector2 endPosition = startPosition + new Vector2(0, moveDistance); // Điểm đến khi trôi lên

		// --- CÀI ĐẶT THỜI GIAN ---
		float stayTime = 1.2f;  // Thời gian đứng im chình ình trên màn hình để đọc
		float animTime = 0.5f;  // Thời gian diễn ra hiệu ứng "bay đi" (Vừa trôi lên vừa mờ)

		// ==========================================
		// GIAI ĐOẠN 1: ĐỨNG IM CHỜ NGƯỜI CHƠI ĐỌC
		// ==========================================
		yield return new WaitForSeconds(stayTime);

		// ==========================================
		// GIAI ĐOẠN 2: VỪA TRÔI LÊN VỪA MỜ DẦN VÀ BIẾN MẤT
		// ==========================================
		float elapsed = 0f;
		while (elapsed < animTime)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / animTime;

			// Công thức Ease-In: Trôi lên từ từ, sau đó tăng tốc độ bay đi
			float easeT = t * t * t;

			// Gộp cả 2 hiệu ứng vào chung 1 vòng lặp
			rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easeT);
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, easeT); // Alpha giảm từ 1 về 0

			yield return null;
		}

		// 3. Kết thúc: Dọn dẹp để chuẩn bị cho lần gọi tiếp theo
		rectTransform.anchoredPosition = startPosition; // Trả về chỗ cũ
		canvasGroup.alpha = 0f;
		toastPanel.SetActive(false);
	}
}