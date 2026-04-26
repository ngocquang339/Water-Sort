using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Bottle : MonoBehaviour
{
	[Header("Cài đặt Màu sắc & UI")]
	public List<ColorMapping> colorDatabase;
	public SpriteRenderer[] waterLayerRenderers;

	[Header("Logic Dữ liệu")]
	public int capacity = 4;
    private Stack<WaterColor> waterLayers = new Stack<WaterColor>();

    public bool isFull(){
        return waterLayers.Count == capacity;
    }

    public bool isEmpty(){
        return waterLayers.Count == 0;
    }

    public WaterColor getTopColor(){
        if(isEmpty()){
			Debug.LogWarning($"{gameObject.name} đang trống");
			return WaterColor.None;
        }
        return waterLayers.Peek();
    }

    public WaterColor removeTopColor(){
        if (isEmpty()) {
            Debug.LogWarning($"{gameObject.name} đang trống, không có gì để đổi");
            return WaterColor.None;
        }
        WaterColor topColor = waterLayers.Pop();
        Debug.Log($"Đã lấy màu {topColor} ra khỏi {gameObject.name}");
        return topColor;
    }

    public bool addNewColor(WaterColor color){
        if(!isFull()){
            if(color == getTopColor() || isEmpty()){
				waterLayers.Push(color);
				Debug.Log($"Đã thêm màu {color} vào {gameObject.name}");
				return true;
			}
			else{
				return false;
			}
		}
        else{
			Debug.LogWarning($"{gameObject.name} đã đầy, không thể thêm!");
			return false;
		}
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
		// Chuyển Stack thành mảng. Mặc định ToArray() của Stack sẽ lấy phần tử trên cùng làm index 0.
		// Ta cần dùng Reverse() để đảo ngược lại: index 0 trở thành phần tử dưới đáy.
		WaterColor[] currentStackArray = waterLayers.Reverse().ToArray();

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
}
