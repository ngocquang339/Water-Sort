using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private Bottle selectedBottle;
	[SerializeField] private float liftOffset = 0.5f;
	private Bottle b1;
	private Bottle b2;

    void Update()
    {
		liftBottle();
    }

    private void liftBottle(){
		if (Input.GetMouseButtonDown(0))
		{
			
			Bottle clickBottle = getBottleFromClick();
			if (clickBottle != null)
			{

				if (clickBottle == selectedBottle)
				{

					clickBottle.transform.position -= new Vector3(0f, liftOffset, 0f);
					selectedBottle = null;

				}
				else if (selectedBottle == null)
				{
					clickBottle.transform.position += new Vector3(0f, liftOffset, 0f);
					selectedBottle = clickBottle;
				}
				else{
					processPourWater(selectedBottle, clickBottle);
					selectedBottle.transform.position -= new Vector3(0f, liftOffset, 0f);
					selectedBottle = null;
				}
			}
			
		}
	}

	private Bottle getBottleFromClick(){
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
		if(hit.collider != null){
			Bottle b = hit.collider.GetComponent<Bottle>();
		return b;
		}

		return null;
	}

	private void processPourWater(Bottle a, Bottle b){
		if (a == null || b == null) return;
		if (b.isFull() || a.isEmpty()) return;
		WaterColor color = a.getTopColor();
		if(b.addNewColor(color)){
			a.removeTopColor();
			a.updateBottleVisuals();
			b.updateBottleVisuals();
		}
	}
}
