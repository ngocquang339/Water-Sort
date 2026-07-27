using System.Threading.Tasks;
using Unity.Services.Authentication; // Để cập nhật tên
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
	public static LeaderboardManager Instance;

	// QUAN TRỌNG: Phải khớp 100% với cái ID bạn đã đặt trên trang Web
	private const string LEADERBOARD_ID = "top_level";

	[Header("Giao diện Bảng xếp hạng")]
	public GameObject rankSlotPrefab; // Kéo Prefab RankSlot vào đây
	public Transform contentTransform; // Kéo object Content của Scroll View vào đây

	private void Awake()
	{
		// Khi load scene mới, Instance sẽ nhận giá trị của LeaderboardManager ở scene mới này
		Instance = this;
	}

	// THÊM HÀM NÀY VÀO: Khi Scene bị đóng và Object bị hủy, phải xóa trí nhớ của Instance!
	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
	// ==========================================
	// 1. CẬP NHẬT TÊN (Gọi khi người chơi nhập tên ở SetName_Scene)
	// ==========================================
	public async Task SubmitPlayerName(string playerName)
	{
		try
		{
			// Bắn cái tên lên Cloud để gắn vào ID
			await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
			Debug.Log("Đã cập nhật tên trên Cloud thành: " + playerName);
		}
		catch (System.Exception e)
		{
			Debug.LogError("Lỗi lưu tên: " + e.Message);
		}
	}

	// ==========================================
	// 2. GỬI ĐIỂM (Gọi mỗi khi chơi thắng / qua màn)
	// ==========================================
	// Thay async void bằng async Task
	public async Task AddScore(int level)
	{
		try
		{
			var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(LEADERBOARD_ID, level);
			Debug.Log($"Đã gửi điểm thành công! Bạn đang ở Level: {scoreResponse.Score}");
		}
		catch (System.Exception e)
		{
			Debug.LogError("Lỗi gửi điểm: " + e.Message);
		}
	}

	// ==========================================
	// HÀM DEBUG: KIỂM TRA TOÀN DIỆN LUỒNG MẠNG
	// ==========================================
	public void DebugNetworkFlow()
	{
		Debug.Log("========== BẮT ĐẦU NỘI SOI LUỒNG UGS ==========");

		// 1. Kiểm tra trạng thái gốc của hệ thống mạng
		Debug.Log($"[Trạm 1] UnityServices.State hiện tại đang là: {UnityServices.State}");

		// 2. Kiểm tra xem Auth có bị mất kết nối giữa chừng không
		try
		{
			Debug.Log($"[Trạm 2] Tình trạng Auth IsSignedIn: {AuthenticationService.Instance.IsSignedIn}");
			if (AuthenticationService.Instance.IsSignedIn)
			{
				Debug.Log($"[Trạm 2] ID đang cầm: {AuthenticationService.Instance.PlayerId}");
			}
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"[Trạm 2] Auth ném ra lỗi: {e.Message}");
		}

		// 3. Ép thằng Leaderboard hiện nguyên hình trước khi gọi hàm kéo điểm
		try
		{
			var testInstance = LeaderboardsService.Instance;
			Debug.Log("[Trạm 3] Đã chạm được vào LeaderboardsService.Instance mà KHÔNG bị lỗi!");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[Trạm 3] Leaderboard tàng hình/chưa khởi tạo! Lỗi y hệt: {e.Message}");
		}

		Debug.Log("=================================================");
	}

	public async void FetchLeaderboard()
	{
		Debug.Log("Đang tải bảng xếp hạng...");
		if (UnityServices.State != ServicesInitializationState.Initialized) return;

		try
		{
			var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID, new GetScoresOptions { Limit = 10 });

			// 1. Dọn dẹp bảng cũ trước khi tải bảng mới (để tránh bị nhân bản khi bấm nút nhiều lần)
			if (contentTransform != null)
			{
				foreach (Transform child in contentTransform)
				{
					Destroy(child.gameObject);
				}
			}
			else
			{
				Debug.LogWarning("Không có contentTransform, bỏ qua việc tạo UI Leaderboard.");
				return;
			}

			// 2. Duyệt qua từng người chơi và tạo khung
			foreach (var entry in scoresResponse.Results)
			{
				// Sinh ra Prefab mới và nhét vào trong Content
				GameObject newSlot = Instantiate(rankSlotPrefab, contentTransform);

				// THÊM DÒNG NÀY VÀO: Ép thẻ Rank về đúng tỉ lệ gốc 1:1
				newSlot.transform.localScale = Vector3.one;
				// Lấy script RankSlotUI ra và nhồi data vào
				RankSlotUI slotScript = newSlot.GetComponent<RankSlotUI>();
				if (slotScript != null)
				{
					// entry.Rank của Unity bắt đầu từ 0 (giống array), nên ta cộng 1 để hiển thị cho chuẩn
					int actualRank = entry.Rank + 1;

					// Tên đôi khi có mã #1234 đằng sau, bạn có thể tách bằng lệnh Split('#')[0] nếu muốn tên sạch đẹp
					string cleanName = entry.PlayerName.Split('#')[0];

					slotScript.SetupSlot(actualRank, cleanName, (int)entry.Score);
				}
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("Lỗi tải bảng xếp hạng: " + e.Message);
		}
	}
}