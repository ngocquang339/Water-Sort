using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class ChestOpeningController : MonoBehaviour
{
	[Header("UI Elements")]
	public Image blackOverlay;
	public RectTransform chestRect;
	public Image chestImage;
	public RectTransform groundCrackRect;
	public Transform rewardSpawnPoint;

	[Header("Prefab")]
	public RewardItemUI rewardItemPrefab; // Kéo Prefab của bạn vào đây

	[Header("Settings")]
	public float fallDuration = 0.6f;

	// Danh sách lưu tạm các phần quà vừa đẻ ra để chuẩn bị cho bay
	private List<RewardItemUI> spawnedRewards = new List<RewardItemUI>();

	[SerializeField] private float chestLandingY = -50f;

	public void PlayChestAnimation(Sprite closedSprite, Sprite openSprite, RewardItem[] rewardsData)
	{
		DailyRewardManager.Instance.darkBackground.SetActive(false);
		DailyRewardManager.Instance.dailyRewardPanel.SetActive(false);
		// 1. Xóa sạch quà cũ (nếu có lỗi kẹt lại)
		foreach (var r in spawnedRewards) { if (r != null) Destroy(r.gameObject); }
		spawnedRewards.Clear();

		// 2. Sinh ra các Prefab dựa trên Data truyền vào
		foreach (var data in rewardsData)
		{
			Debug.Log("Số lượng quà truyền vào là: " + rewardsData.Length);
			RewardItemUI newReward = Instantiate(rewardItemPrefab, rewardSpawnPoint);
			// Gọi hàm đắp data vào Prefab (nhớ sửa lại biến .image, .amount cho đúng class của bạn)
			newReward.SetupReward(data.image, data.amount);

			// Tạm thời tàng hình nó đi, giấu vào trong rương
			newReward.gameObject.SetActive(false);
			newReward.transform.localScale = Vector3.zero;

			spawnedRewards.Add(newReward);
		}

		// 3. Chạy chuỗi Animation Rương
		ResetState(closedSprite);

		Sequence seq = DOTween.Sequence();
		seq.Append(blackOverlay.DOFade(0.99f, 0.3f));
		seq.Append(chestRect.DOAnchorPosY(chestLandingY, fallDuration).SetEase(Ease.OutBounce));
		float firstImpactTime = 0.3f + (fallDuration * 0.36f);

		// Chèn vết nứt xuất hiện đúng vào thời điểm firstImpactTime
		seq.Insert(firstImpactTime, groundCrackRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
		seq.Insert(0.3f + fallDuration - 0.1f, groundCrackRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
		seq.AppendInterval(0.5f);	

		seq.Append(chestRect.DOPunchRotation(new Vector3(0, 0, 15f), 0.3f, 10, 1f));
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
	}

	private IEnumerator PopOutRewardsRoutine()
	{
		foreach (var reward in spawnedRewards)
		{
			reward.gameObject.SetActive(true);
			RectTransform rect = reward.GetComponent<RectTransform>();

			// Tính toán vị trí ngẫu nhiên văng ra
			float randomX = Random.Range(-300f, 300f);
			float randomY = Random.Range(100f, 400f);
			Vector2 targetPos = new Vector2(randomX, randomY);

			// Hiệu ứng bay bổng
			rect.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
			rect.DOJumpAnchorPos(targetPos, jumpPower: 150f, numJumps: 1, duration: 0.5f);

			// [TỰ ĐỘNG DỌN RÁC] Hủy object sau 2.5 giây kể từ lúc bay ra
			Destroy(reward.gameObject, 2.5f);

			yield return new WaitForSeconds(0.1f);
		}

		// Xóa sạch danh sách sau khi đã xử lý xong
		spawnedRewards.Clear();
	}
}