using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputController : MonoBehaviour
{
    public static PlayerInputController Instance { get; private set; }

    private Bottle selectedBottle;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        LiftBottle();
    }

    private void LiftBottle()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !GameManager.instance.isLocked)
        {
            Bottle clickBottle = GetBottleFromClick();
            if (clickBottle != null && PourController.Instance != null)
            {
                if (PourController.Instance.IsBusyWith(clickBottle)) return;

                if (clickBottle == selectedBottle)
                {
                    PourController.Instance.DropBottle(selectedBottle);
                    selectedBottle = null;
                }
                else if (selectedBottle == null && !clickBottle.isCompleted())
                {
                    if (clickBottle.getTopColor() == null) return;
                    PourController.Instance.LiftUpBottle(clickBottle);
                    selectedBottle = clickBottle;
                }
                else
                {
                    if (selectedBottle != null)
                    {
                        PourController.Instance.ExecutePour(selectedBottle, clickBottle);
                        selectedBottle = null;
                    }
                }
            }
        }
    }

    private Bottle GetBottleFromClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Bottle>();
        }
        return null;
    }

    public void ClearSelection()
    {
        if (selectedBottle != null && PourController.Instance != null)
        {
            PourController.Instance.DropBottle(selectedBottle);
        }
        selectedBottle = null;
    }
    
    public Bottle GetSelectedBottle()
    {
        return selectedBottle;
    }
}
