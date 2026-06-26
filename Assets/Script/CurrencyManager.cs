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
		currentCoin = PlayerPrefs.GetFloat(COIN_KEY, 1000);
		Debug.Log("Current Coin Loaded: " + currentCoin);
		currentDiamond = PlayerPrefs.GetFloat(DIAMOND_KEY, 10000);
		Debug.Log("Current Diamond Loaded: " + currentDiamond);
		currentUndo = PlayerPrefs.GetFloat(UNDO_KEY, 2);
		Debug.Log("Current Undo Loaded: " + currentUndo);
		currentHint = PlayerPrefs.GetFloat(HINT_KEY, 2);
		Debug.Log("Current Hint Loaded: " + currentHint);
		currentAddBottle = PlayerPrefs.GetFloat(ADDBOTTLE_KEY, 2);
		Debug.Log("Current Add Bottle Loaded: " + currentAddBottle);
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
		Debug.Log("Adding Undo: " + amount);
		currentUndo += amount;
		PlayerPrefs.SetFloat(UNDO_KEY, currentUndo);
		PlayerPrefs.Save();
		OnUndoChanged?.Invoke(currentUndo);
	}

	public void AddHint(float amount){
		Debug.Log("Adding Hint: " + amount);
		currentHint += amount;
		PlayerPrefs.SetFloat(HINT_KEY, currentHint);
		PlayerPrefs.Save();
		OnHintChanged?.Invoke(currentHint);
	}

	public void AddBonusBottle(float amount)
	{
		Debug.Log("Adding Bonus Bottle: " + amount);
		currentAddBottle += amount;
		PlayerPrefs.SetFloat(ADDBOTTLE_KEY, currentAddBottle);
		PlayerPrefs.Save();
		OnAddBottleChanged?.Invoke(currentAddBottle);
	}

	public void AddDiamond(float amount)
	{
		currentDiamond += amount;
		PlayerPrefs.SetFloat(DIAMOND_KEY, currentDiamond);
		PlayerPrefs.Save();

		OnDiamondChanged?.Invoke(currentDiamond);
	}

	public void AddCurrency(float amount, RewardItemType rewardItem)
	{
		switch (rewardItem)
		{
			case RewardItemType.Coin:
				AddCoin(amount);
				break;
			case RewardItemType.Diamond:
				AddDiamond(amount);
				break;
			case RewardItemType.Undo:
				AddUndo(amount);
				break;
			case RewardItemType.Hint:
				AddHint(amount);
				break;
			case RewardItemType.AddBottle:
				AddBonusBottle(amount);
				break;
		}
	}

	// Hàm kiểm tra xem có đủ tiền không
	public bool CanAfford(float cost, CurrencyType currencyType)
	{
		switch (currencyType)
		{
			case CurrencyType.Coin:
				Debug.Log("Current Coin: " + currentCoin);
				Debug.Log("Cost: " + cost);
				return currentCoin >= cost;
			case CurrencyType.Diamond:
				Debug.Log("Current Diamond: " + currentDiamond);
				Debug.Log("Cost: " + cost);
				return currentDiamond >= cost;
			default:
				return false;
		}
	}

	// Hàm trừ vàng (Chỉ gọi khi CanAfford = true)
	public void SpendCurrency(float amount, CurrencyType currencyType)
	{
		switch (currencyType)
		{
			case CurrencyType.Coin:
				currentCoin -= amount;
				PlayerPrefs.SetFloat(COIN_KEY, currentCoin);
				PlayerPrefs.Save();
				OnCoinChanged?.Invoke(currentCoin);
				break;
			case CurrencyType.Diamond:
				currentDiamond -= amount;
				PlayerPrefs.SetFloat(DIAMOND_KEY, currentDiamond);
				PlayerPrefs.Save();
				OnDiamondChanged?.Invoke(currentDiamond);
				break;
		}
	}	

	// Hàm để các hệ thống khác xem số dư
	public float GetCoin() => currentCoin;
	public float GetDiamond() => currentDiamond;
	//hi
}