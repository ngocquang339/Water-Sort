using UnityEngine;
using TMPro;
public class TopBarCoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    void Start()
    {
        updateCoinUI(CurrencyManager.Instance.GetCoin());
    }

    void OnEnable(){
        CurrencyManager.OnCoinChanged += updateCoinUI;
    }

    void OnDisable(){
        CurrencyManager.OnCoinChanged -= updateCoinUI;
    }
    private void updateCoinUI(float newAmount){
		if (newAmount < 1000)
		{
			coinText.text = newAmount.ToString();
		}
		else if (newAmount >= 1000 && newAmount < 1000000)
		{
			coinText.text = (newAmount / 1000f).ToString("0.#") + "K";
		}
		else if (newAmount >= 1000000 && newAmount < 1000000000)
		{
			coinText.text = (newAmount / 1000000f).ToString("0.#") + "M";
		}
		else
		{
			coinText.text = (newAmount / 1000000000f).ToString("0.#") + "B";
		}
	}

}
