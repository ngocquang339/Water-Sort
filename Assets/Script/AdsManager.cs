using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using TMPro;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
	public static AdsManager instance;
	[SerializeField] string _androidGameId = "6126835";
	[SerializeField] string _iOSGameId = "6126834";
	[SerializeField] bool _testMode = true;

	[Header("UI Cảnh Báo")]

	public GameObject notAvailable_Panel;

	public TextMeshProUGUI notAvailableAds;
	private string _gameId;
	private string _adUnitId = "Rewarded_Android";
	private CurrencyManager currencyManager;

	// THÊM BIẾN NÀY ĐỂ TỰ THEO DÕI TRẠNG THÁI QUẢNG CÁO
	private bool _isAdReady = false;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		currencyManager = CurrencyManager.Instance;
		InitializeAds();
	}

	public void InitializeAds()
	{
		_gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? _iOSGameId : _androidGameId;
		Advertisement.Initialize(_gameId, _testMode, this);
	}

	public void OnInitializationComplete()
	{
		Debug.Log("Khởi tạo Unity Ads thành công!");
		LoadAd();
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	{
		Debug.Log($"Lỗi khởi tạo: {error.ToString()} - {message}");
	}

	public void LoadAd()
	{
		Debug.Log("Đang tải quảng cáo...");
		Advertisement.Load(_adUnitId, this);
	}

	// ==========================================================
	// HÀM BẮT SỰ KIỆN: KHI QUẢNG CÁO TẢI XONG THÌ BẬT BIẾN LÊN TRUE
	// ==========================================================
	public void OnUnityAdsAdLoaded(string adUnitId)
	{
		if (adUnitId.Equals(_adUnitId))
		{
			Debug.Log("Video đã tải xong vào bộ nhớ tạm!");
			_isAdReady = true; // Đánh dấu là đã sẵn sàng
		}
	}

	public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
	{
		Debug.Log($"Lỗi tải quảng cáo ({error}): {message}. Đang tự động thử lại sau 3 giây...");
		_isAdReady = false; // Tải lỗi thì chắc chắn là chưa sẵn sàng
		Invoke(nameof(LoadAd), 3f);
	}

	// ==========================================================
	// HÀM SHOW QUẢNG CÁO SỬ DỤNG BIẾN TỰ VIẾT
	// ==========================================================
	public void ShowAd()
	{
		if (_isAdReady)
		{
			_isAdReady = false;
			Advertisement.Show(_adUnitId, this);
		}
		else
		{
			// DÙNG 1 DÒNG CODE DUY NHẤT:
			ToastManager.instance.ShowToast(notAvailable_Panel, notAvailableAds.text);
		}
	}

	//private IEnumerator ShowToastMessage()
	//{
	//	if (notAvailablePopup != null)
	//	{
	//		notAvailablePopup.SetActive(true);
	//		yield return new WaitForSeconds(2f);
	//		notAvailablePopup.SetActive(false);
	//	}
	//}

	public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
	{
		if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
		{
			Debug.Log("Người chơi đã xem hết quảng cáo! Trả thưởng thành công.");

			if (currencyManager != null)
			{
				currencyManager.AddCoin(20);
			}

			LoadAd(); // Tải video mới
		}
	}

	public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
	{
		// Xem lỗi thì cũng phải load lại video mới
		LoadAd();
	}

	public void OnUnityAdsShowStart(string adUnitId) { }
	public void OnUnityAdsShowClick(string adUnitId) { }
}