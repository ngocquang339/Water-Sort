using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSelectorUI : MonoBehaviour
{
	[Header("UI Cờ và Trượt")]
	public RectTransform carouselRect;

	[Tooltip("Nhập Width của 1 cụm cờ + Spacing")]
	public float stepDistance = 235.9f;
	public float slideDuration = 0.25f;

	[Header("Nút Xác Nhận")]
	public GameObject confirmButton;

	private int currentIndex = 0;       // 0: Anh, 1: Nhật, 2: Việt Nam
	private int currentActiveIndex = 0;
	private int maxIndex = 2;

	private bool isSliding = false;

	IEnumerator Start()
	{
		confirmButton.SetActive(false);

		// 1. Chờ hệ thống Từ điển của Unity tải xong xuôi
		yield return LocalizationSettings.InitializationOperation;

		// 2. Tự động kiểm tra xem game hiện tại đang dùng ngôn ngữ gì
		var activeLocale = LocalizationSettings.SelectedLocale;
		var allLocales = LocalizationSettings.AvailableLocales.Locales;

		for (int i = 0; i < allLocales.Count; i++)
		{
			if (allLocales[i] == activeLocale)
			{
				currentIndex = i;
				currentActiveIndex = i;
				break;
			}
		}

		// 3. Nhường Unity 1 khung hình để dựng xong UI (Chống lỗi trượt lệch lúc mới mở)
		yield return new WaitForEndOfFrame();

		// 4. Toán học: Đẩy băng chuyền về đúng vị trí lá cờ đang được chọn
		float targetX = -(currentIndex - 1) * stepDistance;
		carouselRect.anchoredPosition = new Vector2(targetX, carouselRect.anchoredPosition.y);
	}

	public void SlideNext()
	{
		if (isSliding) return;
		currentIndex++;

		// Vòng tuần hoàn (Lướt Tới)
		if (currentIndex > maxIndex) currentIndex = 0;

		MoveToCurrentIndex();
	}

	public void SlidePrev()
	{
		if (isSliding) return;
		currentIndex--;

		// Vòng tuần hoàn (Lướt Lùi)
		if (currentIndex < 0) currentIndex = maxIndex;

		MoveToCurrentIndex();
	}

	private void MoveToCurrentIndex()
	{
		// Tính toán tọa độ X cần trượt tới
		float targetX = -(currentIndex - 1) * stepDistance;
		StartCoroutine(AnimateSlide(targetX));
		CheckConfirmButton();
	}

	private IEnumerator AnimateSlide(float targetX)
	{
		isSliding = true;
		Vector2 startPos = carouselRect.anchoredPosition;
		Vector2 targetPos = new Vector2(targetX, startPos.y);

		float timePassed = 0f;
		while (timePassed < slideDuration)
		{
			timePassed += Time.deltaTime;
			// Dùng SmoothStep để trượt mượt (nhanh ở giữa, chậm lúc dừng)
			carouselRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, timePassed / slideDuration));
			yield return null;
		}

		carouselRect.anchoredPosition = targetPos;
		isSliding = false;
	}

	private void CheckConfirmButton()
	{
		// Hiện nút Confirm nếu ngôn ngữ đang xem khác ngôn ngữ hệ thống đang dùng
		confirmButton.SetActive(currentIndex != currentActiveIndex);
	}

	public void ConfirmLanguage()
	{
		StartCoroutine(SetLocale(currentIndex));
	}

	private IEnumerator SetLocale(int localeIndex)
	{
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];

		currentActiveIndex = currentIndex;
		confirmButton.SetActive(false);
	}
}