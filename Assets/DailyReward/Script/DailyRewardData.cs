using UnityEngine;

// 1. TẠO THÊM CLASS NÀY ĐỂ GÓI GỌN NHIỀU VẬT PHẨM VÀO 1 NGÀY
[System.Serializable]
public class DayRewardConfig
{
	[Tooltip("Ngày 1-6 nhập Size = 1. Riêng Ngày 7 nhập Size = 3")]
	public RewardItem[] items;
}

// 1. TẠO CẤU TRÚC CHO 1 CÁI RƯƠNG
[System.Serializable]
public class MilestoneChestConfig
{
	[Header("Vị trí hiển thị trên thanh UI (Từ 0.0 đến 1.0)")]
	[Range(0f, 1f)]
	public float visualAnchor; // <--- THÊM DÒNG NÀY

	[Header("Điều kiện mở rương")]
	[Tooltip("Số ngày cần đạt để mở. Ví dụ: 8, 15, 22, 30")]
	public int requiredDays;

	[Header("Phần thưởng trong rương")]
	[Tooltip("Khai báo các vật phẩm sẽ văng ra khi mở rương")]
	public RewardItem[] rewards;

	[Header("Hình ảnh rương (Tùy chọn để tráo ảnh)")]
	public Sprite chestClosedIcon;
	public Sprite chestOpenedIcon;
}

[CreateAssetMenu(fileName = "DailyRewardData", menuName = "Scriptable Objects/DailyRewardData")]
public class DailyRewardData : ScriptableObject
{
	[Header("Danh sách 7 ngày điểm danh")]
	// 2. Sửa mảng RewardItem thành mảng DayRewardConfig
	public DayRewardConfig[] days;

	[Header("Danh sách Rương Mốc Chuỗi (Vòng lặp dài)")]
	public MilestoneChestConfig[] milestoneChests;
}
