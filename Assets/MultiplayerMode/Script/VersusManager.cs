using UnityEngine;
using System.Collections;
public class VersusManager : MonoBehaviour
{
	public static VersusManager Instance;

	[Header("Bộ Đề Thi (3 Màn Chơi)")]
	public LevelData[] versusLevels = new LevelData[3];

	[Header("Tham chiếu 2 Người Chơi")]
	public PlayerBoardController player1Board;
	public PlayerBoardController player2Board;

	[Header("UI Kết Quả - Player 1")]
	public GameObject blackOverlayP1; // BỔ SUNG BIẾN NÀY
	public GameObject victoryPanelP1;
	public TMPro.TextMeshProUGUI winnerTextP1;

	[Header("UI Text Chốt Hạ - Player 1")]
	public GameObject finalResultGroupP1; // Chứa 2 dòng text bên dưới
	public TMPro.TextMeshProUGUI championTextP1;
	public TMPro.TextMeshProUGUI timeTextP1;

	[Header("UI Kết Quả - Player 2")]
	public GameObject blackOverlayP2; // BỔ SUNG BIẾN NÀY
	public GameObject victoryPanelP2;
	public TMPro.TextMeshProUGUI winnerTextP2;

	[Header("UI Text Chốt Hạ - Player 2")]
	public GameObject finalResultGroupP2; // Chứa 2 dòng text bên dưới
	public TMPro.TextMeshProUGUI championTextP2;
	public TMPro.TextMeshProUGUI timeTextP2;

	private bool isGameOver = false;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// CODE DÀNH RIÊNG CHO BẢN BUILD PC: Phóng to cửa sổ thành ngang
#if UNITY_STANDALONE || UNITY_EDITOR
		// 1920x1080, chế độ Windowed (có viền) hoặc FullScreenWindow (tràn viền)
		Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
#endif
		if (versusLevels.Length >= 5)
		{
			player1Board.InitBoard(0);
			player2Board.InitBoard(0);
		}
		else
		{
			Debug.LogError("Hy ơi, nhớ kéo đủ 3 file ScriptableObject vào ô Versus Levels nhé!");
		}
	}

	public void OnPlayerCompleteLevel(int playerID, int completedLevelIndex)
	{
		if (isGameOver) return;

		int nextLevelIndex = completedLevelIndex + 1;

		if (nextLevelIndex >= 3)
		{
			DeclareWinner(playerID);
			return;
		}

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
		string finalTime = "00:00";

		VersusTimer timer = GetComponent<VersusTimer>();
		if (timer != null)
		{
			timer.StopTimer();
			finalTime = timer.GetFinalTime();

			PlayerPrefs.SetString("WinnerTime", finalTime);
			PlayerPrefs.SetInt("WinnerID", winnerID);
			PlayerPrefs.Save();
		}

		player1Board.isLocked = true;
		player2Board.isLocked = true;

		// Bổ sung truyền finalTime vào Coroutine để in ra màn hình
		StartCoroutine(VictoryAnimationRoutine(winnerID, finalTime));
	}

	private System.Collections.IEnumerator VictoryAnimationRoutine(int winnerID, string finalTime)
	{
		if (AudioManager.instance != null) AudioManager.instance.PlayWinSound();

		// Phân luồng biến cho người thắng
		GameObject activeOverlay = (winnerID == 1) ? blackOverlayP1 : blackOverlayP2;
		GameObject activePanel = (winnerID == 1) ? victoryPanelP1 : victoryPanelP2;
		GameObject activeResultGroup = (winnerID == 1) ? finalResultGroupP1 : finalResultGroupP2;
		TMPro.TextMeshProUGUI activeChampionText = (winnerID == 1) ? championTextP1 : championTextP2;
		TMPro.TextMeshProUGUI activeTimeText = (winnerID == 1) ? timeTextP1 : timeTextP2;

		// 1. BẬT MÀN ĐEN
		if (activeOverlay != null) activeOverlay.SetActive(true);

		// 2. HIỆN CHIẾC CÚP (Phóng to nảy lên)
		if (activePanel != null)
		{
			activePanel.SetActive(true);
			RectTransform panelRect = activePanel.GetComponent<RectTransform>();
			panelRect.localScale = Vector3.zero;

			Vector3 targetScale = new Vector3(0.65f, 0.65f, 1f);
			float duration = 0.5f;
			float elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
				panelRect.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easeT);
				yield return null;
			}
			panelRect.localScale = targetScale;

			// Nghỉ 2 giây để người chơi nhìn chiếc cúp
			yield return new WaitForSeconds(2f);

			// 3. TẮT CHIẾC CÚP (Thu nhỏ thụt lùi)
			elapsed = 0f;
			float outDuration = 0.3f; // Tắt nhanh hơn lúc bật
			while (elapsed < outDuration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / outDuration;
				// Công thức Ease In Back: Giật lùi về 0
				float easeT = t * t * (2.70158f * t - 1.70158f);
				panelRect.localScale = Vector3.LerpUnclamped(targetScale, Vector3.zero, easeT);
				yield return null;
			}
			activePanel.SetActive(false);
		}

		// 4. HIỆN TEXT CHỐT HẠ (Nảy lên y hệt cúp)
		if (activeResultGroup != null)
		{
			activeResultGroup.SetActive(true);

			if (activeChampionText != null) activeChampionText.text = $"TEAM {winnerID} WIN";
			if (activeTimeText != null) activeTimeText.text = $"Time: {finalTime}";

			RectTransform resultRect = activeResultGroup.GetComponent<RectTransform>();

			// 👇 BỔ SUNG: Lưu lại tỷ lệ gốc bạn đã setup đẹp đẽ ngoài Scene
			Vector3 textTargetScale = resultRect.localScale;
			// Đề phòng lỡ đang set = 0 thì lấy mặc định là 1
			if (textTargetScale == Vector3.zero) textTargetScale = Vector3.one;

			// GIỜ MỚI ÉP NÓ VỀ 0 ĐỂ CHUẨN BỊ NẢY LÊN
			resultRect.localScale = Vector3.zero;

			float duration = 0.5f;
			float elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);

				// NẢY LÊN THEO ĐÚNG TỶ LỆ textTargetScale GỐC
				resultRect.localScale = Vector3.LerpUnclamped(Vector3.zero, textTargetScale, easeT);
				yield return null;
			}
			resultRect.localScale = textTargetScale;
		}
	}
}