using System.Collections.Generic;
using System.Collections;
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

	[Header("Cài đặt Nút Bần")]
	public GameObject corkObject; // Kéo thả cái nút bần vào đây
	public float corkDropHeight = 1.0f; // Khoảng cách nắp đậy rơi xuống (từ trên cao)
	public float corkDropDuration = 0.3f; // Thời gian rơi (0.3 giây là vừa đủ nhanh và dứt khoát)

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

	// ========================================================================
	// --- THÊM CÁC HÀM NÀY VÀO DƯỚI CÙNG CỦA CLASS BOTTLE.CS ĐỂ HỖ TRỢ HINT ---
	// ========================================================================

	// 1. Cấu trúc dữ liệu phụ (Struct) để lưu trữ CHỈ LOGIC của 1 chai.
	// Giống như một tấm bản đồ, không phải là cái chai thật.
	[System.Serializable]
	public struct BottleLogicState
	{
		public int capacity;
		public WaterColor[] layers; // Dùng mảng thay vì Stack để BFS dễ tính toán

		// Hàm trợ giúp: Kiểm tra xem trạng thái giả lập này có hoàn thiện không
		public bool IsComplete()
		{
			if (layers.Length == 0) return true; // Rỗng = hoàn thiện
			if (layers.Length != capacity) return false;
			WaterColor baseColor = layers[0];
			foreach (WaterColor c in layers) { if (c != baseColor) return false; }
			return true;
		}
	}

	// 2. HÀM CHỤP TRẠNG THÁI (Snapshot):
	// Hàm này được gọi bởi Manager. Nó tạo ra một bản sao logic rỗng,
	// copy capacity và copy toàn bộ các tầng màu nước hiện có vào mảng 'layers'.
	public BottleLogicState GetLogicState()
	{
		return new BottleLogicState
		{
			capacity = this.capacity,
			// ToArray() tạo ra một bản sao mảng mới, không ảnh hưởng stack thật
			layers = this.waterLayers.ToArray()
		};
	}

	// 3. HÀM KIỂM TRA HOÀN THIỆN (True/False):
	// Thường dùng để GameManager kiểm tra điều kiện Win màn.
	// Yêu cầu: Chai phải đầy (capacity) VÀ tất cả các tầng màu phải giống hệt nhau.
	public bool isCompleted()
	{
		if (isEmpty()) return true; // Chai rỗng được coi là hoàn thiện
		if (!isFull()) return false; // Chưa đầy thì không hoàn thiện

		WaterColor[] currentArray = waterLayers.ToArray();
		WaterColor firstColor = currentArray[0];

		// Kiểm tra tất cả các tầng màu xem có giống tầng đáy không
		for (int i = 1; i < currentArray.Length; i++)
		{
			if (currentArray[i] != firstColor) return false;
		}
		return true;
	}

	// Hàm công khai để GameManager gọi
	public void CloseCork()
	{
		if (corkObject != null)
		{
			StartCoroutine(AnimateCorkRoutine());
		}
		else
		{
			Debug.LogWarning($"Chai {gameObject.name} chưa được gắn nút bần!");
		}
	}

	// Coroutine xử lý Animation rơi nắp
	private IEnumerator AnimateCorkRoutine()
	{
		// 1. Lưu lại vị trí chuẩn (đích đến) mà bạn đã căn chỉnh bằng tay ngoài Scene
		Vector3 finalPos = corkObject.transform.localPosition;

		// 2. Tính toán vị trí bắt đầu (Cao hơn vị trí chuẩn một đoạn)
		Vector3 startPos = finalPos + new Vector3(0f, corkDropHeight, 0f);

		// 3. Đưa nắp lên cao và Bật cho nó hiện lên
		corkObject.transform.localPosition = startPos;
		corkObject.SetActive(true);

		// 4. Di chuyển mượt mà từ trên xuống
		float timePassed = 0f;
		while (timePassed < corkDropDuration)
		{
			timePassed += Time.deltaTime;
			float percent = timePassed / corkDropDuration;

			// Mẹo Pro: Dùng SmoothStep thay vì Lerp thường để nắp rơi có gia tốc (nhanh dần rồi hãm lại ở đáy)
			float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

			corkObject.transform.localPosition = Vector3.Lerp(startPos, finalPos, smoothPercent);
			yield return null;
		}

		// 5. Chốt vị trí cuối cùng để tránh sai số
		corkObject.transform.localPosition = finalPos;
	}

	// --- THÊM VÀO BOTTLE.CS ---
	public void MakeEmptyBottle()
	{
		// 1. DỌN SẠCH DỮ LIỆU BÊN TRONG
		waterLayers.Clear();

		// 2. TẮT HIỂN THỊ CỦA CÁC LỚP NƯỚC BÊN NGOÀI
		if (waterLayerRenderers != null)
		{
			for (int i = 0; i < waterLayerRenderers.Length; i++)
			{
				// CHỈ CẦN ẨN ĐI LÀ ĐỦ. TUYỆT ĐỐI KHÔNG ÉP SCALE VỀ 0 Ở ĐÂY NỮA!
				// Để nguyên kích thước gốc cho GameManager đọc được chiều cao chuẩn.
				waterLayerRenderers[i].gameObject.SetActive(false);
			}
		}

		// Tắt luôn mặt Oval đi vì chai rỗng thì không có bề mặt nước
		if (ovalInsideRenderer != null) ovalInsideRenderer.gameObject.SetActive(false);
		if (ovalBorderRenderer != null) ovalBorderRenderer.gameObject.SetActive(false);

		// 3. ĐẢM BẢO MỞ NẮP CHAI
		if (corkObject != null)
		{
			corkObject.SetActive(false);
		}
	}
}