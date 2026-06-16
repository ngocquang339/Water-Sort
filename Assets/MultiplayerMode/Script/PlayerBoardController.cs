using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerBoardController : MonoBehaviour
{
	public GameManager gameManager;
	private List<Bottle> busyBottles = new List<Bottle>();
	[Header("Cấu hình Người chơi")]
	public int playerID; // 1 hoặc 2
	public Camera playerCamera;
	public bool isPlayerOne;

	[Header("UI Bế Tắc")]
	public GameObject outOfMovesPopup; // Kéo bảng UI thông báo hết bước đi vào đây
	public GameObject dark_Panel;

	[Header("Hiệu ứng pháo hoa")]
	public ParticleSystem bottleDonePrefab;

	[Header("Hiệu ứng Pháo Hoa Khi Win")]
	public ParticleSystem leftConfetti;
	public ParticleSystem rightConfetti;

	[Header("Win Game Effects")]
	public GameObject blackOverlay; // Kéo Black_Overlay vào đây
	public GameObject confettiRainPrefab; // Kéo Prefab máy phát pháo giấy vào đây
	public RectTransform winPanelRect;
	public GameObject winUIPanel;

	[Header("Next Level Popup")]
	public RectTransform nextLevelPopupRect;

	[Header("Danh sách chai nước")]
	public List<Bottle> allBottles;

	[Header("Hiệu ứng Nước chảy")]
	public LineRenderer waterStream;
	public ParticleSystem waterSplashPrefab;

	[Header("Cấu hình Sinh bình")]
	public Transform bottleContainer;
	public GameObject bottlePrefab;

	[Header("Thông số Gameplay (Copy từ cũ)")]
	public float liftOffset = 1.5f;
	public bool isLocked = false;

	[Header("Cài đặt Lưới (Grid Settings)")]
	public float spacingX = 1.5f;
	public float spacingY = 2.0f;
	public int maxBottlesPerRow = 5;

	[Header("Cài đặt Animation")]
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private float pourAngle = 90f;
	[SerializeField] private float pourOffsetX = 0.8f;
	[SerializeField] private float pourOffsetY = 1.0f;

	private Bottle selectedBottle;
	private int currentLevelIndex = 0;

	// Danh sách chứa các chai thuộc về nửa màn hình này để check win
	private List<Bottle> myBottles = new List<Bottle>();

	// ==========================================
	// 1. PHẦN MỚI: NHẬN ĐỀ THI TỪ TRỌNG TÀI
	// ==========================================
	public void InitBoard(int levelIndex)
	{
		currentLevelIndex = levelIndex;
		selectedBottle = null;
		isLocked = false;

		// Dọn dẹp chai cũ
		foreach (Transform child in bottleContainer) { Destroy(child.gameObject); }
		myBottles.Clear();

		// Kéo Data và sinh bình mới
		LevelData levelData = VersusManager.Instance.versusLevels[levelIndex];
		GenerateLevel(levelData);
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

	private IEnumerator PourWaterRoutine(Bottle source, Bottle target, Vector3 groundPos)
	{
		// 1. KIỂM TRA ĐIỀU KIỆN TRƯỚC KHI BAY
		if (target.isFull() || source.isEmpty() || (!target.isEmpty() && target.getTopColor().Peek() != source.getTopColor().Peek()))
		{
			yield return StartCoroutine(AnimateBottle(source.transform, groundPos, 0f, moveSpeed));
			yield break;
		}
		busyBottles.Add(source);
		busyBottles.Add(target);

		// 2. BAY ĐẾN VỊ TRÍ RÓT
		float direction = Mathf.Sign(target.transform.position.x - source.transform.position.x);
		float targetAngle = direction > 0 ? -pourAngle : pourAngle;
		Vector3 pourPosition = target.transform.position + new Vector3(-direction * pourOffsetX, pourOffsetY, 0f);
		yield return StartCoroutine(AnimateBottle(source.transform, pourPosition, targetAngle, moveSpeed));

		// 3. TÍNH TOÁN "TRƯỚC" XEM SẼ RÓT ĐƯỢC BAO NHIÊU KHỐI NƯỚC
		Stack<WaterColor> colorStack = source.getTopColor();
		int amountToPour = colorStack.Count;
		int spaceInTarget = target.capacity - target.currentWaterCount;
		int actualPourAmount = Mathf.Min(amountToPour, spaceInTarget); // Lấy số lượng thực tế có thể rót

		if (actualPourAmount > 0)
		{
			Color unityColor = source.GetUnityColor(colorStack.Peek());

			// ---- CHUẨN BỊ THÔNG SỐ ĐỂ CHẠY ANIMATION ----
			int srcStartCount = source.currentWaterCount;
			int tgtStartCount = target.currentWaterCount;
			int srcEndCount = srcStartCount - actualPourAmount;
			int tgtEndCount = tgtStartCount + actualPourAmount;

			Vector3[] tgtOrigScales = new Vector3[actualPourAmount];
			Vector3[] srcOrigScales = new Vector3[actualPourAmount];

			for (int i = 0; i < actualPourAmount; i++)
			{
				// Bật trước các cục nước ở chai Target, tô màu và ÉP CHIỀU CAO VỀ 0
				var tgtRend = target.waterLayerRenderers[tgtStartCount + i];
				tgtOrigScales[i] = tgtRend.transform.localScale; // Lưu lại kích thước chuẩn
				tgtRend.gameObject.SetActive(true);
				tgtRend.color = unityColor;
				tgtRend.transform.localScale = new Vector3(tgtOrigScales[i].x, 0f, tgtOrigScales[i].z);

				// Lưu lại kích thước chuẩn của chai Source để bóp nhỏ dần
				var srcRend = source.waterLayerRenderers[srcStartCount - 1 - i];
				srcOrigScales[i] = srcRend.transform.localScale;
			}

			// Bật Tia nước và Hạt nước
			Vector3 splashPos = target.ovalInsideRenderer.transform.position;
			waterStream.gameObject.SetActive(true);
			waterStream.startColor = unityColor;
			waterStream.endColor = unityColor;

			ParticleSystem splash = Instantiate(waterSplashPrefab, splashPos, Quaternion.identity);
			var mainModule = splash.main;
			mainModule.startColor = unityColor;
			splash.Play();

			// ========================================================
			// VÒNG LẶP MA THUẬT: ĐỒNG BỘ THỜI GIAN THỰC (0.4 GIÂY)
			// ========================================================
			// 1. TÍNH TOÁN THỜI GIAN RÓT LINH HOẠT
			float timePerLayer = 0.25f; // Thời gian rót cho 1 lớp nước (Bạn có thể tăng giảm tùy ý)

			// Tổng thời gian = Số lớp nước x Thời gian 1 lớp
			// Nếu rót 1 lớp: 1 * 0.25 = 0.25 giây. Nếu rót 3 lớp: 3 * 0.25 = 0.75 giây.
			float pourDuration = actualPourAmount * timePerLayer;

			float timePassed = 0f;
			// 2. BẬT TIẾNG RÓT NƯỚC BẮT ĐẦU TỪ GIÂY HAY NHẤT (Ví dụ: Giây 5.5)
			if (AudioManager.instance != null) AudioManager.instance.StartPourSound();

			while (timePassed < pourDuration)
			{
				timePassed += Time.deltaTime;
				float percent = timePassed / pourDuration;

				// A. Kéo giãn/Thu nhỏ các khối nước (NỐI TIẾP NHAU)
				float totalProgress = percent * actualPourAmount; // Nhân rộng tiến trình theo số lớp

				for (int i = 0; i < actualPourAmount; i++)
				{
					// Bí quyết: Tính toán phần trăm chạy cho TỪNG lớp
					// Lớp i=1 sẽ bị kìm ở mức 0 cho đến khi lớp i=0 chạy được 100%
					float layerProgress = Mathf.Clamp01(totalProgress - i);

					// Chai Source: Thu nhỏ từ Scale Gốc -> 0 (Lớp trên cùng xẹp trước)
					var srcRend = source.waterLayerRenderers[srcStartCount - 1 - i];
					srcRend.transform.localScale = new Vector3(srcOrigScales[i].x, Mathf.Lerp(srcOrigScales[i].y, 0f, layerProgress), srcOrigScales[i].z);

					// Chai Target: Kéo dài từ 0 -> Scale Gốc (Lớp dưới cùng mọc trước)
					var tgtRend = target.waterLayerRenderers[tgtStartCount + i];
					tgtRend.transform.localScale = new Vector3(tgtOrigScales[i].x, Mathf.Lerp(0f, tgtOrigScales[i].y, layerProgress), tgtOrigScales[i].z);
				}

				// B. Cho mặt Oval trôi lên/xuống cực mượt (GIỮ NGUYÊN TỌA ĐỘ X VÀ Z GỐC)
				float srcCurrentY = Mathf.Lerp(source.GetOvalYPosition(srcStartCount), source.GetOvalYPosition(srcEndCount), percent);
				float tgtCurrentY = Mathf.Lerp(target.GetOvalYPosition(tgtStartCount), target.GetOvalYPosition(tgtEndCount), percent);

				// Sửa chai Source
				Vector3 srcPos = source.ovalInsideRenderer.transform.parent.localPosition;
				srcPos.y = srcCurrentY; // Chỉ ghi đè trục Y
				source.ovalInsideRenderer.transform.parent.localPosition = srcPos;

				// Sửa chai Target
				Vector3 tgtPos = target.ovalInsideRenderer.transform.parent.localPosition;
				tgtPos.y = tgtCurrentY; // Chỉ ghi đè trục Y
				target.ovalInsideRenderer.transform.parent.localPosition = tgtPos;

				// C. Tia nước và bọt biển chạy theo mặt Oval Target đang dâng lên
				waterStream.SetPosition(0, source.mouthPoint.position);
				waterStream.SetPosition(1, target.ovalInsideRenderer.transform.position);
				splash.transform.position = target.ovalInsideRenderer.transform.position;

				yield return null; // Chờ frame tiếp theo
			}
			// ========================================================
			// 4. CHÍNH XÁC LÚC NƯỚC DỪNG CHẢY -> TẮT ÂM THANH NGAY LẬP TỨC!
			if (AudioManager.instance != null) AudioManager.instance.StopPourSound();

			// Dừng bắn thêm hạt mới, nhưng để các hạt cũ rơi tự nhiên
			splash.Stop();

			// Hẹn giờ 1.5 giây sau mới xóa object để dọn rác
			Destroy(splash.gameObject, 1.5f);

			// Dọn dẹp: Trả lại kích thước gốc cho Prefab để lần sau không bị lỗi tàng hình
			for (int i = 0; i < actualPourAmount; i++)
			{
				source.waterLayerRenderers[srcStartCount - 1 - i].transform.localScale = srcOrigScales[i];
				target.waterLayerRenderers[tgtStartCount + i].transform.localScale = tgtOrigScales[i];
			}

			// 4. CHỐT SỔ LOGIC SAU KHI ĐÃ NHÌN THẤY NƯỚC CHẢY XONG
			int poured = target.addNewColor(colorStack);
			source.removeTopColor(poured);

			source.updateBottleVisuals();
			target.updateBottleVisuals();
		}

		waterStream.gameObject.SetActive(false); // Tắt tia nước

		// 5. BAY VỀ MẶT ĐẤT VÀ KẾT THÚC
		yield return StartCoroutine(AnimateBottle(source.transform, groundPos, 0f, moveSpeed));

		busyBottles.Remove(source);
		busyBottles.Remove(target);

		if (target.isCompleted())
		{
			Debug.Log("Chai này đã hoàn thiện");
			Instantiate(bottleDonePrefab, target.mouthPoint.position, Quaternion.identity);
			target.CloseCork();
		}
		bool check = CheckWin();
		//Check nước đi hợp lệ
		if (!check) CheckGameState();
	}

	public void CheckGameState()
	{
		// Kiểm tra Win trước (Giả sử bạn có hàm CheckWin)
		// if (CheckWinCondition()) return; 

		// Nếu không Win, kiểm tra xem có bị Deadlock không
		if (!HasAnyValidMove() && !CheckWin())
		{
			Debug.Log("Hết bước đi! Chuẩn bị hiện thông báo...");
			StartCoroutine(HandleDeadlockRoutine());
		}
	}

	private IEnumerator HandleDeadlockRoutine()
	{
		isLocked = true;
		// Đợi 1.5 giây để hiệu ứng rót nước cuối cùng kịp chạy xong
		yield return new WaitForSeconds(1.5f);

		// Hiện bảng UI thông báo (Ví dụ: "Bạn đã hết bước đi! Dùng +1 Bình hoặc Xem Quảng Cáo để Undo")
		if (outOfMovesPopup != null && !HasAnyValidMove())
		{
			StartCoroutine(popupCoroutine(dark_Panel, outOfMovesPopup));
		}
	}

	private IEnumerator popupCoroutine(GameObject darkPanel, GameObject popup)
	{
		if (darkPanel != null)
		{
			darkPanel.SetActive(true);
		}
		// 1. Bật object lên và ép kích thước về 0
		popup.SetActive(true);
		RectTransform popupRect = popup.GetComponent<RectTransform>();
		// --- THÊM DÒNG NÀY: Ép nó về chính giữa màn hình (Tọa độ 0, 0) ---
		popupRect.anchoredPosition = Vector2.zero;
		popupRect.localScale = Vector3.zero;

		// 2. Cài đặt thời gian phóng to (0.5 giây)
		float duration = 0.5f;
		float elapsed = 0f;
		bool check = HasAnyValidMove();
		if (SceneManager.GetActiveScene().name == "MainScene")
		{
			AudioManager.instance.PlayPopupSound();
		}
		else if (!check)
		{
			AudioManager.instance.PlayGameOver();
		}
		else
		{
			AudioManager.instance.PlayPopupSound();
		}
		// 3. Vòng lặp Animation
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;

			// Công thức nảy Juicy (Ease Out Back) mượn từ WinPanel
			float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);

			popupRect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeT);
			yield return null;
		}

		// 4. Chốt hạ kích thước chuẩn tránh sai số
		popupRect.localScale = Vector3.one;
	}


	public bool HasAnyValidMove()
	{
		for (int i = 0; i < allBottles.Count; i++)
		{
			Bottle fromBottle = allBottles[i];

			// Nếu chai rỗng hoặc đã hoàn thiện rồi -> Không rót đi nữa
			if (fromBottle.isEmpty() || fromBottle.isCompleted()) continue;

			// Lấy màu trên cùng của chai nguồn
			// (Do hàm getTopColor của bạn trả về Stack, ta dùng Peek() để lấy màu thật)
			WaterColor colorToPour = fromBottle.getTopColor().Peek();

			for (int j = 0; j < allBottles.Count; j++)
			{
				if (i == j) continue; // Không tự kiểm tra với chính mình

				Bottle toBottle = allBottles[j];

				// Nếu chai đích đầy -> Không nhận được
				if (toBottle.isFull()) continue;

				// Nếu chai đích RỖNG hoặc có MÀU TRÊN CÙNG GIỐNG NHAU -> Có thể rót!
				if (toBottle.isEmpty() || toBottle.getTopColor().Peek() == colorToPour)
				{
					return true; // Chỉ cần tìm thấy 1 đường đi là trả về True ngay
				}
			}
		}

		// Nếu chạy hết vòng lặp mà không return true, nghĩa là HẾT ĐƯỜNG
		return false;
	}
	private IEnumerator AnimateBottle(Transform bottleTransform, Vector3 targetPos, float targetRotation, float duration)
	{
		Vector3 startPos = bottleTransform.position;
		Quaternion startRot = bottleTransform.rotation;
		Quaternion endRot = Quaternion.Euler(0, 0, targetRotation);

		float timePassed = 0f;

		while (timePassed < duration)
		{
			timePassed += Time.deltaTime;
			float percent = timePassed / duration;

			bottleTransform.position = Vector3.Lerp(startPos, targetPos, percent);
			bottleTransform.rotation = Quaternion.Lerp(startRot, endRot, percent);

			yield return null;
		}

		bottleTransform.position = targetPos;
		bottleTransform.rotation = endRot;
	}

	// ==========================================
	// 3. PHẦN MỚI TỐI ƯU: ĐIỀU KHIỂN CẢM ỨNG 
	// ==========================================
	void Update()
	{
		HandleMultiTouchInput();
	}

	private void HandleMultiTouchInput()
	{
		if (isLocked) return;

		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.phase == TouchPhase.Began)
			{
				bool isInCorrectHalf = isPlayerOne ? (touch.position.x < Screen.width / 2f) : (touch.position.x >= Screen.width / 2f);

				if (isInCorrectHalf)
				{
					if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;
					ProcessBottleAction(touch.position);
				}
			}
		}
	}

	private void ProcessBottleAction(Vector2 touchPosition)
	{
		Bottle clickBottle = getBottleFromTouch(touchPosition);
		if (clickBottle != null)
		{
			if (clickBottle == selectedBottle)
			{
				Vector3 groundPos = clickBottle.transform.position - new Vector3(0f, liftOffset, 0f);
				if (AudioManager.instance != null) AudioManager.instance.PlayBottleDown();
				StartCoroutine(AnimateBottle(clickBottle.transform, groundPos, 0f, moveSpeed));
				selectedBottle = null;
			}
			else if (selectedBottle == null)
			{
				if (AudioManager.instance != null) AudioManager.instance.PlayBottleLift();
				if (clickBottle.getTopColor() == null) return;

				Vector3 liftPos = clickBottle.transform.position + new Vector3(0f, liftOffset, 0f);
				StartCoroutine(AnimateBottle(clickBottle.transform, liftPos, 0f, moveSpeed));
				selectedBottle = clickBottle;
			}
			else
			{
				Vector3 sourceGroundPos = selectedBottle.transform.position - new Vector3(0f, liftOffset, 0f);
				StartCoroutine(PourWaterRoutine(selectedBottle, clickBottle, sourceGroundPos));
				selectedBottle = null;
			}
		}
	}

	private Bottle getBottleFromTouch(Vector2 screenPosition)
	{
		Vector2 worldPosition = playerCamera.ScreenToWorldPoint(screenPosition);
		RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
		if (hit.collider != null) return hit.collider.GetComponent<Bottle>();
		return null;
	}

	// ==========================================
	// 4. PHẦN COPY TỪ CŨ: COROUTINE DI CHUYỂN & RÓT NƯỚC
	// ==========================================
	// BẠN PASTE HÀM IEnumerator AnimateBottle() TỪ CODE CŨ VÀO ĐÂY

	// BẠN PASTE HÀM IEnumerator PourWaterRoutine() TỪ CODE CŨ VÀO ĐÂY
	// [!!! LƯU Ý QUAN TRỌNG !!!] Ở cuối hàm PourWaterRoutine, 
	// sau khi nước rót xong, bạn phải gọi hàm CheckWinCondition();

	// ==========================================
	// 5. PHẦN COPY TỪ CŨ: KIỂM TRA THẮNG
	// ==========================================
	public bool CheckWin()
	{
		bool isWin = true;
		foreach (Bottle bottle in allBottles)
		{
			if (!bottle.isEmpty() && !bottle.isCompleted())
			{
				isWin = false;
				break;
			}
		}

		if (isWin)
		{
			StartCoroutine(HandleWinCoroutine());
			return true;
		}
		return false;

		if (isWin)
		{
			Debug.Log($"Player {playerID} đã giải xong!");
			VersusManager.Instance.OnPlayerCompleteLevel(playerID, currentLevelIndex);
		}
	}

	private IEnumerator HandleWinCoroutine()
	{
		yield return StartCoroutine(WinSequenceRoutine());
		SaveProgress();
	}

	private void SaveProgress()
	{
		int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
		PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
		PlayerPrefs.SetInt("LevelNumber", currentLevel + 1);
		PlayerPrefs.Save();
	}

	private IEnumerator WinSequenceRoutine()
	{
		if (AudioManager.instance != null) AudioManager.instance.PlayWinSound();
		// 1. CHUẨN BỊ MÀN HÌNH
		if (blackOverlay != null) blackOverlay.SetActive(true);

		winUIPanel.SetActive(true);
		winPanelRect.localScale = Vector3.zero;

		// Đảm bảo Popup Next Level đang bị ẩn/thu nhỏ từ đầu
		if (nextLevelPopupRect != null)
		{
			nextLevelPopupRect.gameObject.SetActive(true);
			nextLevelPopupRect.localScale = Vector3.zero;
		}

		Vector3 topCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10f));
		if (confettiRainPrefab != null)
		{
			Instantiate(confettiRainPrefab, topCenter, confettiRainPrefab.transform.rotation);
		}

		// 2. PHÓNG TO WIN_PANEL ("LEVEL COMPLETE")
		float duration = 0.5f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
			winPanelRect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeT);
			yield return null;
		}
		winPanelRect.localScale = Vector3.one;

		// 3. Thời gian hiển thị panel
		yield return new WaitForSeconds(0.8f);

		// 4. THU NHỎ WIN_PANEL XUỐNG BẰNG 0
		elapsed = 0f;
		float outDuration = 0.3f;
		while (elapsed < outDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / outDuration;
			float easeT = t * t * (2.70158f * t - 1.70158f);
			winPanelRect.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, easeT);
			yield return null;
		}
		// Thu nhỏ xong thì tắt hẳn cái bảng Level Complete đi cho nhẹ máy
		winPanelRect.gameObject.SetActive(false);

		// 6. PHÓNG TO POPUP CÚP VÀNG & NÚT BẤM (NEXT LEVEL POPUP)
		if (nextLevelPopupRect != null)
		{
			elapsed = 0f;
			leftConfetti.Play();
			rightConfetti.Play();
			while (elapsed < duration) // Vẫn dùng thời gian duration = 0.5f
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				// Công thức nảy Juicy y hệt lúc nãy
				float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
				nextLevelPopupRect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeT);
				yield return null;
			}
			nextLevelPopupRect.localScale = Vector3.one; // Chốt hạ
		}
		// Truyền số Level hiện tại vào. Unity sẽ tự so sánh, nếu cao hơn điểm cũ nó mới lưu.
		int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
	}
}