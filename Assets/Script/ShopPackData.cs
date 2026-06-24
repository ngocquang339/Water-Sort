using UnityEngine;
using UnityEngine.UI;
// 1. Định nghĩa danh mục trong Shop
public enum ShopCategory
{
	ResourcePack,
	CoinPack,
	GemsPack, // Thêm nếu sau này cần
	CashPack  // Thêm nếu sau này cần
}

public enum RewardItemType { Coin, Diamond, Undo, Hint, AddBottle }
public enum CurrencyType { Coin, Diamond}

[System.Serializable]
public class PackReward
{
	public RewardItemType itemType;
	public float amount;
	public Sprite itemIcon;
}

[CreateAssetMenu(fileName = "New Shop Pack", menuName = "Shop/Pack Data")]
public class ShopPackData : ScriptableObject
{
	[Header("Phân loại danh mục")]
	public ShopCategory category; 

	[Header("Thông tin cơ bản")]
	public float price;
	public Sprite priceIcon;
	public CurrencyType currencyType;
	[Header("Phần thưởng hiển thị")]
	// Vì UI mới của bạn mỗi thẻ chỉ hiện 1 món duy nhất, ta chỉ cần 1 biến này là đủ
	public PackReward mainReward;
}