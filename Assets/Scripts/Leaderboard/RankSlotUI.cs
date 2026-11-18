using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework.Internal;

public class RankSlotUI : MonoBehaviour
{
	[Header("Các thành phần UI")]
	public Image frameImage;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI levelNumber_Text;
	public TextMeshProUGUI rankText;

	[Header("Kho chứa Khung ảnh")]
	public Sprite top1Frame;
	public Sprite top2Frame;
	public Sprite top3Frame;
	public Sprite normalFrame;

	[Header ("Ranking_Icon")]
	public Sprite bottom1Frame;
	public Sprite bottom2Frame;
	public Sprite bottom3Frame;

	[Header("Avatar")]
	public Image avatarImage;

	// Hàm này sẽ được LeaderboardManager gọi để nhét data vào
	public void SetupSlot(int rank, string playerName, int level)
	{
		nameText.text = playerName;
		if (levelNumber_Text != null) 
		{
			levelNumber_Text.text = level.ToString();
		}

		Image rankIconImage = frameImage.transform.Find("Rank_Icon")?.GetComponent<Image>();

		// 2. Tráo ảnh khung (Rank bắt đầu từ 1)
		if (rank == 1)
		{
			Debug.Log("Bạn đang ở rank 1, set up frame top 1");
			frameImage.sprite = top1Frame;
			if (rankIconImage != null) {
				rankIconImage.gameObject.SetActive(true);
				rankIconImage.sprite = bottom1Frame;
			}
		}
		else if (rank == 2)
		{
			Debug.Log("Bạn đang ở rank 2, set up frame top 2");
			frameImage.sprite = top2Frame;
			if (rankIconImage != null) {
				rankIconImage.gameObject.SetActive(true);
				rankIconImage.sprite = bottom2Frame;
			}
		}
		else if (rank == 3)
		{
			Debug.Log("Bạn đang ở rank 3, set up frame top 3");
			frameImage.sprite = top3Frame;
			if (rankIconImage != null) {
				rankIconImage.gameObject.SetActive(true);
				rankIconImage.sprite = bottom3Frame;
			}
		}
		else
		{
			Debug.Log("Bạn đang ở rank bình thường, set up frame normal");
			frameImage.sprite = normalFrame;
			rankText.text = rank.ToString();
			rankText.gameObject.SetActive(true);
			if (rankIconImage != null) {
				rankIconImage.gameObject.SetActive(false);
			}
		}
	}
}