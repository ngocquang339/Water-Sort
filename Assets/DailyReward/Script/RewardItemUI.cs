using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
	public Image iconImage;
	public TextMeshProUGUI amountText;

	// Hàm nhận data để đắp vào UI
	public void SetupReward(Sprite icon, int amount)
	{
		if (iconImage != null) iconImage.sprite = icon;
		if (amountText != null) amountText.text = "x" + amount;
	}
}