using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelId;
    public BottleSetUp[] bottleInLevel;
}

[System.Serializable]
public class BottleSetUp{
    public WaterColor[] initialColors = new WaterColor[4];
}
