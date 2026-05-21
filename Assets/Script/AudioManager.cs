using UnityEngine;

public class AudioManager : MonoBehaviour
{
	// Tạo Singleton để có thể gọi ở bất cứ đâu bằng cú pháp AudioManager.instance
	public static AudioManager instance;

	[Header("Trạm Phát Âm Thanh")]
	public AudioSource sfxSource; // Cái loa để phát hiệu ứng âm thanh (SFX)

	[Header("Kho Âm Thanh (Audio Clips)")]
	public AudioClip buttonClickClip;
	public AudioClip bottleLift;
	public AudioClip winGameClip;
	public AudioClip pourClip;
	public AudioClip PopupClip;
	public AudioClip bottleDown;

	private void Awake()
	{
		// Bí quyết chống tắt tiếng: Giữ cho AudioManager sống sót khi chuyển màn chơi
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject); // Không phá hủy object này khi LoadScene
		}
		else
		{
			Destroy(gameObject); // Tránh bị nhân đôi khi load lại scene
		}
	}

	// --- CÁC HÀM PHÁT ÂM THANH ---

	// 1. Tiếng nút bấm UI
	public void PlayButtonClick()
	{
		if (sfxSource != null && buttonClickClip != null)
		{
			sfxSource.PlayOneShot(buttonClickClip);
		}
	}

	// 2. Tiếng chạm vào chai nước
	public void PlayBottleLift()
	{
		if (sfxSource != null && bottleLift != null)
		{
			// PlayOneShot giúp các âm thanh đè lên nhau mà không bị ngắt quãng
			sfxSource.PlayOneShot(bottleLift);
		}
	}

	public void PlayBottleDown()
	{
		if (sfxSource != null && bottleDown != null)
		{
			// PlayOneShot giúp các âm thanh đè lên nhau mà không bị ngắt quãng
			sfxSource.PlayOneShot(bottleDown);
		}
	}

	// 3. Tiếng chiến thắng
	public void PlayWinSound()
	{
		if (sfxSource != null && winGameClip != null)
		{
			sfxSource.PlayOneShot(winGameClip);
		}
	}

	public void StartPourSound()
	{
		if (sfxSource != null && pourClip != null)
		{
			sfxSource.clip = pourClip;
			sfxSource.loop = true;
			sfxSource.Play();
		}
	}

	public void StopPourSound()
	{
		if (sfxSource != null)
		{
			sfxSource.Stop();
		}
	}
	public void PlayPopupSound()
	{
		if (sfxSource != null && PopupClip != null)
		{
			sfxSource.PlayOneShot(PopupClip);
		}
	}
}