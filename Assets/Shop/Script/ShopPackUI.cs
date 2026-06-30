using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPackUI : MonoBehaviour
{
	[Header("UI Elements")]
	public TextMeshProUGUI amountText;    // Chữ số lượng ở trên cùng (VD: 20)
	public Image centerIcon;              // Cái ảnh ở giữa (VD: Nút Undo)
	public TextMeshProUGUI priceText;     // Chữ giá tiền ở dưới nút (VD: 100)
	public Image priceIcon;
	private ShopPackData myPackData;

	public void SetupPack(ShopPackData data)
	{
		myPackData = data;

		// 1. Cập nhật Số lượng (Lấy từ mainReward)
		if (amountText != null)
		{
			amountText.text = data.mainReward.amount.ToString();
		}

		// 2. Cập nhật Ảnh Item
		if (centerIcon != null && data.mainReward.itemIcon != null)
		{
			centerIcon.sprite = data.mainReward.itemIcon;
			
		}

		// 3. Cập nhật Giá tiền
		if (priceText != null)
		{
			priceText.text = data.price.ToString();
		}

		if (priceIcon != null) {
			priceIcon.sprite = data.priceIcon;
		}
	}

	public void OnBuyButtonClicked()
	{
		ShopManager shopManager = FindFirstObjectByType<ShopManager>();
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayButtonClick(); // Hoặc hàm phát âm thanh tương ứng của bạn
		}

		if (CurrencyManager.Instance.CanAfford(myPackData.price, myPackData.currencyType))
		{
			CurrencyManager.Instance.SpendCurrency(myPackData.price, myPackData.currencyType);
			CurrencyManager.Instance.AddCurrency(myPackData.mainReward.amount, myPackData.mainReward.itemType);
		}
		else
		{
			Debug.Log("Chạy animation toast");
			// Truyền Panel Đỏ và câu thông báo vào
			ToastManager.instance.ShowToast(shopManager.outOfCurrency_Panel, shopManager.outOfCurrnecyText.text);
		}
	}
}