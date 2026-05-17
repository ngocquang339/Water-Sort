using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Sử dụng Linq để thao tác mảng/list dễ hơn
using System.Text; // Sử dụng StringBuilder để tạo hash nhanh

// Struct lưu trữ thông tin 1 bước rót, dùng index của chai
public struct PourStep { public int fromIndex; public int toIndex; }

public class GameHintManager : MonoBehaviour
{
	public List<Bottle> allBottles; // Kéo thả tất cả chai nước vào đây
	public Button hintButton; // Kéo thả nút Hint vào đây
	public GameManager gameManager;
	void Start() { hintButton.onClick.AddListener(OnHintClick); }

	void OnHintClick()
	{
		allBottles = gameManager.allBottles;
		if (allBottles == null || allBottles.Count == 0) return;
		PourStep? hint = FindHint();
		if (hint.HasValue) { ShowHint(hint.Value); }
		else { Debug.Log("Không tìm thấy bước đi hợp lệ!"); }
	}

	PourStep? FindHint()
	{
		// Duyệt qua tất cả các chai để tìm chai Nguồn (Chai rót đi)
		for (int i = 0; i < allBottles.Count; i++)
		{
			Bottle fromBottle = allBottles[i];

			// Bỏ qua nếu chai rỗng hoặc đã hoàn thiện
			if (fromBottle.isEmpty() || fromBottle.isCompleted()) continue;

			WaterColor topColor = fromBottle.getTopColor().Peek();

			// Duyệt tìm chai Đích (Chai nhận nước)
			for (int j = 0; j < allBottles.Count; j++)
			{
				if (i == j) continue; // Không tự rót vào chính nó
				Bottle toBottle = allBottles[j];

				if (toBottle.isFull()) continue; // Bỏ qua nếu chai đích đã đầy

				// TRƯỜNG HỢP 1: Chai đích có nước và màu trên cùng giống nhau -> Rót được!
				if (!toBottle.isEmpty() && toBottle.getTopColor().Peek() == topColor)
				{
					return new PourStep { fromIndex = i, toIndex = j };
				}

				// TRƯỜNG HỢP 2: Chai đích rỗng -> Rót được!
				if (toBottle.isEmpty())
				{
					// ĐIỀU KIỆN CHỐNG NGU: 
					// Nếu chai Nguồn chỉ có duy nhất 1 khối màu nguyên khối, đừng xúi người chơi rót sang chai rỗng khác làm gì cho phí bước.
					// Chỉ xúi rót sang chai rỗng nếu chai Nguồn đang bị lộn xộn nhiều màu.
					if (fromBottle.getTopColor().Count != fromBottle.currentWaterCount)
					{
						return new PourStep { fromIndex = i, toIndex = j };
					}
				}
			}
		}

		// Nếu chạy hết 2 vòng lặp mà không tìm được, nghĩa là màn chơi đã Deadlock
		return null;
	}

	// 7. HÀM HIỂN THỊ GỢI Ý (ShowHint):
	// Nhận bước rót (PourStep) và hiển thị nó ra màn hình.
	void ShowHint(PourStep hintStep)
	{
		// Chuyển đổi index thành Object chai nước thật trên Scene
		Bottle from = allBottles[hintStep.fromIndex];
		Bottle to = allBottles[hintStep.toIndex];

		Debug.Log($"Gợi ý: Tự động rót từ {from.name} sang {to.name}");

		// Ra lệnh cho Sếp GameManager thực hiện animation ngay lập tức!
		if (gameManager != null)
		{
			gameManager.ExecuteHintPour(from, to);
		}
		else
		{
			Debug.LogError("Bạn quên kéo GameManager vào ô Game Hint Manager ở Inspector rồi!");
		}
	}
}

// Struct lưu trữ trạng thái của 1 chai
// Cập nhật: Sử dụng WaterColor enum thay vì Color
struct BottleState { public int id; public int capacity; public List<WaterColor> layers; }