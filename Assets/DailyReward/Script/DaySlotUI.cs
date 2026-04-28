using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DaySlotState{
    Locked,
    Available,
    Claimed
}
public class DaySlotUI : MonoBehaviour
{
	[Header("UI Elements")]
	public TextMeshProUGUI dayText;      
	public TextMeshProUGUI amountText;  
	public Image rewardIcon;
	public Button slotButton;

	[Header("Trạng thái Overlay")]
	public GameObject checkmarkIcon;     
	public GameObject lockIcon;        

	public void updateSlotUI(int dayNumber, RewardItem itemData, DaySlotState state){
		dayText.text = "Day " + dayNumber.ToString();
		amountText.text = "x" + itemData.amount.ToString();
		rewardIcon.sprite = itemData.image;

		switch (state) {
			case DaySlotState.Locked:
				checkmarkIcon.SetActive(false);
				lockIcon.SetActive(true);
				rewardIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
				slotButton.interactable = false;
				break;
			case DaySlotState.Available:
				checkmarkIcon.SetActive(false);
				lockIcon.SetActive(false);
				rewardIcon.color = Color.white;
				slotButton.interactable = true;
				break;

			case DaySlotState.Claimed:
				checkmarkIcon.SetActive(true);
				lockIcon.SetActive(false);
				rewardIcon.color = Color.white;
				slotButton.interactable = false;
				break;
		}
	}
}
