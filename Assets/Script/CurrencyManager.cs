using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
	public static CurrencyManager Instance { get; private set; }

	public static event Action<float> OnCoinChanged;
	public static event Action<float> OnDiamondChanged;
	public static event Action<float> OnUndoChanged;
	public static event Action<float> OnHintChanged;
	public static event Action<float> OnAddBottleChanged;
	//public TopBarCoinUI coinUI;

	private const string COIN_KEY = "Player_Coin";
	private const string DIAMOND_KEY = "Player_Diamond";
	private const string UNDO_KEY = "Help_Undo";
	private const string HINT_KEY = "Help_Hint";
	private const string ADDBOTTLE_KEY = "Help_AddBottle";
	private float currentCoin;
	private float currentDiamond;
	private float currentUndo;
	private float currentHint;
	private float currentAddBottle;

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		LoadCurrency();
	}

	private void LoadCurrency()
	{
		currentCoin = PlayerPrefs.GetFloat("Player_Coin", 0);
		Debug.Log("Current Coin Loaded: " + currentCoin);
		currentDiamond = PlayerPrefs.GetFloat("Player_Diamond", 0);
	}

	public void AddCoin(float amount)
	{
		currentCoin += amount;
		PlayerPrefs.SetFloat(COIN_KEY, currentCoin);
		PlayerPrefs.Save();

		// 2. PHÁT LOA THÔNG BÁO! 
		// Dấu ? để kiểm tra xem có ai đang nghe không (nếu không có ai nghe thì không phát để tránh lỗi)
		OnCoinChanged?.Invoke(currentCoin);
	}

	public void AddUndo(float amount){
		currentUndo += amount;
		PlayerPrefs.SetFloat(UNDO_KEY, currentUndo);
		PlayerPrefs.Save();
		OnUndoChanged?.Invoke(currentUndo);
	}

	public void AddHint(float amount){
		currentHint += amount;
		PlayerPrefs.SetFloat(HINT_KEY, currentHint);
		PlayerPrefs.Save();
		OnHintChanged?.Invoke(currentHint);
	}

	public void AddBonusBottle(float amount)
	{
		currentAddBottle += amount;
		PlayerPrefs.SetFloat(ADDBOTTLE_KEY, currentAddBottle);
		PlayerPrefs.Save();
		OnAddBottleChanged?.Invoke(currentAddBottle);
	}

	public void AddDiamond(float amount)
	{
		currentDiamond += amount;
		PlayerPrefs.SetFloat("Player_Diamond", currentDiamond);
		PlayerPrefs.Save();

		OnDiamondChanged?.Invoke(currentDiamond);
	}

	// Hàm kiểm tra xem có đủ tiền không
	public bool CanAfford(int cost)
	{
		Debug.Log("Current Coin: " + currentCoin);
		Debug.Log("Cost: " + cost);
		return currentCoin >= cost;
	}

	// Hàm trừ vàng (Chỉ gọi khi CanAfford = true)
	public void SpendCoins(float amount)
	{
		currentCoin -= amount;
		PlayerPrefs.SetFloat(COIN_KEY, currentCoin);
		PlayerPrefs.Save();

		OnCoinChanged?.Invoke(currentCoin);

		Debug.Log("Đã trừ vàng. Còn lại: " + currentCoin);
	}

	// Hàm để các hệ thống khác xem số dư
	public float GetCoin() => currentCoin;
	public float GetDiamond() => currentDiamond;
	//hi
}