using UnityEngine;
using TMPro; // Giả sử bạn dùng TextMeshPro cho UI vàng

public class CoinManager : MonoBehaviour
{
	public static CoinManager instance; // Singleton

	[Header("UI Vàng Chính")]
	public TextMeshProUGUI coinGlobalText; // Kéo Text hiển thị tổng vàng vào đây

	private int currentCoins;
	private const string COIN_KEY = "UserCoins_Total"; // Key lưu vào máy

	private void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);

		LoadCoins();
	}

	// Tải vàng khi vào game
	private void LoadCoins()
	{
		// Mặc định cho 500 vàng nếu lần đầu chơi
		currentCoins = PlayerPrefs.GetInt(COIN_KEY, 500);
		UpdateUI();
	}

	// Hàm kiểm tra xem có đủ tiền không
	public bool CanAfford(int cost)
	{
		return currentCoins >= cost;
	}

	// Hàm trừ vàng (Chỉ gọi khi CanAfford = true)
	public void SpendCoins(int amount)
	{
		currentCoins -= amount;
		PlayerPrefs.SetInt(COIN_KEY, currentCoins);
		PlayerPrefs.Save();

		// UPDATE UI NGAY LẬP TỨC
		UpdateUI();

		Debug.Log("Đã trừ vàng. Còn lại: " + currentCoins);
	}

	// Cập nhật số lên màn hình
	private void UpdateUI()
	{
		if (coinGlobalText != null)
		{
			coinGlobalText.text = currentCoins.ToString();
		}
	}

	// Hàm hack vàng để test (Tùy chọn)
	[ContextMenu("Add 1000 Coins")]
	public void HackAddCoins()
	{
		currentCoins += 1000;
		PlayerPrefs.SetInt(COIN_KEY, currentCoins);
		UpdateUI();
	}
}