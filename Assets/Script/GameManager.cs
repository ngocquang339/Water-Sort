using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private Bottle selectedBottle;
	private Stack<stepInfor> saveStepInfor = new Stack<stepInfor>();

	private List<Bottle> busyBottles = new List<Bottle>();

	[Header("Cài đặt Game")]
	[SerializeField] private float liftOffset = 0.5f;

	[Header("Hiệu ứng Nước chảy")]
	public LineRenderer waterStream;
	public ParticleSystem waterSplashPrefab;

	[Header("Cài đặt Animation")]
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private float pourAngle = 90f;
	[SerializeField] private float pourOffsetX = 0.8f;
	[SerializeField] private float pourOffsetY = 1.0f;

	void Update()
	{
		liftBottle();
	}

	private void liftBottle()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Bottle clickBottle = getBottleFromClick();
			if (clickBottle != null)
			{
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

	public void backStep()
	{
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
		}
		else return;
	}

	public void hintStep(){
		
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
}