using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPackUI : MonoBehaviour
{
	[Header("UI Elements")]
	public TextMeshProUGUI packNameText;
	public Image backgroundImage;
	public TextMeshProUGUI priceText;
	public GameObject ribbon;
	public Image boxItem;

	[Header("Main Reward UI (Cục to bên trái)")]
	public Image mainRewardIcon;           // Sẽ kéo MainReward_Image vào đây
	public TextMeshProUGUI mainRewardAmount; // Sẽ kéo Amount_Text của nó vào đây

	[Header("Reward Spawning (Khay vật phẩm phụ)")]
	public Transform rewardContainer;
	public GameObject rewardItemPrefab;

	private ShopPackData myPackData;

	public void SetupPack(ShopPackData data)
	{
		myPackData = data;

		// 1. Đổi hình nền
		if (backgroundImage != null && data.packBackground != null)
		{
			backgroundImage.sprite = data.packBackground;
		}

		if(ribbon != null && !data.isPopular){
			ribbon.SetActive(false);
		}

		if (boxItem != null && !data.isPopular) {
			boxItem.color = new Color32(255, 238, 169, 255);
		}
		else{
			boxItem.color = new Color32(204, 69, 255, 255);
		}

		// 2. Cập nhật Text cơ bản
		packNameText.text = data.packName;
		priceText.text = data.priceString;

		// 3. SET UP PHẦN THƯỞNG CHÍNH (Bên trái)
		if (mainRewardIcon != null && data.mainReward.itemIcon != null)
		{
			mainRewardIcon.sprite = data.mainReward.itemIcon;
		}
		if (mainRewardAmount != null)
		{
			mainRewardAmount.text = data.mainReward.amount.ToString();
		}

		// 4. SINH RA PHẦN THƯỞNG PHỤ (Bên phải)
		foreach (PackReward reward in data.rewards)
		{
			GameObject itemGO = Instantiate(rewardItemPrefab, rewardContainer);
			itemGO.transform.localScale = Vector3.one;

			// 1. Cái khuôn (itemGO) bây giờ chính là Icon, nên ta lấy thẳng Image từ nó
			Image iconImg = itemGO.GetComponent<Image>();

			// 2. Tìm object con chứa chữ có dấu gạch dưới như thiết kế của bạn
			TextMeshProUGUI amountTxt = itemGO.transform.Find("Amount_Text").GetComponent<TextMeshProUGUI>();

			iconImg.sprite = reward.itemIcon;
			amountTxt.text = reward.amount.ToString(); // Thêm "x" vào trước nếu muốn: "x" + reward.amount
		}
	}

	public void OnBuyButtonClicked()
	{
		Debug.Log("Người chơi bấm mua gói: " + myPackData.packName);
	}
}