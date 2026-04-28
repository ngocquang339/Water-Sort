using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardData", menuName = "Scriptable Objects/DailyRewardData")]
public class DailyRewardData : ScriptableObject
{
	[Header("Danh sách phần thưởng điểm danh")]
	[Tooltip("Hãy nhập Size = 7 cho 7 ngày đăng nhập")]
	public RewardItem[] rewards;
}
