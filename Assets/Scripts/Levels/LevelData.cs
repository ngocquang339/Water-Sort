using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelId;

	[Header("Cài đặt Chai nước")]
	public int bottleCapacity = 4;         
	public GameObject customBottlePrefab;

	[Header("Dữ liệu Màu sắc")]
	public BottleSetUp[] bottleInLevel;
}

[System.Serializable]
public class BottleSetUp{
	public WaterColor[] initialColors;
}
