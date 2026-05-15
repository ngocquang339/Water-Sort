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

	private void UpdateCoinUI(int newAmount)
	{
		coinText.text = newAmount.ToString();
	}

	private void UpdateDiamondUI(int newAmount)
	{
		diamondText.text = newAmount.ToString();
	}
}