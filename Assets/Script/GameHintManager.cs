using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Struct này để public ở ngoài cùng để cả 2 file đều dùng được
public struct PourStep { public int fromIndex; public int toIndex; }

public class GameHintManager : MonoBehaviour
{
	// HÀM NÀY BÂY GIỜ LÀ PUBLIC ĐỂ GAMEMANAGER GỌI
	// Chuyền thẳng danh sách chai từ GameManager sang đây cho nhẹ não
	public PourStep? FindHint(List<Bottle> allBottles)
	{
		if (allBottles == null || allBottles.Count == 0) return null;

		for (int i = 0; i < allBottles.Count; i++)
		{
			Bottle fromBottle = allBottles[i];
			if (fromBottle.isEmpty() || fromBottle.isCompleted()) continue;

			WaterColor topColor = fromBottle.getTopColor().Peek();

			for (int j = 0; j < allBottles.Count; j++)
			{
				if (i == j) continue;
				Bottle toBottle = allBottles[j];

				if (toBottle.isFull()) continue;

				// TRƯỜNG HỢP 1
				if (!toBottle.isEmpty() && toBottle.getTopColor().Peek() == topColor)
				{
					return new PourStep { fromIndex = i, toIndex = j };
				}

				// TRƯỜNG HỢP 2
				if (toBottle.isEmpty())
				{
					if (fromBottle.getTopColor().Count != fromBottle.currentWaterCount)
					{
						return new PourStep { fromIndex = i, toIndex = j };
					}
				}
			}
		}
		return null; // Deadlock
	}
}