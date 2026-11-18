using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class UGSManager : MonoBehaviour
{
	public static UGSManager Instance;

	// 1. Awake CHỈ DÙNG để xử lý logic Bất tử (Chạy ngay lập tức)
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	// 2. Start DÙNG để bật mạng (Chạy chậm hơn 1 nhịp để chờ Unity nạp xong Package)
	private async void Start()
	{
		// Phải kiểm tra xem đây có phải bản gốc không thì mới kết nối
		if (Instance == this)
		{
			await InitializeUnityServices();
		}
	}

	private async Task InitializeUnityServices()
	{
		try
		{
			var options = new InitializationOptions();
			options.SetEnvironmentName("production");

			await UnityServices.InitializeAsync(options);
			Debug.Log("Khởi tạo Server UGS thành công (Môi trường: Production)!");

			await SignInAnonymously();
		}
		catch (System.Exception e)
		{
			Debug.LogError("Lỗi khởi tạo UGS: " + e.Message);
		}
	}

	private async Task SignInAnonymously()
	{
		try
		{
			if (!AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				Debug.Log("Đăng nhập ẩn danh thành công! Player ID của bạn là: " + AuthenticationService.Instance.PlayerId);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("Lỗi đăng nhập ẩn danh: " + e.Message);
		}
	}
}