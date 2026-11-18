using UnityEngine;

public class Ranking_Panel_Slide : MonoBehaviour
{

	[Header("UI Reference")]
	// Chỉ cần kéo script PanelSlider của ShopPanel vào đây
	public PanelSlider shopPanelSlider;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public void ClickOpenShop()
	{
		// Gọi động cơ trượt mở ra
		shopPanelSlider.OpenPanel();
	}

	public void ClickCloseShop() 
	{ 
		shopPanelSlider.ClosePanel(); 
	}
}
