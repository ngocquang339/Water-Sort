using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBoardController : MonoBehaviour
{
	private BottleMulti selectedBottle;
	private List<BottleMulti> busyBottles = new List<BottleMulti>();
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
	public RectTransform winPanelRect;
	public GameObject winUIPanel;

	[Header("Next Level Popup")]
	public RectTransform nextLevelPopupRect;

	[Header("Danh sách chai nước")]
	public List<BottleMulti> allBottles;

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

	[Header("Cài đặt Game")]
	public LevelManagerMultiplayer levelManager;

	[Header("Hiệu ứng chuyển màn (Fade Nửa Màn Hình)")]
	public Image localFadeImage;
	public float fadeDuration = 0.5f;
	private int currentLevelIndex = 0;

	// Danh sách chứa các chai thuộc về nửa màn hình này để check win
	private List<Bottle> myBottles = new List<Bottle>();


	void Update()
	{
		if (isLocked) return;

		if (Input.touchCount > 0)
		{
			HandleMultiTouchInput();
		}
		else
		{
			liftBottle(); // Giữ nguyên hàm liftBottle của bạn để test bằng chuột
		}
	}
	// ==========================================
	// 1. PHẦN MỚI: NHẬN ĐỀ THI TỪ TRỌNG TÀI
	// ==========================================
	public void InitBoard(int levelIndex)
	{
		currentLevelIndex = levelIndex;
		selectedBottle = null;
		isLocked = false;

		busyBottles.Clear();
		allBottles.Clear();

		// Dọn dẹp chai cũ
		foreach (Transform child in bottleContainer) { Destroy(child.gameObject); }
		myBottles.Clear();

		// Kéo Data
		LevelData levelData = VersusManager.Instance.versusLevels[levelIndex];
		LevelManagerMultiplayer.Instance.GeneratePlayerLevel(levelData, bottleContainer, playerID, this);
	}

	private IEnumerator PourWaterRoutine(BottleMulti source, BottleMulti target, Vector3 groundPos)
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
		int actualPourAmount = Mathf.Min(amountToPour, spaceInTarget);

		if (actualPourAmount > 0)
		{
			Color unityColor = source.GetUnityColor(colorStack.Peek());

			// ---- CHUẨN BỊ THÔNG SỐ ĐỂ CHẠY ANIMATION ----
			int srcStartCount = source.currentWaterCount;
			int tgtStartCount = target.currentWaterCount;
			int srcEndCount = srcStartCount - actualPourAmount;
			int tgtEndCount = tgtStartCount + actualPourAmount;

			// 👇 KHÔI PHỤC: Mảng lưu trữ kích thước gốc của các lớp nước
			Vector3[] tgtOrigScales = new Vector3[actualPourAmount];
			Vector3[] srcOrigScales = new Vector3[actualPourAmount];

			for (int i = 0; i < actualPourAmount; i++)
			{
				// Chai Target: Bật lên, tô màu và ép chiều cao Y về 0
				var tgtRend = target.waterLayerRenderers[tgtStartCount + i];
				tgtOrigScales[i] = tgtRend.transform.localScale;
				tgtRend.gameObject.SetActive(true);
				tgtRend.color = unityColor;
				tgtRend.transform.localScale = new Vector3(tgtOrigScales[i].x, 0f, tgtOrigScales[i].z);

				// Chai Source: Lưu lại kích thước gốc để bóp nhỏ dần
				var srcRend = source.waterLayerRenderers[srcStartCount - 1 - i];
				srcOrigScales[i] = srcRend.transform.localScale;
			}

			// Bật mặt Oval chai đích lên trước (Đề phòng chai đang rỗng bị ẩn)
			if (!target.ovalInsideRenderer.gameObject.activeSelf)
			{
				target.ovalInsideRenderer.gameObject.SetActive(true);
				target.ovalBorderRenderer.gameObject.SetActive(true);

				float h, s, v;
				Color.RGBToHSV(unityColor, out h, out s, out v);
				s = Mathf.Clamp01(s - 0.2f);
				v = Mathf.Clamp01(v + 0.3f);
				target.ovalInsideRenderer.color = Color.HSVToRGB(h, s, v);
				target.ovalBorderRenderer.color = Color.white;
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
			// VÒNG LẶP MA THUẬT: ĐỒNG BỘ THỜI GIAN THỰC
			// ========================================================
			float timePerLayer = 0.25f;
			float pourDuration = actualPourAmount * timePerLayer;
			float timePassed = 0f;

			if (AudioManager.instance != null) AudioManager.instance.StartPourSound();

			while (timePassed < pourDuration)
			{
				timePassed += Time.deltaTime;
				float percent = timePassed / pourDuration;

				float totalProgress = percent * actualPourAmount;

				float srcCurrentY = source.GetOvalYPosition(srcStartCount);
				float tgtCurrentY = target.GetOvalYPosition(tgtStartCount);

				// A. Kéo giãn/Thu nhỏ các khối nước (NỐI TIẾP NHAU)
				for (int i = 0; i < actualPourAmount; i++)
				{
					float layerProgress = Mathf.Clamp01(totalProgress - i);

					// 👇 KHÔI PHỤC: Ép scale Y để tạo hiệu ứng rút/dâng nước
					// Chai Source: Thu nhỏ từ Scale Gốc -> 0
					var srcRend = source.waterLayerRenderers[srcStartCount - 1 - i];
					srcRend.transform.localScale = new Vector3(srcOrigScales[i].x, Mathf.Lerp(srcOrigScales[i].y, 0f, layerProgress), srcOrigScales[i].z);

					// Chai Target: Kéo dài từ 0 -> Scale Gốc
					var tgtRend = target.waterLayerRenderers[tgtStartCount + i];
					tgtRend.transform.localScale = new Vector3(tgtOrigScales[i].x, Mathf.Lerp(0f, tgtOrigScales[i].y, layerProgress), tgtOrigScales[i].z);

					// ĐỒNG BỘ MẶT OVAL TỪNG NẤC
					if (layerProgress > 0f)
					{
						float sStartY = source.GetOvalYPosition(srcStartCount - i);
						float sEndY = source.GetOvalYPosition(srcStartCount - i - 1);
						srcCurrentY = Mathf.Lerp(sStartY, sEndY, layerProgress);

						float tStartY = target.GetOvalYPosition(tgtStartCount + i);
						float tEndY = target.GetOvalYPosition(tgtStartCount + i + 1);
						tgtCurrentY = Mathf.Lerp(tStartY, tEndY, layerProgress);
					}
				}

				// B. Gán tọa độ Y cho mặt Oval
				Vector3 srcPos = source.ovalInsideRenderer.transform.parent.localPosition;
				srcPos.y = srcCurrentY;
				source.ovalInsideRenderer.transform.parent.localPosition = srcPos;

				Vector3 tgtPos = target.ovalInsideRenderer.transform.parent.localPosition;
				tgtPos.y = tgtCurrentY;
				target.ovalInsideRenderer.transform.parent.localPosition = tgtPos;

				// C. Tia nước và bọt biển chạy theo mặt Oval Target đang dâng lên
				waterStream.SetPosition(0, source.mouthPoint.position);
				waterStream.SetPosition(1, target.ovalInsideRenderer.transform.position);
				splash.transform.position = target.ovalInsideRenderer.transform.position;

				yield return null; // Chờ frame tiếp theo
			}

			// ========================================================
			// 4. CHÍNH XÁC LÚC NƯỚC DỪNG CHẢY
			if (AudioManager.instance != null) AudioManager.instance.StopPourSound();
			splash.Stop();
			Destroy(splash.gameObject, 1.5f);

			// 👇 KHÔI PHỤC: Dọn dẹp trả lại kích thước chuẩn cho các lớp để không bị lỗi tàng hình
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
			BottleMulti fromBottle = allBottles[i];

			// Nếu chai rỗng hoặc đã hoàn thiện rồi -> Không rót đi nữa
			if (fromBottle.isEmpty() || fromBottle.isCompleted()) continue;

			// Lấy màu trên cùng của chai nguồn
			// (Do hàm getTopColor của bạn trả về Stack, ta dùng Peek() để lấy màu thật)
			WaterColor colorToPour = fromBottle.getTopColor().Peek();

			for (int j = 0; j < allBottles.Count; j++)
			{
				if (i == j) continue; // Không tự kiểm tra với chính mình

				BottleMulti toBottle = allBottles[j];

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

		// Chốt hạ tọa độ và góc quay chính xác khi kết thúc Animation
		bottleTransform.position = targetPos;
		bottleTransform.rotation = endRot;
	}

	// ==========================================
	// 3. PHẦN MỚI TỐI ƯU: ĐIỀU KHIỂN CẢM ỨNG 
	// ==========================================


	private void liftBottle()
	{
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !isLocked)
		{
			BottleMulti clickBottle = getBottleFromClick();
			if (clickBottle != null)
			{
				// NẾU CHAI NÀY ĐANG BẬN -> BỎ QUA
				if (busyBottles.Contains(clickBottle)) return;

				// 1. CHẠM LẠI VÀO CHAI ĐANG CHỌN -> BỎ XUỐNG
				if (clickBottle == selectedBottle)
				{
					Vector3 groundPos = clickBottle.transform.position - new Vector3(0f, liftOffset, 0f);
					AudioManager.instance.PlayBottleDown();
					StartCoroutine(AnimateBottle(clickBottle.transform, groundPos, 0f, moveSpeed));
					selectedBottle = null;
				}
				// 2. CHƯA CHỌN CHAI NÀO -> NHẤC LÊN
				else if (selectedBottle == null)
				{
					if (AudioManager.instance != null) AudioManager.instance.PlayBottleLift();
					if (clickBottle.getTopColor() == null) return;

					Vector3 liftPos = clickBottle.transform.position + new Vector3(0f, liftOffset, 0f);
					StartCoroutine(AnimateBottle(clickBottle.transform, liftPos, 0f, moveSpeed));

					selectedBottle = clickBottle;
				}
				// 3. ĐÃ CHỌN CHAI A, BẤM VÀO CHAI B -> ĐỔ NƯỚC
				else
				{
					Vector3 sourceGroundPos = selectedBottle.transform.position - new Vector3(0f, liftOffset, 0f);

					// Chạy Coroutine rót nước đã được gộp chung tia nước
					StartCoroutine(PourWaterRoutine(selectedBottle, clickBottle, sourceGroundPos));

					// Giải phóng để chọn cặp khác
					selectedBottle = null;
				}
			}
		}
	}

	private BottleMulti getBottleFromClick()
	{
		// Đổi Camera.main thành playerCamera để ăn khớp với Camera của từng nửa màn hình
		Vector2 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
		if (hit.collider != null)
		{
			return hit.collider.GetComponent<BottleMulti>();
		}
		return null;
	}


	private void HandleMultiTouchInput()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);

			// Chỉ xử lý khi ngón tay vừa chạm vào màn hình
			if (touch.phase == TouchPhase.Began)
			{
				// Logic chia đôi màn hình cho 2 người chơi của bạn
				bool isInCorrectHalf = isPlayerOne ? (touch.position.x < Screen.width / 2f) : (touch.position.x >= Screen.width / 2f);

				if (isInCorrectHalf)
				{
					// Tránh lỗi bấm xuyên qua UI (ví dụ chạm vào nút Pause mà chai vẫn nhấc lên)
					if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;

					ProcessBottleAction(touch.position);
				}
			}
		}
	}

	private void ProcessBottleAction(Vector2 touchPosition)
	{
		BottleMulti clickBottle = getBottleFromTouch(touchPosition);
		if (clickBottle != null)
		{
			// QUAN TRỌNG: Chặn thao tác nếu chai đang bận chạy animation rót nước
			if (busyBottles.Contains(clickBottle)) return;

			// 1. Chạm lại vào chai đang chọn -> Đặt xuống
			if (clickBottle == selectedBottle)
			{
				Vector3 groundPos = clickBottle.transform.position - new Vector3(0f, liftOffset, 0f);
				if (AudioManager.instance != null) AudioManager.instance.PlayBottleDown();
				StartCoroutine(AnimateBottle(clickBottle.transform, groundPos, 0f, moveSpeed));
				selectedBottle = null;
			}
			// 2. Chưa chọn chai nào -> Nhấc lên
			else if (selectedBottle == null)
			{
				if (AudioManager.instance != null) AudioManager.instance.PlayBottleLift();
				if (clickBottle.getTopColor() == null) return;

				Vector3 liftPos = clickBottle.transform.position + new Vector3(0f, liftOffset, 0f);
				StartCoroutine(AnimateBottle(clickBottle.transform, liftPos, 0f, moveSpeed));
				selectedBottle = clickBottle;
			}
			// 3. Đã chọn chai A, chạm vào chai B -> Rót nước
			else
			{
				Vector3 sourceGroundPos = selectedBottle.transform.position - new Vector3(0f, liftOffset, 0f);
				StartCoroutine(PourWaterRoutine(selectedBottle, clickBottle, sourceGroundPos));
				selectedBottle = null;
			}
		}
	}

	private BottleMulti getBottleFromTouch(Vector2 screenPosition)
	{
		Vector2 worldPosition = playerCamera.ScreenToWorldPoint(screenPosition);
		RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
		if (hit.collider != null)
		{
			return hit.collider.GetComponent<BottleMulti>();
		}
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
		foreach (BottleMulti bottle in allBottles)
		{
			if (!bottle.isEmpty() && !bottle.isCompleted())
			{
				isWin = false;
				break;
			}
		}

		if (isWin)
		{
			// Thêm dòng này để KHÓA nửa màn hình của người đã giải xong (Người kia vẫn vuốt được)
			isLocked = true;
			StartCoroutine(HandleWinCoroutine());
			return true;
		}
		return false;

		// HÃY XÓA TOÀN BỘ ĐOẠN NÀY ĐI VÌ NÓ BỊ NẰM DƯỚI RETURN (SẼ KHÔNG BAO GIỜ CHẠY):
		// if (isWin) {
		//    Debug.Log($"Player {playerID} đã giải xong!");
		//    VersusManager.Instance.OnPlayerCompleteLevel(playerID, currentLevelIndex);
		// }
	}

	private IEnumerator HandleWinCoroutine()
	{
		isLocked = true; // Khóa cảm ứng của nửa màn hình này lại

		if (AudioManager.instance != null) AudioManager.instance.PlayWinSound();

		// 1. Tùy chọn: Bắn pháo hoa ăn mừng nhẹ nhàng cho vui mắt
		if (leftConfetti != null) leftConfetti.Play();
		if (rightConfetti != null) rightConfetti.Play();

		// 2. Chờ 1.5 giây để người chơi nhìn ngắm các chai nước đã hoàn thành
		yield return new WaitForSeconds(1.5f);

		// 3. Kiểm tra xem đây có phải là màn cuối cùng của bộ đề thi không?
		int totalLevels = VersusManager.Instance.versusLevels.Length;
		bool isFinalLevel = (currentLevelIndex >= totalLevels - 1);

		if (isFinalLevel)
		{
			// MÀN CUỐI CÙNG: Báo thẳng cho Trọng Tài để hiện bảng "CHIẾN THẮNG CHUNG CUỘC" giữa màn hình
			VersusManager.Instance.OnPlayerCompleteLevel(playerID, currentLevelIndex);
		}
		else
		{
			// MÀN TRUNG GIAN: Dùng hiệu ứng kéo rèm để chuyển qua màn tiếp theo một cách mượt mà

			// Kéo rèm đen che kín nửa màn hình
			yield return StartCoroutine(LocalFadeOut());

			// Báo Trọng Tài. Trọng tài sẽ lập tức dọn chai cũ và đẻ chai mới (InitBoard) ở phía sau tấm rèm đen
			VersusManager.Instance.OnPlayerCompleteLevel(playerID, currentLevelIndex);

			// (Phòng hờ InitBoard mở khóa quá sớm, ta khóa lại cho chắc)
			isLocked = true;

			// Mở rèm đen ra. Lúc này bộ chai nước mới đã được xếp sẵn sàng!
			yield return StartCoroutine(LocalFadeIn());

			// Mở khóa để người chơi bắt đầu giải màn mới
			isLocked = false;
		}
	}

	// ========================================================
	// 2 HÀM PHỤ TRỢ: CHỈ KÉO RÈM Ở NỬA MÀN HÌNH HIỆN TẠI
	// ========================================================
	private IEnumerator LocalFadeOut()
	{
		if (localFadeImage != null)
		{
			localFadeImage.gameObject.SetActive(true);
			localFadeImage.raycastTarget = true; // Chặn mọi thao tác bấm bậy
			float timer = 0f;
			Color c = localFadeImage.color;
			while (timer < fadeDuration)
			{
				timer += Time.deltaTime;
				c.a = Mathf.Clamp01(timer / fadeDuration);
				localFadeImage.color = c;
				yield return null;
			}
		}
	}

	private IEnumerator LocalFadeIn()
	{
		if (localFadeImage != null)
		{
			float timer = 0f;
			Color c = localFadeImage.color;
			while (timer < fadeDuration)
			{
				timer += Time.deltaTime;
				c.a = 1f - Mathf.Clamp01(timer / fadeDuration);
				localFadeImage.color = c;
				yield return null;
			}
			localFadeImage.raycastTarget = false;
			localFadeImage.gameObject.SetActive(false);
		}
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
		if (blackOverlay != null) blackOverlay.SetActive(true);

		winUIPanel.SetActive(true);
		winPanelRect.localScale = Vector3.zero;
		winPanelRect.anchoredPosition = Vector2.zero; // Ép về giữa

		if (nextLevelPopupRect != null)
		{
			nextLevelPopupRect.gameObject.SetActive(true);
			nextLevelPopupRect.localScale = Vector3.zero;
			nextLevelPopupRect.anchoredPosition = Vector2.zero; // Ép về giữa

		}

		// =======================================================
		// BỔ SUNG: Tạo một tỷ lệ mục tiêu nhỏ hơn (Ví dụ 0.65 = 65% kích thước gốc)
		// Bạn có thể tự tăng giảm 0.65f thành 0.7f hoặc 0.5f cho vừa mắt
		// =======================================================
		Vector3 targetScale = new Vector3(0.65f, 0.65f, 1f);

		float duration = 0.5f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);

			// THAY Vector3.one BẰNG targetScale
			winPanelRect.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easeT);
			yield return null;
		}

		// CHỐT HẠ BẰNG targetScale
		winPanelRect.localScale = targetScale;
		

		yield return new WaitForSeconds(0.8f);

		// THU NHỎ LẠI TỪ targetScale VỀ 0
		elapsed = 0f;
		float outDuration = 0.3f;
		while (elapsed < outDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / outDuration;
			float easeT = t * t * (2.70158f * t - 1.70158f);

			// THAY Vector3.one BẰNG targetScale
			winPanelRect.localScale = Vector3.LerpUnclamped(targetScale, Vector3.zero, easeT);
			yield return null;
		}
		winPanelRect.gameObject.SetActive(false);

		// PHÓNG TO POPUP NEXT LEVEL CŨNG DÙNG targetScale
		if (nextLevelPopupRect != null)
		{
			elapsed = 0f;
			leftConfetti.Play();
			rightConfetti.Play();
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);

				// THAY Vector3.one BẰNG targetScale
				nextLevelPopupRect.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easeT);
				yield return null;
			}

			// CHỐT HẠ BẰNG targetScale
			nextLevelPopupRect.localScale = targetScale;
		}
		int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
	}

	// HÀM NÀY GẮN VÀO NÚT RELOAD TRÊN POPUP
	public void OnClickReloadLevel()
	{
		// 1. Tắt các Popup che màn hình đi
		if (outOfMovesPopup != null) outOfMovesPopup.SetActive(false);
		if (dark_Panel != null) dark_Panel.SetActive(false);

		// (Nếu bạn có làm hàm ClosePopupCoroutine thì gọi nó thay cho SetActive cũng được)

		// 2. Gọi lại hàm InitBoard với chính Level Index hiện tại
		InitBoard(currentLevelIndex);
		// 3. Mở khóa cảm ứng để chơi lại
		isLocked = false;
	}
}