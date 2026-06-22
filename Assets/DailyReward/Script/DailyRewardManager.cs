using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
public class DailyRewardManager : MonoBehaviour
{
	public static DailyRewardManager Instance { get; private set; }

	[Header("Dữ liệu phần thưởng")]
	public DailyRewardData rewardData;
	public GameObject rewardItemPrefab;

	[Header("UI Elements")]
	public GameObject dailyRewardPanel;
	public GameObject blockInputPanel;
	public GameObject darkBackground;
	public Button chestButton;
	private bool isClaimed = false;

	private static bool showDailyPopup = false;

	// Các biến trạng thái nội bộ
	private int currentStreak = 6;      // Đang ở chuỗi ngày thứ mấy (0 đến 6)
	private DateTime lastClaimTime;     // Lần cuối cùng bấm nhận quà là lúc nào?

	private const string STREAK_KEY = "DailyReward_Streak";
	private const string TIME_KEY = "DailyReward_LastClaimTime";

	public int totalClaimedDays = 0;
	private const string TOTAL_TIME_KEY = "DailyReward_TotalDays";

	public ChestOpeningController chestAnimationController;

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		LoadData();
		CheckStreakReset();
	}

	void Start()
	{
		// Vừa vào game là chạy luôn đếm ngược
		if (!showDailyPopup)
		{
			StartCoroutine(ShowDailyRewardDelay());
		}
	}

	IEnumerator ShowDailyRewardDelay()
	{
		if (blockInputPanel != null) blockInputPanel.SetActive(true);

		yield return new WaitForSeconds(1f);

		ShowDailyReward();
	}

	public void ShowDailyReward()
	{
		if (showDailyPopup) return;

		if (dailyRewardPanel != null) dailyRewardPanel.SetActive(true);
		if (darkBackground != null) darkBackground.SetActive(true);
		if (blockInputPanel != null) blockInputPanel.SetActive(false);

		showDailyPopup = true;
		Debug.Log("Đã tự động hiển thị Daily Reward!");
	}

	// Hàm để gắn vào nút X (đóng quà)
	public void CloseDailyReward()
	{
		if (dailyRewardPanel != null) dailyRewardPanel.SetActive(false);
		if (darkBackground != null) darkBackground.SetActive(false);
	}

	// Lôi dữ liệu từ RAM điện thoại lên khi vừa mở game
	private void LoadData()
	{
		currentStreak = PlayerPrefs.GetInt(STREAK_KEY, 6);
		totalClaimedDays = PlayerPrefs.GetInt(TOTAL_TIME_KEY, 6);

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

	public void ClaimTodayReward()
	{
		if (!CanClaimToday()) return;

		// Lấy toàn bộ Hộp Quà của ngày hôm nay
		DayRewardConfig todayConfig = rewardData.days[currentStreak];

		// Dùng vòng lặp để nhét toàn bộ đồ trong hộp vào ví
		foreach (RewardItem item in todayConfig.items)
		{
			if (item.rewardType == "Coin")
			{
				CurrencyManager.Instance.AddCoin(item.amount);
			}
			else if (item.rewardType == "Undo")
			{
				CurrencyManager.Instance.AddUndo(item.amount);
			}
			else if (item.rewardType == "Hint") {
				CurrencyManager.Instance.AddHint(item.amount);
			}
			else if (item.rewardType == "AddBottle") {
				CurrencyManager.Instance.AddBonusBottle(item.amount);
			}
			else
			{
				CurrencyManager.Instance.AddDiamond(item.amount);
			}
		}

		lastClaimTime = System.DateTime.Now;
		currentStreak++;
		if (currentStreak >= rewardData.days.Length) currentStreak = 0;
		totalClaimedDays++;
		// Tìm ngày to nhất của rương cuối cùng (Ví dụ: 30)

		int maxDays = rewardData.milestoneChests[rewardData.milestoneChests.Length - 1].requiredDays;


		// Nếu đầy thanh thì reset về 0 để chạy lại vòng lặp tháng mới

		if (totalClaimedDays > maxDays)
		{

			totalClaimedDays = 1;

		}
		SaveData();
	}

	private void SaveData()
	{
		PlayerPrefs.SetInt(STREAK_KEY, currentStreak);
		PlayerPrefs.SetString(TIME_KEY, lastClaimTime.ToString());
		PlayerPrefs.SetInt(TOTAL_TIME_KEY, totalClaimedDays);
		PlayerPrefs.Save();
	}

	public bool IsChestClaimed(int chestIndex)
	{
		if (chestIndex < 0 || chestIndex >= rewardData.milestoneChests.Length) return false;
		return isClaimed;
	}

	public void ClaimMilestoneChest(int chestIndex)
	{
		// 1. Kiểm tra tính hợp lệ
		if (chestIndex < 0 || chestIndex >= rewardData.milestoneChests.Length) return;

		// Chặn ngay nếu rương đã được nhận (Sử dụng IsChestClaimed)
		// Giả sử isClaimed là biến để check cho rương hiện tại, nên check IsChestClaimed(chestIndex) thì đúng hơn
		if (IsChestClaimed(chestIndex))
		{
			Debug.Log("Rương này đã được nhận rồi!");
			return;
		}

		MilestoneChestConfig chest = rewardData.milestoneChests[chestIndex];
		if (chest == null || chest.rewards == null) return;

		// 👇 ĐÃ SỬA: Xóa bỏ bước GenerateRewardUIs (tức xóa Step 2 cũ)

		// 👇 CHỖ NÀY CỰC QUAN TRỌNG: Sửa lại cách gọi hàm Animation
		// Thay vì truyền generatedRewardUIs (mảng UI), bạn truyền thẳng MẢNG DATA (chest.rewards) sang!
		chestAnimationController.PlayChestAnimation(
			chest.chestClosedIcon,
			chest.chestOpenedIcon,
			chest.rewards // <-- Truyền Data thuần túy sang đây
		);

		// 4. Thực hiện cộng quà vào kho đồ (Dữ nguyên logic này của bạn)
		foreach (var item in chest.rewards)
		{
			if (item != null)
			{
				if (item.rewardType == "Coin")
				{
					CurrencyManager.Instance.AddCoin(item.amount);
					Debug.Log($"Đã nhận được {item.amount} Coin từ rương cột mốc!");
				}
				else if (item.rewardType == "Undo")
				{
					CurrencyManager.Instance.AddUndo(item.amount);
					Debug.Log($"Đã nhận được {item.amount} Undo từ rương cột mốc!");
				}
				else if (item.rewardType == "Hint")
				{
					CurrencyManager.Instance.AddHint(item.amount);
					Debug.Log($"Đã nhận được {item.amount} Hint từ rương cột mốc!");
				}
				else if (item.rewardType == "AddBottle")
				{
					CurrencyManager.Instance.AddBonusBottle(item.amount);
					Debug.Log($"Đã nhận được {item.amount} Bonus Bottle từ rương cột mốc!");
				}
				else
				{
					CurrencyManager.Instance.AddDiamond(item.amount);
					Debug.Log($"Đã nhận được {item.amount} Diamond từ rương cột mốc!");
				}
			}
		}

		// 5. Đánh dấu đã nhận rương
		isClaimed = true; // Cần lưu ý logic này phụ thuộc vào cách bạn lưu trạng thái
	}
}