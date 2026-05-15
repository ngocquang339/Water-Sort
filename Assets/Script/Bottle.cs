using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Bottle : MonoBehaviour
{
	[Header("Cài đặt Màu sắc & UI")]
	public List<ColorMapping> colorDatabase;
	public SpriteRenderer[] waterLayerRenderers;

	[Header("Cấu hình Mặt nước")]
	public float[] surfaceYPositions;

	public SpriteRenderer ovalInsideRenderer;
	public SpriteRenderer ovalBorderRenderer;

	[Header("Logic Dữ liệu")]
	public int capacity = 4;
	private Stack<WaterColor> waterLayers = new Stack<WaterColor>();
	public Transform mouthPoint;

	public bool isFull()
	{
		return waterLayers.Count == capacity;
	}

	public bool isEmpty()
	{
		return waterLayers.Count == 0;
	}

	public Stack<WaterColor> getTopColor()
	{
		if (isEmpty())
		{
			Debug.LogWarning($"{gameObject.name} đang trống");
			return null;
		}
		Stack<WaterColor> color = new Stack<WaterColor>();
		WaterColor[] colorArray = waterLayers.ToArray();
		color.Push(colorArray[0]);
		for (int i = 1; i < colorArray.Length; i++)
		{
			if (colorArray[i] == colorArray[i - 1])
			{
				color.Push(colorArray[i]);
			}
			else
			{
				break;
			}
		}

		return color; ;
	}

	public void removeTopColor(int count)
	{
		if (isEmpty())
		{
			Debug.LogWarning($"{gameObject.name} đang trống, không có gì để đổi");
			return;
		}
		for (int i = 0; i < count; i++)
		{
			waterLayers.Pop();
		}
	}

	public int addNewColor(Stack<WaterColor> color)
	{
		int amount = color.Count;
		int count = 0;
		for (int i = 0; i < amount; i++)
		{
			if (!isFull())
			{
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
		int currentCount = waterLayers.Count;
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
		if (currentCount == 0)
		{
			ovalInsideRenderer.gameObject.SetActive(false);
			ovalBorderRenderer.gameObject.SetActive(false);
		}
		else
		{
			ovalInsideRenderer.gameObject.SetActive(true);
			ovalBorderRenderer.gameObject.SetActive(true);

			// BÍ QUYẾT: Lấy đúng tọa độ Y từ mảng dựa trên số lượng nước hiện tại
			// currentCount - 1 vì mảng bắt đầu từ 0
			if (currentCount <= surfaceYPositions.Length)
			{
				float targetY = surfaceYPositions[currentCount - 1];

				// Cập nhật vị trí cho cụm Oval
				Vector3 newPos = ovalInsideRenderer.transform.parent.localPosition;
				newPos.y = targetY;
				ovalInsideRenderer.transform.parent.localPosition = newPos;
			}

			// Cập nhật màu sắc mặt Oval (Sáng và tươi hơn màu nước)
			WaterColor topColor = getTopColor().Peek();
			Color baseUnityColor = GetUnityColor(topColor);

			// Tách màu gốc sang hệ HSV (Hue, Saturation, Value)
			float h, s, v;
			Color.RGBToHSV(baseUnityColor, out h, out s, out v);

			// Ép bớt độ đậm (S) và tăng độ sáng (V)
			s = Mathf.Clamp01(s - 0.2f);
			v = Mathf.Clamp01(v + 0.3f);

			// Chuyển ngược lại thành màu Unity và gán cho Oval
			Color brighterColor = Color.HSVToRGB(h, s, v);

			ovalInsideRenderer.color = brighterColor;
			ovalBorderRenderer.color = Color.white;
		}
	}

	public Color GetUnityColor(WaterColor targetColor)
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

	public void addWater(WaterColor color)
	{
		waterLayers.Push(color);
	}

	// --- THÊM 2 HÀM NÀY VÀO BOTTLE.CS ---
	// Lấy số lượng tầng nước hiện tại
	public int currentWaterCount => waterLayers.Count;

	// Tính toán tọa độ Y cho mặt Oval một cách an toàn
	public float GetOvalYPosition(int waterCount)
	{
		if (waterCount <= 0)
			return surfaceYPositions[0] - 0.4f; // Nếu cạn sạch, cho Oval chìm xuống dưới đáy
		if (waterCount > surfaceYPositions.Length)
			return surfaceYPositions[surfaceYPositions.Length - 1];
		return surfaceYPositions[waterCount - 1];
	}
}