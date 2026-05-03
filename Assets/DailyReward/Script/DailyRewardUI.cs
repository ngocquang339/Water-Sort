using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
	[Header("UI Elements")]
	public GameObject popupPanel;

	[Header("Danh sách 7 ô ngày")]
	public DaySlotUI[] daySlots;

	void Start()
	{
		// TỰ ĐỘNG NỐI DÂY SỰ KIỆN: 
		// Duyệt qua cả 7 ô, nếu ô nào bị bấm, gọi hàm OnSlotClicked ở dưới
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
			if (i >= data.rewards.Length) break;

			// Hỏi ông Não xem ô số [i] này đang ở trạng thái gì
			DaySlotState slotState = manager.GetStateForDay(i);

			// Ra lệnh cho thằng thợ sơn (DaySlotUI) bắt đầu vẽ
			daySlots[i].updateSlotUI(i + 1, data.rewards[i], slotState);
		}

		// Đã xóa phần xử lý nút Claim cũ ở đây
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
		popupPanel.SetActive(true);
	}

	public void ClosePopup()
	{
		popupPanel.SetActive(false);
	}
}