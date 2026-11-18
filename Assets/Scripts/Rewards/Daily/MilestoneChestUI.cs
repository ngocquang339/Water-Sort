using UnityEngine;
using UnityEngine.UI;

public class MilestoneChestUI : MonoBehaviour
{
	[Header("UI Components")]
	public Image chestImage;      
	public GameObject redDot;      
	public GameObject glowEffect; 
	public GameObject sun_Burst;
	public Button chestButton;     

	private int chestIndex;
	private System.Action<int> onClaimAction;

	// Hàm khởi tạo ban đầu (Giống như đăng ký listener cho nút bấm)
	public void SetupChest(int index, System.Action<int> onChestClicked)
	{
		this.chestIndex = index;
		this.onClaimAction = onChestClicked;

		if (chestButton != null)
		{
			chestButton.onClick.RemoveAllListeners();
			chestButton.onClick.AddListener(() => onClaimAction?.Invoke(chestIndex));
		}
	}

	// Hàm quyết định xem rương sẽ hiển thị như thế nào
	public void UpdateChestUI(MilestoneChestConfig config, int totalDays, bool isClaimed)
	{
		// Kiểm tra xem người chơi đã tích lũy đủ số ngày yêu cầu chưa
		bool isReached = totalDays >= config.requiredDays;

		if (isClaimed)
		{
			// TRẠNG THÁI 1: ĐÃ NHẬN QÙA
			chestImage.sprite = config.chestOpenedIcon; // Đổi sang ảnh rương mở nắp
			redDot.SetActive(false);
			glowEffect.SetActive(false);
			sun_Burst.SetActive(false);
			if (chestButton != null) chestButton.interactable = false; // Đã nhận rồi thì cấm bấm tiếp
		}
		else if (isReached)
		{
			// TRẠNG THÁI 2: ĐỦ ĐIỀU KIỆN NHƯNG CHƯA NHẬN (Kích hoạt Chấm đỏ + Phát sáng)
			chestImage.sprite = config.chestClosedIcon;
			redDot.SetActive(true);
			glowEffect.SetActive(true);
			sun_Burst.SetActive(true);
			if (chestButton != null) chestButton.interactable = true; // Cho phép bấm nhận quà
		}
		else
		{
			// TRẠNG THÁI 3: CHƯA ĐỦ ĐIỀU KIỆN (Khóa)
			chestImage.sprite = config.chestClosedIcon;
			redDot.SetActive(false);
			glowEffect.SetActive(false);
			sun_Burst.SetActive(false);
			if (chestButton != null) chestButton.interactable = false; // Chưa đủ ngày thì không cho bấm
		}
	}
}