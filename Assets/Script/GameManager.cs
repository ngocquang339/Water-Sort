using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum HelpType { Undo, Hint, AddBottle }
public class GameManager : MonoBehaviour
{
	private Bottle selectedBottle;
	private Stack<stepInfor> saveStepInfor = new Stack<stepInfor>();

	private List<Bottle> busyBottles = new List<Bottle>();
	public static GameManager instance;

	[Header("Cài đặt Game")]
	[SerializeField] private float liftOffset = 0.5f;
	public TextMeshProUGUI levelText;
	public LevelManager levelManager;
	public GameHintManager gameHintManager;

	[Header("Hiệu ứng Nước chảy")]
	public LineRenderer waterStream;
	public ParticleSystem waterSplashPrefab;

	[Header("Cài đặt Animation")]
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private float pourAngle = 90f;
	[SerializeField] private float pourOffsetX = 0.8f;
	[SerializeField] private float pourOffsetY = 1.0f;

	[Header("Danh sách chai nước")]
	public List<Bottle> allBottles;

	[Header("UI Bế Tắc")]
	public GameObject outOfMovesPopup; // Kéo bảng UI thông báo hết bước đi vào đây

	[Header("Hiệu ứng pháo hoa")]
	public ParticleSystem bottleDonePrefab;

	[Header("Win Game Effects")]
	public GameObject blackOverlay; // Kéo Black_Overlay vào đây
	public GameObject confettiRainPrefab; // Kéo Prefab máy phát pháo giấy vào đây
	public RectTransform winPanelRect;
	public GameObject winUIPanel;

	[Header("Next Level Popup")]
	public RectTransform nextLevelPopupRect; // Kéo object NextLevel_Popup vào đây

	[Header("UI Hiển Thị Số Lượt Trợ Giúp")]
	public TextMeshProUGUI undoText;       // Kéo Text số của nút Undo vào đây
	public TextMeshProUGUI hintText;       // Kéo Text số của nút Hint vào đây
	public TextMeshProUGUI addBottleText;  // Kéo Text số của nút Thêm Chai vào đây

	[Header("Cài đặt Thêm Chai")]
	public int maxExtraBottlesPerLevel = 1; // Giới hạn số chai được thêm mỗi màn
	private int extraBottlesUsedThisLevel = 0; // Đếm số chai đã thêm trong màn hiện tại
	public GameObject emptyBottlePrefab; // Kéo Prefab chai rỗng vào đây
	public Button addBottleButton;
	// Các biến để quản lý số lượng trong Code
	private int remainingUndo;
	private int remainingHint;
	private int remainingAddBottle;

	// Đặt tên các Key lưu trữ thành hằng số để tránh gõ sai chính tả
	private const string KEY_UNDO = "Help_Undo";
	private const string KEY_HINT = "Help_Hint";
	private const string KEY_ADD_BOTTLE = "Help_AddBottle";

	public PopupManager popupManager;

	private void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		LoadHelpQuantities();
	}

	// Hàm tải dữ liệu khi vừa vào game

	void Update()
	{
		liftBottle();
	}

	private void liftBottle()
	{
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			Bottle clickBottle = getBottleFromClick();
			if (clickBottle != null)
			{
				if (AudioManager.instance != null) AudioManager.instance.PlayBottleClick();
				// NẾU CHAI NÀY ĐANG BẬN -> BỎ QUA
				if (busyBottles.Contains(clickBottle)) return;

				// 1. CHẠM LẠI VÀO CHAI ĐANG CHỌN -> BỎ XUỐNG
				if (clickBottle == selectedBottle)
				{
					Vector3 groundPos = clickBottle.transform.position - new Vector3(0f, liftOffset, 0f);
					StartCoroutine(AnimateBottle(clickBottle.transform, groundPos, 0f, moveSpeed));
					selectedBottle = null;
				}
				// 2. CHƯA CHỌN CHAI NÀO -> NHẤC LÊN
				else if (selectedBottle == null)
				{
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
			float pourDuration = 0.4f; // Bạn có thể chỉnh con số này để nước chảy nhanh/chậm
			float timePassed = 0f;

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

			saveStepInfor.Push(createStepInfor(source, target, poured));
		}

		waterStream.gameObject.SetActive(false); // Tắt tia nước

		// 5. BAY VỀ MẶT ĐẤT VÀ KẾT THÚC
		yield return StartCoroutine(AnimateBottle(source.transform, groundPos, 0f, moveSpeed));

		busyBottles.Remove(source);
		busyBottles.Remove(target);
		
		if(target.isCompleted()){
			Debug.Log("Chai này đã hoàn thiện");
			Instantiate(bottleDonePrefab, target.mouthPoint.position, Quaternion.identity);
			target.CloseCork();
		}
		CheckWin();
		//Check nước đi hợp lệ
		if(!CheckWin()) CheckGameState();
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

	private Bottle getBottleFromClick()
	{
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
		if (hit.collider != null)
		{
			return hit.collider.GetComponent<Bottle>();
		}
		return null;
	}

	public void startGame()
	{
		SceneManager.LoadScene("MainPlayScene");
	}

	// 1. Hàm siêu nhẹ để kiểm tra xem CÒN BƯỚC ĐI KHÔNG
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

	// 2. Coroutine tạo độ trễ trước khi hiện thông báo
	private IEnumerator HandleDeadlockRoutine()
	{
		// Đợi 1.5 giây để hiệu ứng rót nước cuối cùng kịp chạy xong
		yield return new WaitForSeconds(1.5f);

		// Hiện bảng UI thông báo (Ví dụ: "Bạn đã hết bước đi! Dùng +1 Bình hoặc Xem Quảng Cáo để Undo")
		if (outOfMovesPopup != null)
		{
			outOfMovesPopup.SetActive(true);
		}
	}

	// 3. Hàm kích hoạt kiểm tra (Sẽ gọi sau khi người chơi rót xong)
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

	public void backStep()
	{
		// LƯU Ý 1: Kiểm tra xem người chơi còn lượt không, hết rồi thì nghỉ khỏe
		if (remainingUndo <= 0)
		{
			Debug.Log("Hết lượt Undo rồi Hy ơi!");
			return;
		}

		if (saveStepInfor.Count > 0)
		{
			stepInfor lastStep = saveStepInfor.Peek();
			Bottle a = lastStep.A;
			Bottle b = lastStep.B;
			if (busyBottles.Contains(a) || busyBottles.Contains(b)) return;
			WaterColor color = b.getTopColor().Peek();
			for (int i = 0; i < lastStep.waterLayers; i++)
			{
				a.addWater(color);
			}
			b.removeTopColor(lastStep.waterLayers);
			a.updateBottleVisuals();
			b.updateBottleVisuals();
			saveStepInfor.Pop();

			// LƯU Ý 2: Sau khi đã lùi bước thành công -> Thực hiện trừ lượt và đổi số UI luôn!
			remainingUndo--;
			PlayerPrefs.SetInt(KEY_UNDO, remainingUndo);
			PlayerPrefs.Save();
			UpdateHelpUI(); // <--- ĐỂ Ô SỐ UI CẬP NHẬT NGAY LẬP TỨC
		}
		else return;
	}

	public struct stepInfor
	{
		public Bottle A;
		public Bottle B;
		public int waterLayers;
	}

	stepInfor createStepInfor(Bottle a, Bottle B, int amountOfLayers)
	{
		stepInfor infor;
		infor.A = a;
		infor.B = B;
		infor.waterLayers = amountOfLayers;
		return infor;
	}

	// --- THÊM HÀM NÀY VÀO GAMEMANAGER.CS ---
	public void ExecuteHintPour(Bottle source, Bottle target)
	{
		// 1. Nếu các chai này đang bận chạy animation thì bỏ qua để tránh lỗi spam nút
		if (busyBottles.Contains(source) || busyBottles.Contains(target)) return;

		// 2. Nếu người chơi ĐANG CHỌN (nhấc) 1 chai nào đó trên tay, bắt buộc hạ nó xuống trước
		if (selectedBottle != null)
		{
			Vector3 dropPos = selectedBottle.transform.position - new Vector3(0f, liftOffset, 0f);
			StartCoroutine(AnimateBottle(selectedBottle.transform, dropPos, 0f, moveSpeed));
			selectedBottle = null;
		}

		// 3. Vị trí mặt đất của chai gợi ý (vì nó đang đứng yên dưới đất nên là position gốc)
		Vector3 groundPos = source.transform.position;

		// 4. Ra lệnh chạy Coroutine rót nước có sẵn của bạn!
		StartCoroutine(PourWaterRoutine(source, target, groundPos));
	}

	public bool CheckWin()
	{
		bool isWin = true;
		foreach (Bottle bottle in allBottles)
		{
			if (!bottle.isEmpty() && !bottle.isCompleted()){
				isWin = false;
				break;
			}
		}

		if (isWin)
		{
			StartCoroutine(WinSequenceRoutine());
			return true;
		}
		return false;
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

		// 5. LƯU GAME
		int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
		PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
		PlayerPrefs.Save();
		levelText.text = "Level " + levelManager.currentLevelData.levelId.ToString();

		// 6. PHÓNG TO POPUP CÚP VÀNG & NÚT BẤM (NEXT LEVEL POPUP)
		if (nextLevelPopupRect != null)
		{
			elapsed = 0f;
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

		// (Xong Coroutine! Game sẽ dừng ở đây để đợi người chơi bấm nút NEXT LEVEL)
	}

	public void onClickHome(){
		SceneManager.LoadScene("MainScene");
	}

	public void onClickNextLevel(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private void LoadHelpQuantities()
	{
		// Bí quyết ở đây: Số 2 ở cuối chính là giá trị mặc định nếu Key chưa tồn tại!
		remainingUndo = PlayerPrefs.GetInt(KEY_UNDO, 2);
		remainingHint = PlayerPrefs.GetInt(KEY_HINT, 2);
		remainingAddBottle = PlayerPrefs.GetInt(KEY_ADD_BOTTLE, 2);

		UpdateHelpUI();
	}

	// Hàm cập nhật số lượng lên các nút UI dưới màn hình
	private void UpdateHelpUI()
	{
		if (undoText != null) undoText.text = remainingUndo.ToString();
		if (hintText != null) hintText.text = remainingHint.ToString();
		if (addBottleText != null) addBottleText.text = remainingAddBottle.ToString();
	}

	// 1. Hàm gọi khi bấm nút QUAY LẠI 1 BƯỚC (Undo)
	public void OnClickUndoButton()
	{
		if (remainingUndo > 0)
		{
			remainingUndo--; // Trừ đi 1 lượt
			PlayerPrefs.SetInt(KEY_UNDO, remainingUndo); // Lưu lại vào máy
			PlayerPrefs.Save();
			UpdateHelpUI(); // Cập nhật lại số trên màn hình

			backStep();
		}
		else
		{
			Debug.Log("Đã hết lượt Undo!");
			if (PopupManager.instance != null) PopupManager.instance.ShowOutOfHelpPopup(HelpType.Undo);
			return;
		}
	}

	

	// 3. Hàm gọi khi bấm nút THÊM CHAI NƯỚC RỖNG
	public void OnClickAddBottleButton()
	{
		if (remainingAddBottle > 0)
		{
			remainingAddBottle--;
			PlayerPrefs.SetInt(KEY_ADD_BOTTLE, remainingAddBottle);
			PlayerPrefs.Save();
			UpdateHelpUI();

			// --- GỌI LOGIC SINH THÊM CHAI RỖNG CỦA BẠN Ở ĐÂY ---
		}
		else
		{
			Debug.Log("Đã hết lượt Thêm chai rỗng!");
		}
	}

	// Hàm này nối vào sự kiện OnClick của nút GỢI Ý (Hint) ngoài Unity
	public void UseHint()
	{
		// 1. KIỂM TRA QUYỀN LỰC: CÒN LƯỢT KHÔNG?
		if (remainingHint <= 0)
		{
			Debug.Log("Hết lượt dùng Gợi ý rồi!");
			if (PopupManager.instance != null) PopupManager.instance.ShowOutOfHelpPopup(HelpType.Hint);
			return;
		}

		// Nếu đang có chai nước rót dở, không cho dùng gợi ý để tránh lỗi animation
		if (busyBottles.Count > 0) return;

		// 2. GIAO VIỆC CHO THƯ KÝ: TÌM BƯỚC ĐI ĐI!
		if (gameHintManager != null)
		{
			// Chuyền allBottles sang cho thư ký tính
			PourStep? hintStep = gameHintManager.FindHint(allBottles);

			// 3. NẾU TÌM ĐƯỢC ĐƯỜNG
			if (hintStep.HasValue)
			{
				// Lấy ra 2 chai nước từ kết quả của thư ký
				Bottle fromBottle = allBottles[hintStep.Value.fromIndex];
				Bottle toBottle = allBottles[hintStep.Value.toIndex];

				Debug.Log($"Gợi ý: Tự động rót từ {fromBottle.name} sang {toBottle.name}");

				// THỰC HIỆN ANIMATION
				ExecuteHintPour(fromBottle, toBottle);

				// TRỪ LƯỢT VÀ UPDATE UI (Chỉ trừ lượt khi đã tìm ra và rót thành công)
				remainingHint--;
				PlayerPrefs.SetInt(KEY_HINT, remainingHint);
				PlayerPrefs.Save();
				UpdateHelpUI();
			}
			else
			{
				// Không trừ lượt nếu màn chơi đã bị khóa (deadlock)
				Debug.Log("Màn chơi bế tắc, không có bước đi gợi ý nào!");
			}
		}
	}

	// Hàm này nối vào sự kiện OnClick của nút THÊM BÌNH ngoài Unity
	public void UseAddBottle()
	{
		// 1. KIỂM TRA GIỚI HẠN CỦA MÀN CHƠI TRƯỚC
		if (extraBottlesUsedThisLevel >= maxExtraBottlesPerLevel)
		{
			Debug.Log("Màn này đã đạt giới hạn thêm chai rồi!");
			return;
		}

		// 2. KIỂM TRA TÚI ĐỒ NGƯỜI CHƠI
		if (remainingAddBottle <= 0)
		{
			Debug.Log("Hết lượt Thêm bình rồi!");
			if (PopupManager.instance != null) PopupManager.instance.ShowOutOfHelpPopup(HelpType.AddBottle);
			return;
		}

		// Đang có chai rót dở thì cấm thêm để tránh lỗi
		if (busyBottles.Count > 0) return;

		// 3. THỰC HIỆN LOGIC THÊM CHAI
		// Sinh chai mới ở tít trên cao (Y = 10)
		Vector3 spawnPos = new Vector3(0, 10f, 0);
		GameObject newBottleObj = Instantiate(emptyBottlePrefab, spawnPos, Quaternion.identity);
		Bottle newBottle = newBottleObj.GetComponent<Bottle>();

		// -- PHẦN CODE MỚI THÊM VÀO ĐÂY --
		if (newBottle != null)
		{
			// Bắt buộc phải gán sức chứa cho chai mới (Thường là bằng với sức chứa của màn hiện tại)
			// Nếu bạn có lưu capacity chung ở LevelManager thì lấy ra, hoặc fix cứng là 4
			newBottle.capacity = levelManager.currentLevelData != null ? levelManager.currentLevelData.bottleCapacity : 4;

			// RÚT CẠN NƯỚC!
			newBottle.MakeEmptyBottle();
		}

		// Thêm vào danh sách quản lý
		allBottles.Add(newBottle);

		// Tăng biến đếm màn chơi
		extraBottlesUsedThisLevel++;
		if (extraBottlesUsedThisLevel >= maxExtraBottlesPerLevel)
		{
			if (addBottleButton != null)
			{
				addBottleButton.interactable = false;
			}
		}

		// GỌI HÀM SẮP XẾP LẠI VÀ CHẠY ANIMATION TRƯỢT
		// Giả sử hàm này bạn viết trong GameManager hoặc LevelManager
		StartCoroutine(RearrangeBottlesRoutine());

		// 4. TRỪ LƯỢT VÀ UPDATE UI
		remainingAddBottle--;
		PlayerPrefs.SetInt(KEY_ADD_BOTTLE, remainingAddBottle);
		PlayerPrefs.Save();
		UpdateHelpUI();
	}

	// ---- BÊN TRONG FILE GameManager.cs ----
	private IEnumerator RearrangeBottlesRoutine()
	{
		// 1. LẤY TỌA ĐỘ MỚI TỪ LEVEL MANAGER
		// allBottles lúc này đã chứa cả cái chai rỗng mới được sinh ra rồi
		List<Vector3> targetPositions = levelManager.GetBottleTargetPositions(allBottles.Count);

		// 2. LƯU LẠI VỊ TRÍ XUẤT PHÁT CỦA CÁC CHAI
		List<Vector3> startPositions = new List<Vector3>();
		for (int i = 0; i < allBottles.Count; i++)
		{
			startPositions.Add(allBottles[i].transform.position);
		}

		// 3. VÒNG LẶP ANIMATION TRƯỢT MƯỢT MÀ
		float duration = 0.4f; // Trượt trong 0.4 giây cho dứt khoát
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;

			// Công thức Ease Out Cubic: Trượt nhanh lúc đầu, chậm dần về đích
			float easeT = 1f - Mathf.Pow(1f - t, 3f);

			for (int i = 0; i < allBottles.Count; i++)
			{
				// Đảm bảo không bị lỗi Out of Bounds nếu số lượng chai và tọa độ khớp nhau
				if (i < targetPositions.Count)
				{
					allBottles[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], easeT);
				}
			}

			yield return null;
		}

		// 4. CHỐT HẠ TỌA ĐỘ CHÍNH XÁC (Đề phòng sai số của frame cuối)
		for (int i = 0; i < allBottles.Count; i++)
		{
			if (i < targetPositions.Count)
			{
				allBottles[i].transform.position = targetPositions[i];
			}
		}
	}

	// Hàm hỗ trợ cộng lượt mua từ Popup sang
	public void		AddHelpQuantity(HelpType type, int amount)
	{
		switch (type)
		{
			case HelpType.Undo:
				remainingUndo += amount;
				PlayerPrefs.SetInt(KEY_UNDO, remainingUndo);
				break;

			case HelpType.Hint:
				remainingHint += amount;
				PlayerPrefs.SetInt(KEY_HINT, remainingHint);
				break;

			case HelpType.AddBottle:
				remainingAddBottle += amount;
				PlayerPrefs.SetInt(KEY_ADD_BOTTLE, remainingAddBottle);
				break;
		}

		PlayerPrefs.Save();
		UpdateHelpUI(); // Cập nhật lại số hiển thị trên UI ngay lập tức
	}
}