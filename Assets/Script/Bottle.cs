using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Bottle : MonoBehaviour
{
	[Header("Cài đặt Màu sắc & UI")]
	public List<ColorMapping> colorDatabase;
	public SpriteRenderer[] waterLayerRenderers;

	[Header("Logic Dữ liệu")]
	public int capacity = 4;
    private Stack<WaterColor> waterLayers = new Stack<WaterColor>();
	public Transform mouthPoint;

	public bool isFull(){
        return waterLayers.Count == capacity;
    }

    public bool isEmpty(){
        return waterLayers.Count == 0;
    }

    public Stack<WaterColor> getTopColor(){
        if(isEmpty()){
			Debug.LogWarning($"{gameObject.name} đang trống");
			return null;
        }
		Stack<WaterColor> color = new Stack<WaterColor>();
		WaterColor[] colorArray = waterLayers.ToArray();
		color.Push(colorArray[0]);
		for(int i = 1; i < colorArray.Length; i++){
			if (colorArray[i] == colorArray[i - 1])
			{
				color.Push(colorArray[i]);
			}
			else{
				break;
			}
		}
		
		return color; ;
    }

    public void removeTopColor(int count){
        if (isEmpty()) {
            Debug.LogWarning($"{gameObject.name} đang trống, không có gì để đổi");
			return;
        }
		for(int i = 0; i < count; i++){
			waterLayers.Pop();
		}
    }

    public int addNewColor(Stack<WaterColor> color){
		int amount = color.Count;
		int count = 0;
		for(int i = 0; i < amount; i++){
			if(!isFull()){
				if (isEmpty() || waterLayers.Peek() == color.Peek())
				{
					waterLayers.Push(color.Pop());
					count++;
				}
			}
		}
		return count;
	}

	public void initializeColors(WaterColor[] initialColors)
	{
		waterLayers.Clear();

		for (int i = 0; i < initialColors.Length; i++)
		{
			if (initialColors[i] != WaterColor.None)
			{
				waterLayers.Push(initialColors[i]);
			}
		}
		updateBottleVisuals();
	}

	public void updateBottleVisuals()
	{
		WaterColor[] currentStackArray = waterLayers.Reverse().ToArray(); // Đảo ngược để phần tử dưới đáy thành index 0

		for (int i = 0; i < waterLayerRenderers.Length; i++)
		{
			if (i < currentStackArray.Length)
			{
				// Nếu vị trí này có nước trong Stack -> Hiện màu
				waterLayerRenderers[i].gameObject.SetActive(true);
				waterLayerRenderers[i].color = GetUnityColor(currentStackArray[i]);
			}
			else
			{
				// Nếu vượt quá số lượng trong Stack -> Ẩn cục nước đó đi
				waterLayerRenderers[i].gameObject.SetActive(false);
			}
		}
	}

	private Color GetUnityColor(WaterColor targetColor)
	{
		foreach (var mapping in colorDatabase)
		{
			if (mapping.colorEnum == targetColor) return mapping.colorValue;
		}
		return Color.white;
	}

	[System.Serializable]
	public struct ColorMapping
	{
		public WaterColor colorEnum;
		public Color colorValue;  
	}

	public void addWater(WaterColor color){
		waterLayers.Push(color);
	}
}
