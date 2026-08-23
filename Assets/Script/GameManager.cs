using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isLocked = false;
    
    [SerializeField] private PopupManager popupManager;
    [SerializeField] private ShopManager shopManager;
  

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void startGame()
    {
        if (LoadingSceneSmooth.Instance != null)
            LoadingSceneSmooth.Instance.StartCoroutine(LoadingSceneSmooth.Instance.LoadSceneSmooth("MainPlayScene"));
    }

    public void reloadLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void CheckGameState()
    {
        if (PourController.Instance != null && !PourController.Instance.HasAnyValidMove() && !CheckWin())
        {
            Debug.Log("Hết bước đi! Chuẩn bị hiện thông báo...");
            isLocked = true;
            if (GameUIManager.Instance != null)
            {
                StartCoroutine(GameUIManager.Instance.ShowDeadlockPopup());
            }
        }
    }

    public bool CheckWin()
    {
        if (PourController.Instance == null) return false;

        bool isWin = true;
        foreach (Bottle bottle in PourController.Instance.allBottles)
        {
            if (!bottle.isEmpty() && !bottle.isCompleted())
            {
                isWin = false;
                break;
            }
        }

        if (isWin)
        {
            StartCoroutine(HandleWinCoroutine());
            return true;
        }
        return false;
    }

    private IEnumerator HandleWinCoroutine()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowWinUI();
        }
        yield return new WaitForSeconds(2.0f); // Đợi UI chạy xong
        SaveProgress();
        RewardPlayer();
    }

    private void SaveProgress()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.SetInt("LevelNumber", currentLevel + 1);
        PlayerPrefs.Save();
    }

    private void RewardPlayer()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCoin(20);
            Debug.Log("Đã thêm 20 Coin vào ví.");
        }
    }

    public void AddHelpQuantity(HelpType type, int amount)
    {
        if (HelpManager.Instance != null)
        {
            HelpManager.Instance.AddHelpQuantity(type, amount);
        }
    }

    public void DiamondNotAvailableToast()
    {
        Debug.Log("Đang hiển thị thông báo Diamond không đủ!");
        if (ToastManager.instance != null && shopManager != null)
        {
            ToastManager.instance.ShowToast(shopManager.diamondNotAvailable_Panel, shopManager.diamondNotAvailable_Text.text);
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public bool HasAnyValidMove()
    {
        if (PourController.Instance != null)
            return PourController.Instance.HasAnyValidMove();
        return false;
    }
}