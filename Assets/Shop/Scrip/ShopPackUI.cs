using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPackUI : MonoBehaviour
{
	[Header("UI Elements")]
	public TextMeshProUGUI packNameText;
	public Image backgroundImage;
	public Image chestIconImage;
	public TextMeshProUGUI priceText;

	[Header("Reward Spawning")]
	public Transform rewardContainer;     // Kéo cái khay Horizontal Layout vào đây
	public GameObject rewardItemPrefab;   // Kéo cái RewardItem_Prefab nhỏ xíu vào đây

	private ShopPackData myPackData;

	// Hàm này sẽ được ShopManager gọi để bơm dữ liệu vào
	public void SetupPack(ShopPackData data)
	{
		myPackData = data;

		// Cập nhật thông tin cơ bản
		packNameText.text = data.packName;
		priceText.text = data.priceString;

		// SINH RA MẢNG VẬT PHẨM BẰNG VÒNG LẶP
		foreach (PackReward reward in data.rewards)
		{
			// Đẻ ra 1 ô vật phẩm và nhét vào cái khay Horizontal Layout
			GameObject itemGO = Instantiate(rewardItemPrefab, rewardContainer);

			// Tìm các thành phần UI bên trong ô vật phẩm đó để gán Icon và Số lượng
			Image iconImg = itemGO.transform.Find("Icon").GetComponent<Image>();
			TextMeshProUGUI amountTxt = itemGO.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();

			iconImg.sprite = reward.itemIcon;
			amountTxt.text = reward.amount.ToString();
		}
	}

	// Gắn hàm này vào sự kiện OnClick của nút Mua Tiền
	public void OnBuyButtonClicked()
	{
		Debug.Log("Người chơi bấm mua gói: " + myPackData.packName);
		// Sau này bạn gọi API thanh toán In-App Purchase ở đây
		// Mua thành công thì vòng lặp qua myPackData.rewards để cộng đồ vào GameManager
	}
}