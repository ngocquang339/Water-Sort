using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
	public static DailyRewardManager Instance { get; private set; }

	[Header("Dữ liệu phần thưởng")]
	public DailyRewardData rewardData;

	// Các biến trạng thái nội bộ
	private int currentStreak = 0;      // Đang ở chuỗi ngày thứ mấy (0 đến 6)
	private DateTime lastClaimTime;     // Lần cuối cùng bấm nhận quà là lúc nào?

	// Tên chìa khóa để lưu xuống bộ nhớ máy (PlayerPrefs)
	private const string STREAK_KEY = "DailyReward_Streak";
	private const string TIME_KEY = "DailyReward_LastClaimTime";

	void Awake()
	{
		// Khởi tạo Singleton
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		LoadData();
		CheckStreakReset();
	}

	// Lôi dữ liệu từ RAM điện thoại lên khi vừa mở game
	private void LoadData()
	{
		currentStreak = PlayerPrefs.GetInt(STREAK_KEY, 0);

		string timeStr = PlayerPrefs.GetString(TIME_KEY, string.Empty);
		if (string.IsNullOrEmpty(timeStr))
		{
			lastClaimTime = DateTime.MinValue; // Chưa từng nhận quà bao giờ
		}
		else
		{
			lastClaimTime = DateTime.Parse(timeStr); // Dịch chuỗi chữ thành ngày tháng
		}
	}

	// Kiểm tra xem người chơi có bị đứt chuỗi đăng nhập không
	private void CheckStreakReset()
	{
		if (lastClaimTime != DateTime.MinValue)
		{
			// Tính khoảng cách giữa ngày hôm nay và ngày nhận cuối cùng
			TimeSpan timePassed = DateTime.Now.Date - lastClaimTime.Date;

			// Nếu bỏ lỡ lớn hơn 1 ngày (ví dụ qua 2 ngày mới vào lại) -> Phạt reset về Ngày 1
			// (Nếu game của bạn thuộc dạng Casual thân thiện, bạn có thể xóa cụm if này đi)
			if (timePassed.Days > 1)
			{
				currentStreak = 0;
				SaveData();
			}
		}
	}

	// Hàm dùng để UI hỏi: "Hôm nay đã được nhận quà chưa sếp?"
	public bool CanClaimToday()
	{
		if (lastClaimTime == DateTime.MinValue) return true; // Nick mới, cho nhận luôn

		TimeSpan timePassed = DateTime.Now.Date - lastClaimTime.Date;
		return timePassed.Days >= 1; // Chỉ cần qua ngày mới (đổi ngày) là được nhận
	}

	// Hàm để UI hỏi: "Sếp ơi, ô số 'dayIndex' vẽ trạng thái gì bây giờ?"
	public DaySlotState GetStateForDay(int dayIndex)
	{
		// Những ngày đã qua
		if (dayIndex < currentStreak) return DaySlotState.Claimed;

		// Chính là ngày hôm nay
		if (dayIndex == currentStreak)
		{
			return CanClaimToday() ? DaySlotState.Available : DaySlotState.Locked;
		}

		// Những ngày ở tương lai
		return DaySlotState.Locked;
	}

	// Hàm QUAN TRỌNG NHẤT: Thực hiện hành động trao quà khi nút được bấm
	public void ClaimTodayReward()
	{
		if (!CanClaimToday())
		{
			Debug.LogWarning("Hôm nay bạn đã nhận quà rồi, đừng gian lận!");
			return;
		}

		// 1. Lấy thông tin gói quà hiện tại từ kho dữ liệu
		RewardItem item = rewardData.rewards[currentStreak];

		// 2. Trao thưởng (TODO: Sau này bạn gọi hàm cộng Vàng/Gem ở đây)
		Debug.Log($"TING TING! Bạn vừa nhận được: {item.amount} {item.rewardType}");

		// 3. Cập nhật lại lịch và đẩy sang ngày tiếp theo
		lastClaimTime = DateTime.Now;
		currentStreak++;

		// Nếu nhận đủ 7 ngày, quay lại vạch xuất phát (Ngày 1)
		if (currentStreak >= rewardData.rewards.Length)
		{
			currentStreak = 0;
		}

		// 4. Lưu chết vào máy để tắt game đi không bị mất
		SaveData();
	}

	// Ghi dữ liệu xuống bộ nhớ điện thoại/máy tính
	private void SaveData()
	{
		PlayerPrefs.SetInt(STREAK_KEY, currentStreak);
		PlayerPrefs.SetString(TIME_KEY, lastClaimTime.ToString());
		PlayerPrefs.Save(); // Ép hệ thống lưu ngay lập tức
	}
}