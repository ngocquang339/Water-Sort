using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }

    public struct StepInfor
    {
        public Bottle A;
        public Bottle B;
        public int waterLayers;
    }

    private Stack<StepInfor> saveStepInfor = new Stack<StepInfor>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PushStep(Bottle a, Bottle b, int layers)
    {
        saveStepInfor.Push(new StepInfor { A = a, B = b, waterLayers = layers });
    }

    public bool BackStep()
    {
        if (saveStepInfor.Count > 0)
        {
            StepInfor lastStep = saveStepInfor.Peek();
            Bottle a = lastStep.A;
            Bottle b = lastStep.B;
            
            if (PourController.Instance != null && (PourController.Instance.IsBusyWith(a) || PourController.Instance.IsBusyWith(b))) return false;
            
            WaterColor color = b.getTopColor().Peek();
            for (int i = 0; i < lastStep.waterLayers; i++)
            {
                a.addWater(color);
            }
            b.removeTopColor(lastStep.waterLayers);
            a.updateBottleVisuals();
            b.updateBottleVisuals();
            
            if (!b.isCompleted()) b.corkObject.SetActive(false);
            if (!a.isCompleted()) a.corkObject.SetActive(false);
            
            saveStepInfor.Pop();
            return true;
        }
        return false;
    }
}
