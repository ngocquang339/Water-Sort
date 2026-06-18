using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BottleMulti : MonoBehaviour
{
	[Header("Cài đặt Màu sắc & UI")]
	public List<ColorMapping> colorDatabase;
	// 👇 KHÔI PHỤC LẠI MẢNG 6 LỚP NƯỚC
	public SpriteRenderer[] waterLayerRenderers;

	[Header("Cấu hình Mặt nước")]
	public float[] surfaceYPositions;

	public SpriteRenderer ovalInsideRenderer;
	public SpriteRenderer ovalBorderRenderer;

	[Header("Logic Dữ liệu")]
	public int capacity = 4;
	private Stack<WaterColor> waterLayers = new Stack<WaterColor>();
	public Transform mouthPoint;

	[Header("Cài đặt Nút Bần")]
	public GameObject corkObject;
	public float corkDropHeight = 1.0f;
	public float corkDropDuration = 0.3f;

	public int playerID;

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

		return color;
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

	// =======================================================
	// [SHADER MỚI] GIAO TIẾP VỚI SHADER 1 LỚP ẢNH
	// =======================================================
	public void updateBottleVisuals()
	{
		Debug.Log(gameObject.name + " đang cập nhật hiển thị, số lượng nước: " + waterLayers.Count);
		int currentCount = waterLayers.Count;
		WaterColor[] currentStackArray = waterLayers.Reverse().ToArray(); // Đảo ngược: Đáy = index 0

		// 👇 QUAY LẠI CÁCH BẬT/TẮT VÀ TÔ MÀU TỪNG LỚP ẢNH
		for (int i = 0; i < waterLayerRenderers.Length; i++)
		{
			if (i < currentStackArray.Length)
			{
				waterLayerRenderers[i].gameObject.SetActive(true);
				waterLayerRenderers[i].color = GetUnityColor(currentStackArray[i]);
			}
			else
			{
				waterLayerRenderers[i].gameObject.SetActive(false);
			}
		}

		// --- PHẦN MẶT OVAL GIỮ NGUYÊN (CHỈ TỐI ƯU LẠI 1 CHÚT) ---
		if (currentCount == 0)
		{
			ovalInsideRenderer.gameObject.SetActive(false);
			ovalBorderRenderer.gameObject.SetActive(false);
		}
		else
		{
			ovalInsideRenderer.gameObject.SetActive(true);
			ovalBorderRenderer.gameObject.SetActive(true);

			if (currentCount <= surfaceYPositions.Length)
			{
				float targetY = surfaceYPositions[currentCount - 1];
				Vector3 newPos = ovalInsideRenderer.transform.parent.localPosition;
				newPos.y = targetY;
				ovalInsideRenderer.transform.parent.localPosition = newPos;
			}

			WaterColor topColor = getTopColor().Peek();
			Color baseUnityColor = GetUnityColor(topColor);

			float h, s, v;
			Color.RGBToHSV(baseUnityColor, out h, out s, out v);
			s = Mathf.Clamp01(s - 0.2f);
			v = Mathf.Clamp01(v + 0.3f);
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

	public int currentWaterCount => waterLayers.Count;

	public float GetOvalYPosition(int waterCount)
	{
		if (waterCount <= 0)
			return surfaceYPositions[0] - 0.4f;
		if (waterCount > surfaceYPositions.Length)
			return surfaceYPositions[surfaceYPositions.Length - 1];
		return surfaceYPositions[waterCount - 1];
	}

	[System.Serializable]
	public struct BottleLogicState
	{
		public int capacity;
		public WaterColor[] layers;

		public bool IsComplete()
		{
			if (layers.Length == 0) return true;
			if (layers.Length != capacity) return false;
			WaterColor baseColor = layers[0];
			foreach (WaterColor c in layers) { if (c != baseColor) return false; }
			return true;
		}
	}

	public BottleLogicState GetLogicState()
	{
		return new BottleLogicState
		{
			capacity = this.capacity,
			layers = this.waterLayers.ToArray()
		};
	}

	public bool isCompleted()
	{
		if (isEmpty()) return true;
		if (!isFull()) return false;

		WaterColor[] currentArray = waterLayers.ToArray();
		WaterColor firstColor = currentArray[0];

		for (int i = 1; i < currentArray.Length; i++)
		{
			if (currentArray[i] != firstColor) return false;
		}
		return true;
	}

	public void CloseCork()
	{
		if (corkObject != null)
		{
			AudioManager.instance.PlayDoneBottle();
			StartCoroutine(AnimateCorkRoutine());
		}
		else
		{
			Debug.LogWarning($"Chai {gameObject.name} chưa được gắn nút bần!");
		}
	}

	private IEnumerator AnimateCorkRoutine()
	{
		Vector3 finalPos = corkObject.transform.localPosition;
		Vector3 startPos = finalPos + new Vector3(0f, corkDropHeight, 0f);

		corkObject.transform.localPosition = startPos;
		corkObject.SetActive(true);

		float timePassed = 0f;
		while (timePassed < corkDropDuration)
		{
			timePassed += Time.deltaTime;
			float percent = timePassed / corkDropDuration;
			float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

			corkObject.transform.localPosition = Vector3.Lerp(startPos, finalPos, smoothPercent);
			yield return null;
		}

		corkObject.transform.localPosition = finalPos;
	}

	//public void MakeEmptyBottle()
	//{
	//	waterLayers.Clear();

	//	// [SHADER MỚI] Dọn sạch chai bằng cách kéo Fill về 0
	//	if (liquidBodyRenderer != null)
	//	{
	//		Material mat = liquidBodyRenderer.material;
	//		for (int i = 1; i <= 6; i++)
	//		{
	//			mat.SetFloat("_Fill" + i, 0f);
	//		}
	//	}

	//	if (ovalInsideRenderer != null) ovalInsideRenderer.gameObject.SetActive(false);
	//	if (ovalBorderRenderer != null) ovalBorderRenderer.gameObject.SetActive(false);

	//	if (corkObject != null)
	//	{
	//		corkObject.SetActive(false);
	//	}
	//}
}