using UnityEngine;

public class RotateUI : MonoBehaviour
{
	[Header("Tốc độ xoay (Âm = Cùng chiều kim đồng hồ)")]
	public float rotationSpeed = -50f;

	void Update()
	{
		// Xoay liên tục trục Z theo thời gian thực
		transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
	}
}