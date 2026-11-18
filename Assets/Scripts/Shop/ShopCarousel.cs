using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // --- PHẢI THÊM NAMESPACE DOTWEEN ---

public class ShopCarousel : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
	[Header("Thành phần UI")]
	public ScrollRect scrollRect;
	public RectTransform[] cardVisuals; // Các Card_Visual (Cái Image)
	public RectTransform[] cardSlots;   // --- MỚI: Kéo các object SLOT tương ứng vào đây ---

	[Header("Cài đặt Animation")]
	public float minScale = 0.8f;
	public float maxScale = 1.2f;
	public float snapSpeed = 0.3f;   // Thời gian DOTween snap (giây), nhỏ hơn = nhanh hơn
	public Ease animationEase = Ease.OutBack; // Hiệu ứng nảy nhẹ khi dừng

	private float centerPointX;
	private bool isDragging = false;
	private int closestCardIndex = 0;

	void Start()
	{
		// Lấy tâm X của ScrollView (tính theo tọa độ World)
		centerPointX = scrollRect.GetComponent<RectTransform>().position.x;

		// Cập nhật scale lần đầu tiên ngay khi start
		UpdateCardsVisuals(true); // true = cập nhật ngay lập tức, không tween

		// Đăng ký sự kiện: Mỗi khi ScrollRect trượt, gọi hàm cập nhật visual
		scrollRect.onValueChanged.AddListener((Vector2 val) => {
			UpdateCardsVisuals(false); // false = dùng DOTween để mượt
		});
	}

	void OnDestroy()
	{
		// Hủy đăng ký sự kiện khi object bị xóa để tránh lỗi bộ nhớ
		scrollRect.onValueChanged.RemoveAllListeners();
	}

	// --- HÀM MA THUẬT: CẬP NHẬT VISUAL VÀ SORTING ORDER MƯỢT MÀ ---
	// instantUpdate: Nếu true, set scale ngay lập tức (dùng cho Start). Nếu false, dùng DOTween.
	private void UpdateCardsVisuals(bool instantUpdate)
	{
		float effectRadius = scrollRect.GetComponent<RectTransform>().rect.width * 0.7f; // Phạm vi ảnh hưởng

		for (int i = 0; i < cardVisuals.Length; i++)
		{
			// Khoảng cách từ tâm SLOT đến tâm ScrollView (dùng Slot để tính chính xác hơn)
			float distance = Mathf.Abs(centerPointX - cardSlots[i].position.x);

			// Tính tỷ lệ Scale mục tiêu
			float targetScale = Mathf.Lerp(maxScale, minScale, distance / effectRadius);
			targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
			Vector3 targetScaleVec = new Vector3(targetScale, targetScale, 1f);

			// --- THỰC HIỆN ANIMATION SCALE ---
			if (instantUpdate)
			{
				cardVisuals[i].localScale = targetScaleVec;
			}
			else
			{
				// Sử dụng DOTween: Dùng DOComplete để hủy animation cũ nếu đang chạy, tránh xung đột
				// Thời gian tween ngắn (0.1s) kết hợp với OnValueChanged tạo độ mượt tuyệt đối
				cardVisuals[i].DOComplete();
				cardVisuals[i].DOScale(targetScaleVec, 0.1f).SetEase(Ease.OutSine);
			}

			// --- XỬ LÝ LỚP ĐÈ (TÁI SỬ DỤNG THUẬT TOÁN KIM TỰ THÁP BƯỚC TRƯỚC) ---
			Canvas cardCanvas = cardVisuals[i].GetComponent<Canvas>();
			if (cardCanvas != null)
			{
				cardCanvas.overrideSorting = true;
				// Chúng ta cần tìm closestCardIndex để tính sorting order
				// (Trong onValueChanged, việc tìm closestIndex mỗi frame là chấp nhận được)
				if (distance < Mathf.Abs(centerPointX - cardSlots[closestCardIndex].position.x))
				{
					closestCardIndex = i;
				}
			}
		}

		// Cập nhật Sorting Order dựa trên closestCardIndex mới tìm được
		for (int i = 0; i < cardVisuals.Length; i++)
		{
			Canvas cardCanvas = cardVisuals[i].GetComponent<Canvas>();
			if (cardCanvas != null)
			{
				cardCanvas.sortingOrder = cardVisuals.Length - Mathf.Abs(i - closestCardIndex);
			}
		}
	}

	// --- XỬ LÝ SNAP (HÚT VÀO GIỮA) BẰNG DOTWEEN CỰC MƯỢT ---
	public void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;
		// Nếu đang snap dở mà người dùng chạm vào -> Dừng snap ngay
		scrollRect.content.DOKill();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
		// Bắt đầu quy trình snap khi buông tay
		SnapToClosestCard();
	}

	private void SnapToClosestCard()
	{
		// Nếu quán tính vẫn còn lớn, đợi nó chậm lại mới hút
		if (Mathf.Abs(scrollRect.velocity.x) > 500f) return;

		scrollRect.velocity = Vector2.zero; // Dừng quán tính vật lý

		// Khoảng cách cần kéo Content để thẻ mục tiêu lọt vào giữa
		float distanceToCenter = centerPointX - cardSlots[closestCardIndex].position.x;
		Vector3 newContentPos = scrollRect.content.position;
		newContentPos.x += distanceToCenter;

		// --- SNAP DÙNG DOTWEEN VỚI EASE NẢY (OUTBACK) ---
		// Kill tween cũ của content để tránh xung đột
		scrollRect.content.DOKill();
		scrollRect.content.DOMoveX(newContentPos.x, snapSpeed).SetEase(animationEase);
	}
}