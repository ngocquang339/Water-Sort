using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class ChestOpeningController : MonoBehaviour
{
	[Header("UI Elements")]
	public Image blackOverlay;
	public RectTransform chestRect;
	public Image chestImage;
	public RectTransform groundCrackRect;
	public Transform rewardSpawnPoint;

	[Header("Phase 2: Flying Targets")]
	public RectTransform coinTopBarIcon;     // Kéo icon Coin trên TopBar vào đây
	public RectTransform diamondTopBarIcon;  // Kéo icon Diamond trên TopBar vào đây
	public RectTransform otherIcon;

	[Header("Prefab")]
	public RewardItemUI rewardItemPrefab; // Kéo Prefab của bạn vào đây

	[Header("Settings")]
	public float fallDuration = 0.6f;

	[Header("Phase 3: Tap to Claim")]
	public TextMeshProUGUI tapToSkipText; // Kéo Text "Tap to skip" vào đây

	private bool canTapToClaim = false; // Cờ đánh dấu khi nào được phép bấm
	private bool isClaiming = false;    // Cờ chặn spam click

	// Danh sách lưu tạm các phần quà vừa đẻ ra để chuẩn bị cho bay
	private List<RewardItemUI> spawnedRewards = new List<RewardItemUI>();

	[SerializeField] private float chestLandingY = -50f;
	[SerializeField] private Sprite coin;
	[SerializeField] private Sprite diamond;

	

	public void PlayChestAnimation(Sprite closedSprite, Sprite openSprite, RewardItem[] rewardsData)
	{
		isClaiming = true;
		DailyRewardManager.Instance.blockInputPanel.SetActive(true);
		DailyRewardManager.Instance.darkBackground.SetActive(false);
		DailyRewardManager.Instance.dailyRewardPanel.SetActive(false);
		// 1. Xóa sạch quà cũ (nếu có lỗi kẹt lại)
		foreach (var r in spawnedRewards) { if (r != null) Destroy(r.gameObject); }
		spawnedRewards.Clear();

		// 2. Sinh ra các Prefab dựa trên Data truyền vào
		foreach (var data in rewardsData)
		{
			RewardItemUI newReward = Instantiate(rewardItemPrefab, rewardSpawnPoint);
			newReward.SetupReward(data.image, data.icon, data.amount);

			// 👇 Gắn tên "Coin" hoặc "Diamond" cho nó dựa vào Data của bạn
			newReward.gameObject.name = data.rewardType;

			newReward.gameObject.SetActive(false);
			newReward.transform.localScale = Vector3.zero;

			spawnedRewards.Add(newReward);
		}

		// 3. Chạy chuỗi Animation Rương
		ResetState(closedSprite);
		AudioManager.instance.PlayChestDrop();
		Sequence seq = DOTween.Sequence();
		seq.Append(blackOverlay.DOFade(0.99f, 0.3f));
		seq.Append(chestRect.DOAnchorPosY(chestLandingY, fallDuration).SetEase(Ease.OutBounce));
		float firstImpactTime = 0.3f + (fallDuration * 0.36f);

		// Chèn vết nứt xuất hiện đúng vào thời điểm firstImpactTime
		seq.Insert(firstImpactTime, groundCrackRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
		seq.Insert(0.3f + fallDuration - 0.1f, groundCrackRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
		seq.AppendInterval(0.5f);	

		seq.Append(chestRect.DOPunchRotation(new Vector3(0, 0, 15f), 0.3f, 10, 1f));
		seq.InsertCallback(seq.Duration() - 0.15f, () =>
		{
			AudioManager.instance.PlayChestOpen();
		});
		seq.AppendCallback(() =>
		{
			chestImage.sprite = openSprite;
			chestRect.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.2f);
		});

		seq.AppendInterval(0.1f);
		seq.AppendCallback(() =>
		{
			StartCoroutine(PopOutRewardsRoutine());
		});
	}

	private void ResetState(Sprite closedSprite)
	{
		blackOverlay.gameObject.SetActive(true);
		blackOverlay.color = new Color(0, 0, 0, 0);

		chestRect.gameObject.SetActive(true);
		chestImage.sprite = closedSprite;
		chestRect.anchoredPosition = new Vector2(0, 1500f);
		chestRect.localScale = Vector3.one;
		chestRect.localRotation = Quaternion.identity;

		groundCrackRect.gameObject.SetActive(true);
		groundCrackRect.localScale = Vector3.zero;

		// Thêm đoạn này vào cuối hàm ResetState:
		canTapToClaim = false;
		isClaiming = false;

		if (tapToSkipText != null)
		{
			tapToSkipText.DOKill(); // Tắt các animation nhấp nháy cũ (nếu có)
			tapToSkipText.gameObject.SetActive(false);
			tapToSkipText.color = new Color(tapToSkipText.color.r, tapToSkipText.color.g, tapToSkipText.color.b, 0f); // Mặc định tàng hình
		}
	}

	private IEnumerator PopOutRewardsRoutine()
	{
		int itemCount = spawnedRewards.Count;
		int maxItemsPerRow = 4;   
		float spacingX = 330f;    
		float spacingY = 200f;    
		float startTargetY = 700f; 
		float targetScale = 2f; 

		for (int i = 0; i < itemCount; i++)
		{
			var reward = spawnedRewards[i];
			reward.gameObject.SetActive(true);
			RectTransform rect = reward.GetComponent<RectTransform>();

			// 1. Tính xem món quà thứ [i] đang nằm ở HÀNG nào và CỘT nào
			int rowIndex = i / maxItemsPerRow;
			int colIndex = i % maxItemsPerRow;

			// 2. Tính xem hàng hiện tại có chính xác bao nhiêu món
			// (Hàng cuối cùng có thể không đủ maxItemsPerRow, ta phải lấy số lượng thực tế để căn giữa cho chuẩn)
			int itemsInThisRow = Mathf.Min(maxItemsPerRow, itemCount - (rowIndex * maxItemsPerRow));

			// 3. Tính toán vị trí X (Căn giữa theo hàng)
			float rowTotalWidth = (itemsInThisRow - 1) * spacingX;
			float startX = -rowTotalWidth / 2f;
			float targetX = startX + (colIndex * spacingX);

			// 4. Tính toán vị trí Y (Hàng số 0 cao nhất, các hàng sau tụt dần xuống)
			float targetY = startTargetY - (rowIndex * spacingY);

			Vector2 targetPos = new Vector2(targetX, targetY);
			AudioManager.instance.PlayItemPop();
			// Hiệu ứng bay bổng
			rect.DOScale(targetScale, 0.4f).SetEase(Ease.OutBack);
			rect.DOJumpAnchorPos(targetPos, jumpPower: 150f, numJumps: 1, duration: 0.5f);

			// [TỰ ĐỘNG DỌN RÁC] Hủy object sau 2.5 giây
			//Destroy(reward.gameObject, 2.5f);

			// Tạm dừng 0.1s để quà bay ra lác đác nhìn đã mắt hơn
			yield return new WaitForSeconds(0.1f);
		}

		// 👇 ĐÃ SỬA: Đợi búng quà xong hết thì hiện chữ nhấp nháy
		if (tapToSkipText != null)
		{
			tapToSkipText.gameObject.SetActive(true);
			// DOTween: Fade alpha lên 1 trong 0.8s, lặp lại vô hạn (-1), kiểu lặp Yoyo (lên 1 rồi lùi về 0)
			tapToSkipText.DOFade(1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
		}

		// Bật cờ cho phép người chơi bấm vào màn hình
		canTapToClaim = true;
		DailyRewardManager.Instance.blockInputPanel.SetActive(false);
	}

	private IEnumerator FlyTrailAnimation(RectTransform sourceItem, RectTransform targetUI, Sprite iconSprite)
	{
		int trailCount = 8; // Số lượng icon nhỏ bay ra thành 1 dải
		float scatterRadius = 80f; // Độ văng rộng ra xung quanh trước khi bay

		Vector3 startWorldPos = sourceItem.position;
		
			Vector3 targetWorldPos = targetUI.position;

		// 👇 ĐÃ SỬA: Tìm đến TẬN CÙNG Canvas gốc dựa vào TopBar (Đảm bảo 100% an toàn)
		Canvas mainCanvas = targetUI.GetComponentInParent<Canvas>().rootCanvas;

		// Tắt cái cục to đi để nhường sân khấu cho các cục nhỏ
		sourceItem.gameObject.SetActive(false);

		for (int i = 0; i < trailCount; i++)
		{
			GameObject flyingObj = new GameObject("FlyingParticle_" + i);
			flyingObj.transform.SetParent(mainCanvas.transform);

			// 👇 ĐÃ SỬA: Đẩy nó xuống dưới cùng Hierarchy để nó đè lên mọi vật thể khác (kể cả DarkBackground)
			flyingObj.transform.SetAsLastSibling();

			// Gán vị trí xuất phát ngay tại cục to cũ
			flyingObj.transform.position = startWorldPos;
			flyingObj.transform.localScale = Vector3.one;

			Image img = flyingObj.AddComponent<Image>();
			img.sprite = iconSprite;
			img.rectTransform.sizeDelta = new Vector2(60, 60); // Kích thước hạt

			// 2. Gắn hiệu ứng DOTween
			Sequence seq = DOTween.Sequence();

			// Bước A: Bay lộn xộn ra xung quanh một chút (Tạo cảm giác nổ tung)
			Vector3 randomOffset = new Vector3(Random.Range(-scatterRadius, scatterRadius), Random.Range(-scatterRadius, scatterRadius), 0);
			seq.Append(flyingObj.transform.DOMove(startWorldPos + randomOffset, 0.2f).SetEase(Ease.OutQuad));

			// Bước B: Nghỉ 1 nhịp siêu ngắn, lác đác
			seq.AppendInterval(Random.Range(0f, 0.1f));

			// Bước C: Bay vút về đích trên Top Bar
			
				seq.Append(flyingObj.transform.DOMove(targetWorldPos, 0.5f).SetEase(Ease.InBack));
			// 👇 ĐÃ SỬA: Bắt đầu phát âm thanh SỚM HƠN 0.15 giây trước khi nó chạm đích
			// Hàm seq.Duration() sẽ lấy tổng thời gian của toàn bộ sequence tính đến hiện tại
			seq.InsertCallback(seq.Duration() - 0.25f, () =>
			{
				AudioManager.instance.PlayItemCollect();
			});

			// Bước D: Đến nơi thì biến mất (Và có thể phát tiếng Ting Ting ở đây)
			seq.AppendCallback(() =>
			{
				Destroy(flyingObj);

				// HIỆU ỨNG THÊM: Làm cái icon đích nảy lên một cái khi bị đập vào
				targetUI.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.1f);
			});

			// Delay khoảng 0.05s trước khi đẻ hạt tiếp theo để chúng bay thành dải nối đuôi nhau
			yield return new WaitForSeconds(0.05f);
		}

		// Cuối cùng dọn sạch cái cục to
		Destroy(sourceItem.gameObject);
	}


	// GẮN HÀM NÀY VÀO SỰ KIỆN ONCLICK CỦA BUTTON TRÊN BLACK PANEL
	public void OnBlackPanelClicked()
	{
		// Chặn click nếu quà chưa bung xong, hoặc đang trong lúc bay rồi (tránh spam)
		if (!canTapToClaim || isClaiming) return;

		isClaiming = true;

		// Tắt ngay chữ Tap to skip
		if (tapToSkipText != null)
		{
			tapToSkipText.DOKill();
			tapToSkipText.gameObject.SetActive(false);
		}

		// Bắt đầu chuỗi Animation dọn dẹp
		StartCoroutine(ClaimAndCloseRoutine());
	}

	private IEnumerator ClaimAndCloseRoutine()
	{
		// 1. ANIMATION TẮT RƯƠNG & NỀN ĐEN
		// Rương thu nhỏ lại cực mượt (InBack tạo cảm giác hút vào trong)
		chestRect.DOScale(0f, 0.3f).SetEase(Ease.InBack);
		groundCrackRect.DOScale(0f, 0.3f).SetEase(Ease.InBack);

		// Mờ dần nền đen
		blackOverlay.DOFade(0f, 0.5f);

		// 2. KÍCH HOẠT ANIMATION QUÀ BAY LÊN TOP BAR
		foreach (var reward in spawnedRewards)
		{
			if (reward == null) continue;

			RectTransform targetUI = null;
			if (reward.gameObject.name == "Coin")
			{
				targetUI = coinTopBarIcon;
				Sprite actualIcon = reward.iconImage;
				StartCoroutine(FlyTrailAnimation(reward.GetComponent<RectTransform>(), targetUI, actualIcon));
			}
			else if (reward.gameObject.name == "Diamond")
			{
				Sprite actualIcon = reward.iconImage;
				targetUI = diamondTopBarIcon;
				StartCoroutine(FlyTrailAnimation(reward.GetComponent<RectTransform>(), targetUI, actualIcon));
			}
			else{
				reward.gameObject.SetActive(false);
			}
		}

		// 3. Chờ các hạt dải ngân hà bay xong xuôi (Khoảng 1.5 giây)
		yield return new WaitForSeconds(1.5f);

		// 4. Dọn dẹp tàn dư, trả lại màn hình sạch sẽ
		chestRect.gameObject.SetActive(false);
		groundCrackRect.gameObject.SetActive(false);
		blackOverlay.gameObject.SetActive(false);

		spawnedRewards.Clear();
		canTapToClaim = false;
		isClaiming = false;

		// (Tùy chọn) Lúc này bạn có thể gọi sang DailyRewardManager báo là luồng UI đã kết thúc hoàn toàn.
	}
}