using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public GameObject bottlePrefab;
	public LevelData currentLevelData;
	public GameManager gameManager;

	[Header("Cài đặt Lưới (Grid Settings)")]
	public float spacingX = 1.5f;
	public float spacingY = 2.0f;
	public int maxBottlesPerRow = 5;

	void Start()
	{
		// 1. Đọc PlayerPrefs xem người chơi đang ở Level mấy. Nếu mới chơi lần đầu, mặc định là 1.
		int currentLevelNumber = PlayerPrefs.GetInt("CurrentLevel", 1);

		// 2. Dùng tuyệt chiêu Resources.Load để tự động tìm file LevelData tương ứng
		// Đảm bảo tên file của bạn đặt chuẩn là "Level_1", "Level_2"...
		LevelData loadedData = Resources.Load<LevelData>("Levels/Level_" + currentLevelNumber);

		// 3. Kiểm tra xem có lấy được Data không (phòng trường hợp người chơi vượt qua level cuối cùng)
		if (loadedData != null)
		{
			currentLevelData = loadedData;
		}
		else
		{
			Debug.LogWarning("Không tìm thấy Level " + currentLevelNumber + " ! Sẽ load lại Level 1.");
			// Tùy bạn xử lý: có thể load lại level 1, hoặc load level cao nhất hiện có
			currentLevelData = Resources.Load<LevelData>("Levels/Level_1");
			// PlayerPrefs.SetInt("CurrentLevel", 1); // Reset lại nếu muốn
		}

		// 4. Sinh màn chơi như bình thường
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
					// ========================================================
					// 2. NHÉT CHAI VỪA ĐẺ VÀO DANH SÁCH CỦA GAMEMANAGER
					// ========================================================
					if (gameManager != null)
					{
						gameManager.allBottles.Add(bottleScript);
					}
				}

				bottleIndex++;
			}
		}
	}

	// Hàm này trả về danh sách tọa độ mới khi tổng số chai thay đổi
	public List<Vector3> GetBottleTargetPositions(int totalBottles)
	{
		List<Vector3> targetPositions = new List<Vector3>();

		int numRows = Mathf.CeilToInt((float)totalBottles / maxBottlesPerRow);
		int remainingBottles = totalBottles;

		for (int row = 0; row < numRows; row++)
		{
			// THUẬT TOÁN CHIA ĐỀU (Giữ y nguyên logic của bạn)
			int rowsLeft = numRows - row;
			int bottlesInThisRow = Mathf.CeilToInt((float)remainingBottles / rowsLeft);
			remainingBottles -= bottlesInThisRow;

			float startX = -(bottlesInThisRow - 1) * spacingX / 2f;
			float startY = (numRows - 1) * spacingY / 2f;

			for (int col = 0; col < bottlesInThisRow; col++)
			{
				float posX = startX + (col * spacingX);
				float posY = startY - (row * spacingY);

				// Lưu tọa độ Z = 0f để các chai không bị lệch chiều sâu
				targetPositions.Add(new Vector3(posX, posY, 0f));
			}
		}

		return targetPositions;
	}
}