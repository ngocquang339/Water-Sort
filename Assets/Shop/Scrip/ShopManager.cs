using UnityEngine;

public class ShopManager : MonoBehaviour
{
	public ShopPackData[] allShopPacks; // Kéo thả 5 cái file ScriptableObject bạn tạo ở Bước 1 vào đây

	public GameObject shopPackPrefab;   // Kéo ShopPack_Prefab tổng vào đây
	public Transform shopContent;       // Kéo cái khay Vertical Layout vào đây

	void Start()
	{
		GenerateShop();
	}

	void GenerateShop()
	{
		foreach (ShopPackData packData in allShopPacks)
		{
			// Sinh ra 1 thẻ Pack
			GameObject packGO = Instantiate(shopPackPrefab, shopContent);

			// Tìm script ShopPackUI trên thẻ đó và truyền Data vào
			ShopPackUI uiScript = packGO.GetComponent<ShopPackUI>();
			if (uiScript != null)
			{
				uiScript.SetupPack(packData);
			}
		}
	}
}