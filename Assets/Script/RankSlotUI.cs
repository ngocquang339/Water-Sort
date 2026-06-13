using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankSlotUI : MonoBehaviour
{
	[Header("Các thành phần UI")]
	public Image frameImage;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI levelText;
	public TextMeshProUGUI levelNumber_Text;

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
		levelText.text = "Lv. " + level.ToString();

		// 2. Tráo ảnh khung (Rank bắt đầu từ 1)
		if (rank == 1)
		{
			frameImage.sprite = top1Frame;
		}
		else if (rank == 2)
		{
			frameImage.sprite = top2Frame;
		}
		else if (rank == 3)
		{
			frameImage.sprite = top3Frame;
		}
		else
		{
			frameImage.sprite = normalFrame;
		}
	}
}