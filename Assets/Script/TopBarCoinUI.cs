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
    private void updateCoinUI(int coin){
        coinText.text = coin.ToString();
    }

}
