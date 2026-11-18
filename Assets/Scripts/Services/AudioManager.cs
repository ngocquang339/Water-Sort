using UnityEngine;

public class AudioManager : MonoBehaviour
{
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
	public AudioClip chestDrop;
	public AudioClip chestOpen;
	public AudioClip itemPop;
	public AudioClip itemCollect;

	[Header("Nguồn âm thanh")]
	public AudioSource musicSource;
	public AudioSource sfxSource;
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void PlayButtonClick()
	{
		if (sfxSource != null && buttonClickClip != null)
		{
			sfxSource.PlayOneShot(buttonClickClip);
		}
	}

	public void PlayBottleLift()
	{
		if (sfxSource != null && bottleLift != null)
		{
			sfxSource.PlayOneShot(bottleLift);
		}
	}

	public void PlayChestDrop()
	{
		if (sfxSource != null && chestDrop != null)
		{
			sfxSource.PlayOneShot(chestDrop);
		}
	}

	public void PlayChestOpen()
	{
		if (sfxSource != null && chestOpen != null)
		{
			sfxSource.PlayOneShot(chestOpen);
		}
	}

	public void PlayItemPop()
	{
		if (sfxSource != null && itemPop != null)
		{
			sfxSource.PlayOneShot(itemPop);
		}
	}
	public void PlayItemCollect()
	{
		if (sfxSource != null && itemCollect != null)
		{
			sfxSource.PlayOneShot(itemCollect);
		}
	}
	public void PlayBottleDown()
	{
		if (sfxSource != null && bottleDown != null)
		{
			sfxSource.PlayOneShot(bottleDown);
		}
	}

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

	public void SetMusicVolume(float volume)
	{
		if (musicSource != null)
		{
			musicSource.volume = volume;
		}
	}

	public void SetSFXVolume(float volume)
	{
		if (sfxSource != null)
		{
			sfxSource.volume = volume;
		}
	}
}