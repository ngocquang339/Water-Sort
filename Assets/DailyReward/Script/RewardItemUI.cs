using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
	public Image rewardImage;
	public Sprite iconImage;
	public TextMeshProUGUI amountText;

	// Hàm nhận data để đắp vào UI
	public void SetupReward(Sprite image, Sprite icon, int amount)
	{
		if (rewardImage != null) rewardImage.sprite = image;
		if (iconImage != null) iconImage = icon;
		if (amountText != null) amountText.text = "x" + amount;
	}
}