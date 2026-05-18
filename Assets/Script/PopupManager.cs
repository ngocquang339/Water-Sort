using System.Collections;
using DG.Tweening;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
	public static PopupManager instance; // Tạo Singleton để gọi từ GameManager cho dễ

	[Header("Popup Out Of Help Settings")]
	public GameObject outOfHelpPopup;
	public RectTransform popupRect;
	public Button buyButton;

	[Header("Cài đặt Giá tiền cho từng loại")]
	[SerializeField] private int undoPrice = 20;
	[SerializeField] private int hintPrice = 20;
	[SerializeField] private int addBottlePrice = 20;

	[Header("Animation Settings")]
	[SerializeField] private float animDuration = 0.5f;

	[Header("UI Floating Text Settings")]
	public GameObject floatingTextPrefab;
	public RectTransform canvasRect;

	// Biến dùng để ghi nhớ người chơi đang thiếu cái gì
	private HelpType currentHelpType;
	private Coroutine currentAnimCoroutine;

	private void Awake()
	{
		if (instance == null) instance = this;
	}

	private void Start()
	{
		if (popupRect != null) popupRect.localScale = Vector3.zero;
		if (outOfHelpPopup != null) outOfHelpPopup.SetActive(false);
	}

	// --- CẬP NHẬT HÀM MỞ POPUP DÀNH CHO TỪNG LOẠI ---
	public void ShowOutOfHelpPopup(HelpType type)
	{
		currentHelpType = type; // Ghi nhớ loại trợ giúp đang thiếu

		// (Tùy chọn bổ sung sau) Bạn có thể đổi chữ hiển thị giá tiền trên nút Mua ở đây dựa vào giá của từng loại
		int price = GetPriceForType(type);
		Debug.Log($"Mở popup mua {type} với giá {price} xu");

		if (outOfHelpPopup == null || popupRect == null) return;
		if (currentAnimCoroutine != null) StopCoroutine(currentAnimCoroutine);

		outOfHelpPopup.SetActive(true);
		popupRect.localScale = Vector3.zero;
		currentAnimCoroutine = StartCoroutine(JuicyAnimationRoutine(Vector3.zero, Vector3.one));
	}

	// Hàm phụ để lấy giá tiền nhanh dựa trên loại trợ giúp
	private int GetPriceForType(HelpType type)
	{
		switch (type)
		{
			case HelpType.Undo: return undoPrice;
			case HelpType.Hint: return hintPrice;
			case HelpType.AddBottle: return addBottlePrice;
			default: return 0;
		}
	}

	// --- CẬP NHẬT LUỒNG XỬ LÝ NÚT MUA ĐA NĂNG ---
	public void OnBuyButtonClicked()
	{
		int currentPrice = GetPriceForType(currentHelpType);

		// 1. Kiểm tra ví tiền với giá tiền động
		if (CoinManager.instance != null && CoinManager.instance.CanAfford(currentPrice))
		{
			// ĐỦ TIỀN -> Trừ tiền
			CoinManager.instance.SpendCoins(currentPrice);

			// Gọi sang GameManager để cộng đúng loại vật phẩm đã mua
			if (GameManager.instance != null)
			{
				GameManager.instance.AddHelpQuantity(currentHelpType, 1);
			}

			// Tắt bảng
			HideOutOfHelpPopup();
		}
		else
		{
			// THIẾU TIỀN -> Báo lỗi chữ bay và rung lắc
			SpawnFloatingTextAnimation();
			if (popupRect != null)
			{
				popupRect.DOShakePosition(0.3f, 10f, 10, 90, false, true);
			}
		}
	}

	public void HideOutOfHelpPopup()
	{
		if (outOfHelpPopup == null || popupRect == null) return;
		if (currentAnimCoroutine != null) StopCoroutine(currentAnimCoroutine);

		currentAnimCoroutine = StartCoroutine(JuicyAnimationRoutine(Vector3.one, Vector3.zero, () => {
			outOfHelpPopup.SetActive(false);
		}));
	}

	private IEnumerator JuicyAnimationRoutine(Vector3 startScale, Vector3 endScale, System.Action callback = null)
	{
		if (popupRect == null) yield break;
		float elapsed = 0f;
		while (elapsed < animDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / animDuration;
			float s = 1.70158f;
			float easeT = --t * t * ((s + 1f) * t + s) + 1f;
			popupRect.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, easeT);
			yield return null;
		}
		popupRect.transform.localScale = endScale;
		callback?.Invoke();
		currentAnimCoroutine = null;
	}

	private void SpawnFloatingTextAnimation()
	{
		if (floatingTextPrefab == null || canvasRect == null) return;
		GameObject go = Instantiate(floatingTextPrefab, canvasRect);
		RectTransform rt = go.GetComponent<RectTransform>();
		TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
		if (rt == null || tmp == null) { Destroy(go); return; }

		rt.anchoredPosition = new Vector2(0, 100f);
		tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1f);

		float duration = 2.0f;
		rt.DOAnchorPosY(rt.anchoredPosition.y + 200f, duration).SetEase(Ease.OutQuad);
		tmp.DOFade(0f, duration).SetEase(Ease.InQuad).OnComplete(() => {
			Destroy(go);
		});
	}
}