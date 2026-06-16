using UnityEngine;
using TMPro; // Đừng quên thư viện này để dùng TextMeshPro

public class VersusTimer : MonoBehaviour
{
	[Header("UI Hiển Thị")]
	public TextMeshProUGUI p1TimerText;
	public TextMeshProUGUI p2TimerText;

	private float elapsedTime = 0f;
	private bool isRunning = false;

	void Start()
	{
		// Vừa vào Scene là bắt đầu đếm giờ luôn
		StartTimer();
	}

	void Update()
	{
		if (isRunning)
		{
			// Cộng dồn thời gian theo từng khung hình
			elapsedTime += Time.deltaTime;

			// Chuyển đổi thành Phút và Giây
			int minutes = Mathf.FloorToInt(elapsedTime / 60f);
			int seconds = Mathf.FloorToInt(elapsedTime % 60f);

			// Định dạng chuỗi hiển thị có số 0 đằng trước (VD: 02:05)
			string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

			// Cập nhật lên cả 2 màn hình
			p1TimerText.text = timeString;
			p2TimerText.text = timeString;
		}
	}

	public void StartTimer()
	{
		elapsedTime = 0f;
		isRunning = true;
	}

	public void StopTimer()
	{
		isRunning = false;
	}
}