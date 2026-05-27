using UnityEngine;
using TMPro;

public class TopBarUI : MonoBehaviour
{
	[Header("UI References")]
	public TextMeshProUGUI coinText;
	public TextMeshProUGUI diamondText;

	void Start()
	{
		UpdateCoinUI(CurrencyManager.Instance.GetCoin());
		UpdateDiamondUI(CurrencyManager.Instance.GetDiamond());
	}
	//hi
	void OnEnable()
	{
		CurrencyManager.OnCoinChanged += UpdateCoinUI;
		CurrencyManager.OnDiamondChanged += UpdateDiamondUI;
	}

	void OnDisable()
	{
		CurrencyManager.OnCoinChanged -= UpdateCoinUI;
		CurrencyManager.OnDiamondChanged -= UpdateDiamondUI;
	}

	private void UpdateCoinUI(float newAmount)
	{
		if (newAmount < 1000)
		{
			coinText.text = newAmount.ToString();
		}
		else if (newAmount >= 1000 && newAmount < 1000000){
			coinText.text = (newAmount / 1000f).ToString("0.#") + "K";
		}
		else if(newAmount >= 1000000 && newAmount < 1000000000){
			coinText.text = (newAmount / 1000000f).ToString("0.#") + "M";
		}
		else{
			coinText.text = (newAmount / 1000000000f).ToString("0.#") + "B";
		}
		
	}

	private void UpdateDiamondUI(float newAmount)
	{
		if(newAmount < 1000){
			diamondText.text = newAmount.ToString();
		}
		else if (newAmount >= 1000 && newAmount < 1000000)
		{
			diamondText.text = (newAmount / 1000f).ToString("0.#") + "K";
		}
		else if (newAmount >= 1000000 && newAmount < 1000000000)
		{
			diamondText.text = (newAmount / 1000000f).ToString("0.#") + "M";
		}
		else
		{
			diamondText.text = (newAmount / 1000000000f).ToString("0.#") + "B";
		}
	}
}