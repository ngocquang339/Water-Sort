using UnityEngine;

public class AudioManager : MonoBehaviour
{
	// Tạo Singleton để có thể gọi ở bất cứ đâu bằng cú pháp AudioManager.instance
	public static AudioManager instance;

	[Header("Kho Âm Thanh (Audio Clips)")]
	public AudioClip buttonClickClip;
	public AudioClip bottleLift;
	public AudioClip winGameClip;
	public AudioClip pourClip;
	public AudioClip PopupClip;
	public AudioClip bottleDown;
	public AudioClip gameOver;
	public AudioClip doneBottle;

	[Header("Nguồn âm thanh")]
	public AudioSource musicSource;
	public AudioSource sfxSource;
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

	public void PlayGameOver(){
		if(sfxSource != null && gameOver != null){
			sfxSource.PlayOneShot(gameOver);
		}
	}

	public void PlayDoneBottle(){
		if(sfxSource != null && doneBottle != null){
			sfxSource.PlayOneShot(doneBottle);
		}
	}

	// Hàm chỉnh volume cho Nhạc nền (Nhận giá trị từ 0 đến 1)
	public void SetMusicVolume(float volume)
	{
		if (musicSource != null)
		{
			musicSource.volume = volume;
		}
	}

	// Hàm chỉnh volume cho Tiếng động hiệu ứng (Nhận giá trị từ 0 đến 1)
	public void SetSFXVolume(float volume)
	{
		if (sfxSource != null)
		{
			sfxSource.volume = volume;
		}
		// Lưu ý: Nếu bạn dùng lệnh AudioSource.PlayOneShot(clip) thông qua sfxSource này,
		// thì khi thay đổi sfxSource.volume, tất cả âm thanh phát ra đều sẽ to nhỏ theo chuẩn xác!
	}
}