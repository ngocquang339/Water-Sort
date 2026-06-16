using UnityEngine;

public class VersusManager : MonoBehaviour
{
	public static VersusManager Instance;

	[Header("Bộ Đề Thi (3 Màn Chơi)")]
	public LevelData[] versusLevels = new LevelData[3]; // Kéo thả 3 file ScriptableObject vào đây

	[Header("Tham chiếu 2 Người Chơi")]
	public PlayerBoardController player1Board;
	public PlayerBoardController player2Board;

	[Header("UI Kết Quả")]
	public GameObject victoryPanel;
	public TMPro.TextMeshProUGUI winnerText;

	private bool isGameOver = false;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// Vừa vào trận, phát Màn 1 (phần tử số 0) cho cả 2 bên chơi cùng lúc
		if (versusLevels.Length >= 3)
		{
			player1Board.InitBoard(0);
			player2Board.InitBoard(0);
		}
		else
		{
			Debug.LogError("Hy ơi, nhớ kéo đủ 3 file ScriptableObject vào ô Versus Levels nhé!");
		}
	}

	// Hàm này được gọi khi 1 trong 2 người chơi hoàn thành Level hiện tại của họ
	public void OnPlayerCompleteLevel(int playerID, int completedLevelIndex)
	{
		if (isGameOver) return;

		int nextLevelIndex = completedLevelIndex + 1;

		// Nếu giải xong level index là 2 (tức là Màn 3), người đó THẮNG CHUNG CUỘC!
		if (nextLevelIndex >= 3)
		{
			DeclareWinner(playerID);
			return;
		}

		// Nếu chưa hết 3 màn, cho bên đó chuyển sang màn tiếp theo
		if (playerID == 1)
		{
			player1Board.InitBoard(nextLevelIndex);
		}
		else if (playerID == 2)
		{
			player2Board.InitBoard(nextLevelIndex);
		}
	}

	private void DeclareWinner(int winnerID)
	{
		isGameOver = true;
		victoryPanel.SetActive(true);
		winnerText.text = $"PLAYER {winnerID} CHIẾN THẮNG!";

		// THÊM DÒNG NÀY: Dừng đồng hồ lại để khoe thành tích!
		GetComponent<VersusTimer>().StopTimer();
		// Dừng thời gian hoặc chặn thao tác của cả 2 bên lại
		Time.timeScale = 0f;
	}
}