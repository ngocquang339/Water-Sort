using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DaySlotState
{
	Locked,
	Available,
	Claimed
}

public class DaySlotUI : MonoBehaviour
{
	[Header("UI Elements")]
	public TextMeshProUGUI dayText;
	public Button slotButton;
	public Image headerBanner;
	public TextMeshProUGUI[] amountTexts;
	public Image[] rewardIcons;

	[Header("Trạng thái Overlay")]
	public GameObject checkmarkIcon;

	public void updateSlotUI(int dayNumber, DayRewardConfig config, DaySlotState state)
	{
		dayText.text = "Day " + dayNumber.ToString();

		// Dùng vòng lặp để bật/tắt và gán Data cho các Icon trong Slot này
		for (int i = 0; i < rewardIcons.Length; i++)
		{
			// Nếu data ngày hôm nay có chứa phần thưởng ở vị trí thứ i
			if (i < config.items.Length)
			{
				rewardIcons[i].gameObject.SetActive(true);
				amountTexts[i].gameObject.SetActive(true);

				rewardIcons[i].sprite = config.items[i].image;
				amountTexts[i].text = "x" + config.items[i].amount.ToString();

				// Làm mờ icon nếu chưa đến ngày nhận
				rewardIcons[i].color = (state == DaySlotState.Locked || state == DaySlotState.Claimed) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
			}
			else
			{
				// Nếu Prefab có 3 chỗ mà Data chỉ có 1 quà (Ngày 1-6), thì tắt 2 chỗ thừa đi
				rewardIcons[i].gameObject.SetActive(false);
				amountTexts[i].gameObject.SetActive(false);
			}
		}

		switch (state)
		{
			case DaySlotState.Locked:
				checkmarkIcon.SetActive(false);
				slotButton.interactable = false;
				break;
			case DaySlotState.Available:
				checkmarkIcon.SetActive(false);
				headerBanner.color = new Color32(60, 200, 100, 255);
				slotButton.interactable = true;
				break;
			case DaySlotState.Claimed:
				checkmarkIcon.SetActive(true);
				headerBanner.color = new Color32(154, 78, 200, 255);
				slotButton.interactable = false;
				break;
		}
	}
}