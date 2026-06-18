using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManagerMultiplayer : MonoBehaviour
{
	public static LevelManagerMultiplayer Instance;
	[Header("Multiplayer Anchors")]
	[Tooltip("Kéo object Player1_Anchor (bên trái) vào đây")]
	public Transform player1Anchor;
	[Tooltip("Kéo object Player2_Anchor (bên phải) vào đây")]
	public Transform player2Anchor;

	[Header("Prefabs & Data")]
	public GameObject bottlePrefab;
	public LevelData currentLevelData;
	public TextMeshProUGUI levelText;

	[Header("Cài đặt Lưới (Grid Settings)")]
	public float spacingX = 1.5f;
	public float spacingY = 2.0f;
	public int maxBottlesPerRow = 5;

	void Awake()
	{
		Instance = this;
	}

	// 2. Thêm tham số PlayerBoardController vào hàm
	public void GeneratePlayerLevel(LevelData levelData, Transform anchor, int playerID, PlayerBoardController boardController)
	{
		int totalBottles = levelData.bottleInLevel.Length;
		int numRows = Mathf.CeilToInt((float)totalBottles / maxBottlesPerRow);
		int bottleIndex = 0;
		int remainingBottles = totalBottles;

		for (int row = 0; row < numRows; row++)
		{
			int rowsLeft = numRows - row;
			int bottlesInThisRow = Mathf.CeilToInt((float)remainingBottles / rowsLeft);
			remainingBottles -= bottlesInThisRow;

			float startX = -(bottlesInThisRow - 1) * spacingX / 2f;
			float startY = (numRows - 1) * spacingY / 2f;

			for (int col = 0; col < bottlesInThisRow; col++)
			{
				float posX = startX + (col * spacingX);
				float posY = startY - (row * spacingY);

				GameObject prefabToUse = levelData.customBottlePrefab != null ? levelData.customBottlePrefab : bottlePrefab;

				// Thay vì dùng: GameObject newBottle = Instantiate(prefabToUse, anchor);
				// Hãy sửa thành 3 dòng này:

				GameObject newBottle = Instantiate(prefabToUse); // Sinh ra trước
				newBottle.transform.SetParent(anchor, false);    // Set cha sau (false để giữ nguyên world position nếu cần)

				// Bây giờ mới đặt vị trí
				newBottle.transform.localPosition = new Vector3(posX, posY, 0f);

				// LƯU Ý: Tùy Prefab bạn đang gắn script tên gì mà đổi thành Bottle hoặc BottleMulti nhé
				BottleMulti bottleScript = newBottle.GetComponent<BottleMulti>();
				if (bottleScript != null)
				{
					bottleScript.capacity = levelData.bottleCapacity;
					bottleScript.initializeColors(levelData.bottleInLevel[bottleIndex].initialColors);
					bottleScript.playerID = playerID;

					// 3. Nhét thẳng chai vừa đẻ vào danh sách (myBottles) của đúng người chơi đó
					if (boardController != null)
					{
						boardController.allBottles.Add(bottleScript);
					}
				}
				bottleIndex++;
			}
		}
	}

	// Cập nhật hàm lấy tọa độ: Cần truyền thêm Anchor để biết đang tính toán cho bên trái hay bên phải
	public List<Vector3> GetBottleTargetPositions(int totalBottles, Transform anchor)
	{
		List<Vector3> targetPositions = new List<Vector3>();

		int numRows = Mathf.CeilToInt((float)totalBottles / maxBottlesPerRow);
		int remainingBottles = totalBottles;

		for (int row = 0; row < numRows; row++)
		{
			int rowsLeft = numRows - row;
			int bottlesInThisRow = Mathf.CeilToInt((float)remainingBottles / rowsLeft);
			remainingBottles -= bottlesInThisRow;

			float startX = -(bottlesInThisRow - 1) * spacingX / 2f;
			float startY = (numRows - 1) * spacingY / 2f;

			for (int col = 0; col < bottlesInThisRow; col++)
			{
				float posX = startX + (col * spacingX);
				float posY = startY - (row * spacingY);

				// LƯU Ý 4: Cộng thêm vị trí của Anchor để trả về tọa độ thế giới (World Position) chính xác
				targetPositions.Add(anchor.position + new Vector3(posX, posY, 0f));
			}
		}

		return targetPositions;
	}
}