using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
	[Header("UI Elements")]
	public GameObject dailyReward;
	public GameObject darkBackground;

	[Header("Danh sách 7 ô ngày")]
	public DaySlotUI[] daySlots;

	[Header("Top Banner UI")]
	public Slider topProgressBar;

	void Start()
	{
		foreach (DaySlotUI slot in daySlots)
		{
			if (slot.slotButton != null)
			{
				slot.slotButton.onClick.AddListener(OnSlotClicked);
			}
		}
	}

	void OnEnable()
	{
		// Kiểm tra xem ông "Não" đã tỉnh dậy và khởi tạo xong Instance chưa
		if (DailyRewardManager.Instance == null)
		{
			// Nếu chưa, hẹn 0.1 giây sau mới chạy hàm vẽ giao diện
			Invoke("RefreshUI", 0.1f);
		}
		else
		{
			// Nếu Não đã sẵn sàng rồi thì vẽ luôn
			RefreshUI();
		}
	}

	public void RefreshUI()
	{
		// 1. Gọi ông "Não" để lấy kho dữ liệu
		DailyRewardManager manager = DailyRewardManager.Instance;
		DailyRewardData data = manager.rewardData;

		// 2. Dùng 1 vòng lặp for để ra lệnh cho cả 7 ô tự vẽ
		for (int i = 0; i < daySlots.Length; i++)
		{
			// Nếu lỡ mảng UI dài hơn mảng Data thì dừng lại để tránh lỗi bục game
			if (i >= data.days.Length) break;

			// Hỏi ông Não xem ô số [i] này đang ở trạng thái gì
			DaySlotState slotState = manager.GetStateForDay(i);

			// Ra lệnh cho thằng thợ sơn (DaySlotUI) bắt đầu vẽ
			daySlots[i].updateSlotUI(i + 1, data.days[i], slotState);
		}

		// 3. CẬP NHẬT THANH TIẾN TRÌNH Ở TRÊN

		// Lấy ngày max (ví dụ 30) để làm độ dài tối đa cho thanh
		int maxDays = data.milestoneChests[data.milestoneChests.Length - 1].requiredDays;
		topProgressBar.maxValue = maxDays;

		// Đổ nước bằng đúng tổng số ngày người chơi đã điểm danh
		topProgressBar.value = manager.totalClaimedDays;

		// Ép thanh Slider đo theo hệ % (từ 0 đến 1)
		topProgressBar.minValue = 0f;
		topProgressBar.maxValue = 1f;

		// Đổ nước bằng hàm bóp méo tự viết
		topProgressBar.value = CalculateFakeProgress(manager.totalClaimedDays, data.milestoneChests);
	}

	// Hàm này sẽ tự động kích hoạt khi người chơi bấm vào BẤT KỲ ô quà nào có thể bấm
	private void OnSlotClicked()
	{
		// Báo cho ông Não thực hiện thuật toán trao quà và lưu ngày tháng
		DailyRewardManager.Instance.ClaimTodayReward();

		// Nhận xong thì load lại giao diện để cái ổ khóa biến thành dấu tích xanh
		RefreshUI();
	}

	public void OpenPopup()
	{
		dailyReward.SetActive(true);
		darkBackground.SetActive(true);
	}

	public void ClosePopup()
	{
		dailyReward.SetActive(false);
		darkBackground.SetActive(false);
	}

	private float CalculateFakeProgress(int currentDays, MilestoneChestConfig[] chests)
	{
		if (currentDays <= 0 || chests == null || chests.Length == 0) return 0f;

		int previousDays = 0;
		float previousAnchor = 0f;

		for (int i = 0; i < chests.Length; i++)
		{
			int targetDays = chests[i].requiredDays;
			float targetAnchor = chests[i].visualAnchor;

			// Tìm xem người chơi đang đứng ở đoạn nào (Ví dụ: giữa rương 1 và 2)
			if (currentDays <= targetDays)
			{
				// Tính số phần trăm thời gian đã qua trong đoạn ngắn này
				float progressInSegment = (float)(currentDays - previousDays) / (targetDays - previousDays);

				// Ép % thời gian đó vào độ dài vật lý thực tế trên màn hình
				return Mathf.Lerp(previousAnchor, targetAnchor, progressInSegment);
			}

			// Nếu đã vượt qua rương này, chuyển mốc để tính đoạn tiếp theo
			previousDays = targetDays;
			previousAnchor = targetAnchor;
		}

		// Nếu đã qua rương cuối cùng thì auto đầy thanh
		return 1f;
	}
}