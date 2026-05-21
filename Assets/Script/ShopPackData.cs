using UnityEngine;

// 1. Định nghĩa các loại vật phẩm có thể có trong game của bạn
public enum RewardItemType { Coin,Diamond, Undo, Hint, AddBottle }

// 2. Cấu trúc của 1 món đồ trong gói quà
[System.Serializable]
public class PackReward
{
	public RewardItemType itemType; // Loại đồ (Vàng, Undo,...)
	public int amount;              // Số lượng (Ví dụ: 100, 5, 3)
	public Sprite itemIcon;         // Hình ảnh icon của vật phẩm đó
}

// 3. ScriptableObject: Nơi lưu trữ Data của cả 1 Gói (Pack)
[CreateAssetMenu(fileName = "New Shop Pack", menuName = "Shop/Pack Data")]
public class ShopPackData : ScriptableObject
{
	public string packName;        // Tên gói (VD: Gói Quà Người Mới)
	public string priceString;     // Giá tiền (VD: "VNđ 29000.00")

	[Header("Giao diện thay đổi")]
	public Sprite packBackground; // Kéo hình nền Vàng hoặc Xanh vào đây
	public bool isPopular;          // Tích true nếu muốn hiện mác "Popular!"

	[Header("Phần thưởng CHÍNH (Cục to bên trái)")]
	public PackReward mainReward; // Thêm biến này đứng riêng lẻ

	[Header("Danh sách vật phẩm")]
	public PackReward[] rewards;   // MẢNG CHỨA CÁC VẬT PHẨM
}