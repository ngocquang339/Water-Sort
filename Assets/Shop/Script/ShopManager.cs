using UnityEngine;

public class ShopManager : MonoBehaviour
{
	public ShopPackData[] allShopPacks;

	[Header("Prefabs")]
	public GameObject shopPackPrefab;

	[Header("Grids Của Các Danh Mục")]
	public Transform resourceGrid;      // Kéo object "Resource_Grid" vào đây
	public Transform coinGrid;          // Kéo object "Coin_Grid" vào đây

	[Header("UI Reference")]
	public PanelSlider shopPanelSlider;

	void Start()
	{
		GenerateShop();
	}

	void GenerateShop()
	{
		foreach (ShopPackData packData in allShopPacks)
		{
			// 1. Phân luồng: Xác định xem ném vào Grid nào
			Transform targetGrid = null;

			switch (packData.category)
			{
				case ShopCategory.ResourcePack:
					targetGrid = resourceGrid;
					break;
				case ShopCategory.CoinPack:
					targetGrid = coinGrid;
					break;
			}

			// Nếu không tìm thấy khay phù hợp thì bỏ qua
			if (targetGrid == null) continue;

			// 2. Sinh ra thẻ Pack chui đúng vào nhà của nó
			GameObject packGO = Instantiate(shopPackPrefab, targetGrid);
			packGO.transform.localScale = Vector3.one;

			// 3. Truyền data
			ShopPackUI uiScript = packGO.GetComponent<ShopPackUI>();
			if (uiScript != null)
			{
				uiScript.SetupPack(packData);
			}
		}
	}

	public void ClickOpenShop()
	{
		shopPanelSlider.OpenPanel();
	}

	public void ClickCloseShop()
	{
		shopPanelSlider.ClosePanel();
	}
}