using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
	public static CurrencyManager Instance { get; private set; }

	public static event Action<int> OnCoinChanged;
	public static event Action<int> OnDiamondChanged;

	private int currentCoin;
	private int currentDiamond;

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		LoadCurrency();
	}

	private void LoadCurrency()
	{
		currentCoin = PlayerPrefs.GetInt("Player_Coin", 0);
		currentDiamond = PlayerPrefs.GetInt("Player_Diamond", 0);
	}

	public void AddCoin(int amount)
	{
		currentCoin += amount;
		PlayerPrefs.SetInt("Player_Coin", currentCoin);
		PlayerPrefs.Save();

		// 2. PHÁT LOA THÔNG BÁO! 
		// Dấu ? để kiểm tra xem có ai đang nghe không (nếu không có ai nghe thì không phát để tránh lỗi)
		OnCoinChanged?.Invoke(currentCoin);
	}

	public void AddDiamond(int amount)
	{
		currentDiamond += amount;
		PlayerPrefs.SetInt("Player_Diamond", currentDiamond);
		PlayerPrefs.Save();

		OnDiamondChanged?.Invoke(currentDiamond);
	}

	// Hàm để các hệ thống khác xem số dư
	public int GetCoin() => currentCoin;
	public int GetDiamond() => currentDiamond;
}