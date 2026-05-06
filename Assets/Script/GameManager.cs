using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private Bottle selectedBottle;

	// DANH SÁCH CÁC CHAI ĐANG BẬN BAY HOẶC RÓT NƯỚC
	private List<Bottle> busyBottles = new List<Bottle>();

	[Header("Cài đặt Game")]
	[SerializeField] private float liftOffset = 0.5f;

	[Header("Hiệu ứng Nước chảy")]
	public LineRenderer waterStream;

	[Header("Cài đặt Animation")]
	[SerializeField] private float moveSpeed = 0.3f;
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
					if (clickBottle.getTopColor() == WaterColor.None) return;

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

	// HÀM RÓT NƯỚC (Đã gộp chung cả Danh sách bận + Tia nước)
	private IEnumerator PourWaterRoutine(Bottle source, Bottle target, Vector3 groundPos)
	{
		if (target.isFull() || source.isEmpty()) yield break;

		// ĐƯA VÀO DANH SÁCH BẬN
		busyBottles.Add(source);
		busyBottles.Add(target);

		float direction = Mathf.Sign(target.transform.position.x - source.transform.position.x);
		float targetAngle = direction > 0 ? -pourAngle : pourAngle;
		Vector3 pourPosition = target.transform.position + new Vector3(-direction * pourOffsetX, pourOffsetY, 0f);

		// BAY TỚI VÀ NGHIÊNG CHAI
		yield return StartCoroutine(AnimateBottle(source.transform, pourPosition, targetAngle, moveSpeed));

		// ===============================================
		// HIỆU ỨNG TIA NƯỚC
		// ===============================================
		WaterColor colorToPour = source.getTopColor();

		waterStream.gameObject.SetActive(true);
		waterStream.startColor = Color.white; // Chỗ này sau nhớ đổi thành màu thực tế nhé
		waterStream.endColor = Color.white;

		// Điểm đầu và điểm cuối tia nước
		waterStream.SetPosition(0, source.mouthPoint.position);
		waterStream.SetPosition(1, target.mouthPoint.position);

		// RÓT NƯỚC LOGIC
		if (target.addNewColor(colorToPour))
		{
			source.removeTopColor();
			source.updateBottleVisuals();
			target.updateBottleVisuals();

			yield return new WaitForSeconds(0.4f); // Chờ tia nước chảy
		}

		waterStream.gameObject.SetActive(false); // Tắt tia nước
												 // ===============================================

		// BAY VỀ VỊ TRÍ MẶT ĐẤT
		yield return StartCoroutine(AnimateBottle(source.transform, groundPos, 0f, moveSpeed));

		// XÓA KHỎI DANH SÁCH BẬN
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
}