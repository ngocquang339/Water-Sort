using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpManager : MonoBehaviour
{

    public static HelpManager Instance { get; private set; }

    [Header("Cài đặt Thêm Chai")]
    public int maxExtraBottlesPerLevel = 1;
    private int extraBottlesUsedThisLevel = 0;
    public Button addBottleButton;

    public GameHintManager gameHintManager;
    public LevelManager levelManager;

    private float remainingUndo;
    private float remainingHint;
    private float remainingAddBottle;

    private const string KEY_UNDO = "Help_Undo";
    private const string KEY_HINT = "Help_Hint";
    private const string KEY_ADD_BOTTLE = "Help_AddBottle";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadHelpQuantities();
    }

    private void LoadHelpQuantities()
    {
        remainingUndo = PlayerPrefs.GetFloat(KEY_UNDO, 2);
        remainingHint = PlayerPrefs.GetFloat(KEY_HINT, 2);
        remainingAddBottle = PlayerPrefs.GetFloat(KEY_ADD_BOTTLE, 2);

        UpdateHelpUI();
    }

    private void UpdateHelpUI()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHelpUI(remainingUndo, remainingHint, remainingAddBottle);
        }
    }

    public void AddHelpQuantity(HelpType type, int amount)
    {
        switch (type)
        {
            case HelpType.Undo:
                remainingUndo += amount;
                PlayerPrefs.SetFloat(KEY_UNDO, remainingUndo);
                break;
            case HelpType.Hint:
                remainingHint += amount;
                PlayerPrefs.SetFloat(KEY_HINT, remainingHint);
                break;
            case HelpType.AddBottle:
                remainingAddBottle += amount;
                PlayerPrefs.SetFloat(KEY_ADD_BOTTLE, remainingAddBottle);
                break;
        }

        PlayerPrefs.Save();
        UpdateHelpUI();
    }

    public void OnClickUndoButton()
    {
        if (GameManager.instance.isLocked) return;
        
        if (remainingUndo > 0)
        {
            if (UndoManager.Instance != null)
            {
                bool success = UndoManager.Instance.BackStep();
                if (success)
                {
                    remainingUndo--;
                    PlayerPrefs.SetFloat(KEY_UNDO, remainingUndo);
                    PlayerPrefs.Save();
                    UpdateHelpUI();
                }
            }
        }
        else
        {
            Debug.Log("Đã hết lượt Undo!");
            if (PopupManager.instance != null) PopupManager.instance.ShowOutOfHelpPopup(HelpType.Undo);
        }
    }

    public void UseHint()
    {
        if (GameManager.instance.isLocked) return;
        
        if (remainingHint <= 0)
        {
            Debug.Log("Hết lượt dùng Gợi ý rồi!");
            if (PopupManager.instance != null) PopupManager.instance.ShowOutOfHelpPopup(HelpType.Hint);
            return;
        }

        if (PourController.Instance != null && PourController.Instance.IsBusy()) return;

        if (gameHintManager != null && PourController.Instance != null)
        {
            PourStep? hintStep = gameHintManager.FindHint(PourController.Instance.allBottles);

            if (hintStep.HasValue)
            {
                Bottle fromBottle = PourController.Instance.allBottles[hintStep.Value.fromIndex];
                Bottle toBottle = PourController.Instance.allBottles[hintStep.Value.toIndex];

                Debug.Log($"Gợi ý: Tự động rót từ {fromBottle.name} sang {toBottle.name}");

                PourController.Instance.ExecuteHintPour(fromBottle, toBottle);

                remainingHint--;
                PlayerPrefs.SetFloat(KEY_HINT, remainingHint);
                PlayerPrefs.Save();
                UpdateHelpUI();
            }
            else
            {
                Debug.Log("Màn chơi bế tắc, không có bước đi gợi ý nào!");
            }
        }
    }

    public void UseAddBottle()
    {
        if (GameManager.instance.isLocked) return;
        
        if (extraBottlesUsedThisLevel >= maxExtraBottlesPerLevel)
        {
            Debug.Log("Màn này đã đạt giới hạn thêm chai rồi!");
            return;
        }

        if (remainingAddBottle <= 0)
        {
            Debug.Log("Hết lượt Thêm bình");
            if (PopupManager.instance != null) PopupManager.instance.ShowOutOfHelpPopup(HelpType.AddBottle);
            return;
        }

        if (PourController.Instance != null && PourController.Instance.IsBusy()) return;

        Vector3 spawnPos = new Vector3(0, 10f, 0);
        GameObject emptyBottlePrefab = levelManager.currentLevelData.customBottlePrefab;
        GameObject newBottleObj = Instantiate(emptyBottlePrefab, spawnPos, Quaternion.identity);
        Bottle newBottle = newBottleObj.GetComponent<Bottle>();

        if (newBottle != null)
        {
            newBottle.capacity = levelManager.currentLevelData != null ? levelManager.currentLevelData.bottleCapacity : 4;
            newBottle.MakeEmptyBottle();
        }

        if (PourController.Instance != null)
        {
            PourController.Instance.allBottles.Add(newBottle);
            StartCoroutine(PourController.Instance.RearrangeBottlesRoutine(levelManager));
        }

        extraBottlesUsedThisLevel++;
        if (extraBottlesUsedThisLevel >= maxExtraBottlesPerLevel)
        {
            if (addBottleButton != null)
            {
                addBottleButton.interactable = false;
            }
        }

        remainingAddBottle--;
        PlayerPrefs.SetFloat(KEY_ADD_BOTTLE, remainingAddBottle);
        PlayerPrefs.Save();
        UpdateHelpUI();
    }
}
