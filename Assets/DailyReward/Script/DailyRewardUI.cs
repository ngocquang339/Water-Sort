using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
	[Header("UI Elements")]
	public GameObject popupPanel; // Cái bảng to chứa toàn bộ giao diện
	public Button claimButton;    // Nút bấm "Nhận quà" to đùng ở dưới (nếu có)

	[Header("Danh sách 7 ô ngày")]
	// Bạn sẽ khóa Inspector lại, bôi đen cả 7 cái Slot ngoài Scene và kéo thả 1 phát vào đây
	public DaySlotUI[] daySlots;

	void OnEnable()
	{
		// Mỗi khi cái bảng Popup này được bật lên, nó sẽ tự động vẽ lại dữ liệu mới nhất
		RefreshUI();
	}

	public void RefreshUI()
	{
		// 1. Gọi ông "Não" để lấy kho dữ liệu
		DailyRewardManager manager = DailyRewardManager.Instance;
		DailyRewardData data = manager.rewardData;

		// 2. Dùng 1 vòng lặp for thần thánh để ra lệnh cho cả 7 ô tự vẽ
		for (int i = 0; i < daySlots.Length; i++)
		{
			// Nếu lỡ mảng UI dài hơn mảng Data thì dừng lại để tránh lỗi bục game
			if (i >= data.rewards.Length) break;

			// Hỏi ông Não xem ô số [i] này đang ở trạng thái gì (Khóa / Đang chờ nhận / Đã nhận)
			DaySlotState slotState = manager.GetStateForDay(i);

			// Ra lệnh cho thằng thợ sơn (DaySlotUI) bắt đầu vẽ!
			// Lưu ý: Ngày hiển thị là i + 1 (ví dụ i=0 thì là Day 1)
			daySlots[i].updateSlotUI(i + 1, data.rewards[i], slotState);
		}

		// 3. Xử lý cái nút Claim to (nếu bạn có)
		if (claimButton != null)
		{
			bool canClaim = manager.CanClaimToday();
			claimButton.interactable = canClaim; // Nút sáng lên nếu được nhận, tối đi nếu không được
		}
	}

	// Gắn hàm này vào sự kiện OnClick() của cái nút Claim ngoài Unity
	public void OnClickClaimButton()
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