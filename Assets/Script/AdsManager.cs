using UnityEngine;
using UnityEngine.Advertisements;

// Bắt buộc phải kế thừa các Interface này để nhận phản hồi từ Unity Ads
public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
	public static AdsManager instance;
	[SerializeField] string _androidGameId = "6126835"; // Game ID Android của bạn
	[SerializeField] string _iOSGameId = "6126834";     // Game ID iOS của bạn
	[SerializeField] bool _testMode = true; // Đang code thì để True, khi nào build thật thì đổi thành False

	private string _gameId;
	private string _adUnitId = "Rewarded_Android"; // ID mặc định của quảng cáo nhận thưởng
	private CurrencyManager currencyManager;
	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
			return;
		}
		currencyManager = CurrencyManager.Instance;
		InitializeAds();
	}

	// 1. Khởi tạo SDK Quảng cáo
	public void InitializeAds()
	{
		_gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? _iOSGameId : _androidGameId;
		Advertisement.Initialize(_gameId, _testMode, this);
	}

	public void OnInitializationComplete()
	{
		Debug.Log("Khởi tạo Unity Ads thành công!");
		LoadAd(); // Khởi tạo xong thì tự động tải quảng cáo luôn
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	{
		Debug.Log($"Lỗi khởi tạo: {error.ToString()} - {message}");
	}

	// 2. Tải Quảng cáo
	public void LoadAd()
	{
		Debug.Log("Đang tải quảng cáo...");
		Advertisement.Load(_adUnitId, this);
	}

	// 3. HÀM NÀY SẼ GẮN VÀO NÚT WATCH ADS
	public void ShowAd()
	{
		Debug.Log("Hiển thị quảng cáo...");
		Advertisement.Show(_adUnitId, this);
	}

	// --- Các hàm Interface bắt buộc cho Load (Không cần viết code vào trong cũng được) ---
	public void OnUnityAdsAdLoaded(string adUnitId) { }
	public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) {
		Debug.Log($"Lỗi tải quảng cáo ({error}): {message}. Đang tự động thử lại sau 3 giây...");

		// Gọi lại hàm LoadAd sau 3 giây để khắc phục rớt mạng tạm thời
		Invoke(nameof(LoadAd), 3f);
	}

	// --- Các hàm Interface bắt buộc cho Show ---
	public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
	{
		// Kiểm tra xem người chơi có xem hết video không
		if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
		{
			Debug.Log("Người chơi đã xem hết quảng cáo! Trả thưởng thành công.");

			currencyManager.AddCoin(20);

			LoadAd(); // Tải video mới để sẵn sàng cho lần bấm tiếp theo
		}
	}

	public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) { }
	public void OnUnityAdsShowStart(string adUnitId) { }
	public void OnUnityAdsShowClick(string adUnitId) { }
}