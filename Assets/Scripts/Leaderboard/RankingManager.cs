using UnityEngine;

public class RankingManager : MonoBehaviour
{
	[Header("UI Reference")]
	// Chỉ cần kéo script PanelSlider của ShopPanel vào đây
	public PanelSlider rankPanelSlider;
	
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public void ClickOpenRanking()
	{
		// Gọi động cơ trượt mở ra
		rankPanelSlider.OpenPanel();
		if (LeaderboardManager.Instance != null)
		{
			LeaderboardManager.Instance.FetchLeaderboard();
		}
		else{
			Debug.LogError("Không tìm thấy LeaderboardManager trong cảnh! Hãy chắc chắn rằng bạn đã thêm LeaderboardManager vào cảnh.");
		}
	}

	public void ClickCloseRanking()
	{
		rankPanelSlider.ClosePanel();
		
	}

	
}
