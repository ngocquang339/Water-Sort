using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	[Header("---- Cấu Hình Sliders UI ----")]
	public Slider musicSlider; 
	public Slider sfxSlider;  

	void Start()
	{
		// 1. TẢI LẠI ÂM LƯỢNG CŨ: Mặc định là 1f (Max Volume) nếu chơi lần đầu
		float savedMusic = PlayerPrefs.GetFloat("Save_MusicVolume", 1f);
		float savedSFX = PlayerPrefs.GetFloat("Save_SFXVolume", 1f);

		// 2. Đập giá trị cũ vào thanh Slider để nút Handle nhảy về đúng vị trí
		if (musicSlider != null) musicSlider.value = savedMusic;
		if (sfxSlider != null) sfxSlider.value = savedSFX;

		// 3. Ép AudioManager áp dụng ngay mức âm lượng này khi vừa mở bảng Settings
		if (AudioManager.instance != null)
		{
			AudioManager.instance.SetMusicVolume(savedMusic);
			AudioManager.instance.SetSFXVolume(savedSFX);
		}

		// 4. LẮNG NGHE SỰ KIỆN: Khi người chơi kéo thanh, tự động kích hoạt hàm đổi volume
		if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
		if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
	}

	// Hàm tự động chạy mỗi khi thanh Nhạc nền bị kéo
	private void OnMusicVolumeChanged(float value)
	{
		if (AudioManager.instance != null)
		{
			AudioManager.instance.SetMusicVolume(value);
		}
		// Lưu lại ngay lập tức vào máy
		PlayerPrefs.SetFloat("Save_MusicVolume", value);
	}

	// Hàm tự động chạy mỗi khi thanh SFX bị kéo
	private void OnSFXVolumeChanged(float value)
	{
		if (AudioManager.instance != null)
		{
			AudioManager.instance.SetSFXVolume(value);
		}
		PlayerPrefs.SetFloat("Save_SFXVolume", value);
	}

	private void OnDestroy()
	{
		// Hủy lắng nghe khi object bị xóa để tránh rác bộ nhớ (Memory Leak)
		if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
		if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
	}
}