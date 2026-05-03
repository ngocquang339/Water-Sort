using UnityEngine;
using TMPro;

public class TopBarUI : MonoBehaviour
{
	[Header("UI References")]
	public TextMeshProUGUI coinText;
	public TextMeshProUGUI diamondText;

	void Start()
	{
		// Khi vừa mở game, hỏi ngân hàng số dư hiện tại để vẽ lên màn hình
		UpdateCoinUI(CurrencyManager.Instance.GetCoin());
		UpdateDiamondUI(CurrencyManager.Instance.GetDiamond());
	}

	// BẬT RADIO: Đăng ký nghe ngóng sự kiện khi UI này được bật lên
	void OnEnable()
	{
		CurrencyManager.OnCoinChanged += UpdateCoinUI;
		CurrencyManager.OnDiamondChanged += UpdateDiamondUI;
	}

	// TẮT RADIO: Bắt buộc phải Hủy đăng ký khi UI này bị tắt đi để tránh rò rỉ bộ nhớ (Memory Leak)
	void OnDisable()
	{
		CurrencyManager.OnCoinChanged -= UpdateCoinUI;
		CurrencyManager.OnDiamondChanged -= UpdateDiamondUI;
	}

	// Hàm này sẽ tự động chạy khi nghe thấy tiếng hét từ CurrencyManager
	private void UpdateCoinUI(int newAmount)
	{
		coinText.text = newAmount.ToString();
	}

	private void UpdateDiamondUI(int newAmount)
	{
		diamondText.text = newAmount.ToString();
	}
}