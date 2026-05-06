using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public GameObject bottlePrefab;
	public LevelData currentLevelData;

	[Header("Cài đặt Lưới (Grid Settings)")]
	public float spacingX = 1.5f;
	public float spacingY = 2.0f;
	public int maxBottlesPerRow = 5;

	void Start()
	{
		GenerateLevel(currentLevelData);
	}

	public void GenerateLevel(LevelData levelData)
	{
		int totalBottles = levelData.bottleInLevel.Length;

		int numRows = Mathf.CeilToInt((float)totalBottles / maxBottlesPerRow);

		int bottleIndex = 0; // Biến theo dõi xem đang khởi tạo đến chai thứ mấy trong Data
		int remainingBottles = totalBottles; // Số chai còn lại chưa được xếp

		for (int row = 0; row < numRows; row++)
		{
			// THUẬT TOÁN CHIA ĐỀU: Lấy số chai còn lại chia cho số hàng còn lại
			int rowsLeft = numRows - row;
			//Làm tròn số chai 
			int bottlesInThisRow = Mathf.CeilToInt((float)remainingBottles / rowsLeft);
			remainingBottles -= bottlesInThisRow;

			float startX = -(bottlesInThisRow - 1) * spacingX / 2f;
			float startY = (numRows - 1) * spacingY / 2f; //Căn giữa cả cụm theo chiều dọc

			//Vòng lặp vẽ từng chai trong hàng hiện tại
			for (int col = 0; col < bottlesInThisRow; col++)
			{
				float posX = startX + (col * spacingX);
				float posY = startY - (row * spacingY);
				Vector2 spawnPosition = new Vector2(posX, posY);

				// 1. Kiểm tra xem Level này có xài Prefab chai riêng không, nếu không thì dùng chai mặc định
				GameObject prefabToUse = levelData.customBottlePrefab != null ? levelData.customBottlePrefab : bottlePrefab;

				// 2. Đẻ ra GameObject chai
				GameObject newBottle = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);

				Bottle bottleScript = newBottle.GetComponent<Bottle>();
				if (bottleScript != null)
				{
					// 3. TRUYỀN SỨC CHỨA TỪ LEVEL DATA VÀO CHAI
					bottleScript.capacity = levelData.bottleCapacity;

					// 4. Nạp màu
					bottleScript.initializeColors(levelData.bottleInLevel[bottleIndex].initialColors);
				}

				bottleIndex++;
			}
		}
	}
}