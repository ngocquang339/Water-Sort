using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("UI Bế Tắc")]
    public GameObject outOfMovesPopup;
    public GameObject dark_Panel;
    public TextMeshProUGUI currentLevelText;

    [Header("Hiệu ứng Pháo Hoa")]
    public ParticleSystem bottleDonePrefab;
    public ParticleSystem leftConfetti;
    public ParticleSystem rightConfetti;

    [Header("Win Game Effects")]
    public GameObject blackOverlay;
    public GameObject  confettiRainPrefab;
    public RectTransform winPanelRect;
    public GameObject winUIPanel;

    [Header("Next Level Popup")]
    public RectTransform nextLevelPopupRect;
    public GameObject nextLevelPopup;

    [Header("Settings Popup")]
    public GameObject settingPopup;
    public GameObject settingDarkPanel;

    [Header("Reload Popup")]
    public GameObject reloadPopup;
    public GameObject reloadDarkPanel;

    [Header("Coming Soon Popup")]
    public GameObject comingSoonPopup;
    public GameObject comingSoonDarkPanel;

    [Header("Xem quảng cáo")]
    public GameObject watchAdsPopup;
    public GameObject darkPanel;

    [Header("Cài đặt Shop")]
    public GameObject shopPopup;
    public GameObject backButton;
    public GameObject gotoShop;

    [Header("UI Hiển Thị Số Lượt Trợ Giúp")]
    public TextMeshProUGUI undoText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI addBottleText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateHelpUI(float undo, float hint, float addBottle)
    {
        if (undoText != null) undoText.text = undo.ToString();
        if (hintText != null) hintText.text = hint.ToString();
        if (addBottleText != null) addBottleText.text = addBottle.ToString();
    }

    public IEnumerator ShowDeadlockPopup()
    {
        yield return new WaitForSeconds(1.5f);

        if (outOfMovesPopup != null)
        {
            StartCoroutine(PopupCoroutine(dark_Panel, outOfMovesPopup));
        }
        if (currentLevelText != null)
        {
            currentLevelText.text = PlayerPrefs.GetInt("CurrentLevel", 1).ToString();
        }
    }

    public void ShowWinUI()
    {
        StartCoroutine(WinSequenceRoutine());
    }

    private IEnumerator WinSequenceRoutine()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayWinSound();
        if (blackOverlay != null) blackOverlay.SetActive(true);

        if (winUIPanel != null) winUIPanel.SetActive(true);
        if (winPanelRect != null) winPanelRect.localScale = Vector3.zero;

        if (nextLevelPopupRect != null)
        {
            nextLevelPopupRect.gameObject.SetActive(true);
            nextLevelPopupRect.localScale = Vector3.zero;
        }

        Vector3 topCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10f));
        if (confettiRainPrefab != null)
        {
            Instantiate(confettiRainPrefab, topCenter, confettiRainPrefab.transform.rotation);
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            if (winPanelRect != null) winPanelRect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeT);
            yield return null;
        }
        if (winPanelRect != null) winPanelRect.localScale = Vector3.one;

        yield return new WaitForSeconds(0.8f);

        elapsed = 0f;
        float outDuration = 0.3f;
        while (elapsed < outDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / outDuration;
            float easeT = t * t * (2.70158f * t - 1.70158f);
            if (winPanelRect != null) winPanelRect.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, easeT);
            yield return null;
        }
        if (winPanelRect != null) winPanelRect.gameObject.SetActive(false);

        if (nextLevelPopupRect != null)
        {
            elapsed = 0f;
            if (leftConfetti != null) leftConfetti.Play();
            if (rightConfetti != null) rightConfetti.Play();
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
                nextLevelPopupRect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeT);
                yield return null;
            }
            nextLevelPopupRect.localScale = Vector3.one;
        }

        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.AddScore(currentLevel);
        }
    }

    private IEnumerator PopupCoroutine(GameObject darkPanelObj, GameObject popup)
    {
        if (darkPanelObj != null) darkPanelObj.SetActive(true);
        if (gotoShop != null && shopPopup != null && shopPopup.activeSelf) gotoShop.SetActive(false);

        popup.SetActive(true);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.localScale = Vector3.zero;
        }

        float duration = 0.5f;
        float elapsed = 0f;
        
        bool hasMoves = true;
        if (GameManager.instance != null) hasMoves = GameManager.instance.HasAnyValidMove();

        if (!hasMoves && SceneManager.GetActiveScene().name == "MainPlayScene")
        {
            if (AudioManager.instance != null) AudioManager.instance.PlayGameOver();
        }
        else
        {
            if (AudioManager.instance != null) AudioManager.instance.PlayPopupSound();
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            if (popupRect != null) popupRect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeT);
            yield return null;
        }
        if (popupRect != null) popupRect.localScale = Vector3.one;
    }

    private IEnumerator ClosePopupCoroutine(GameObject panelToClose, GameObject popupToClose)
    {
        RectTransform popupRect = popupToClose.GetComponent<RectTransform>();
        if (popupRect == null) yield break;

        float duration = 0.3f;
        float elapsed = 0f;

        if (AudioManager.instance != null) AudioManager.instance.PlayPopupSound();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * t * (2.70158f * t - 1.70158f);
            popupRect.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, easeT);
            yield return null;
        }
        popupRect.localScale = Vector3.zero;
        popupToClose.SetActive(false);
        if (panelToClose != null) panelToClose.SetActive(false);
    }

    public void OnClickHome()
    {
        if (LoadingSceneSmooth.Instance != null)
            LoadingSceneSmooth.Instance.StartCoroutine(LoadingSceneSmooth.Instance.LoadSceneSmooth("MainScene"));
    }

    public void OnClickNextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (currentLevel >= 15)
        {
            StartCoroutine(ClosePopupCoroutine(blackOverlay, nextLevelPopup));
            StartCoroutine(PopupCoroutine(comingSoonDarkPanel, comingSoonPopup));
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickWatchAds()
    {
        StartCoroutine(PopupCoroutine(darkPanel, watchAdsPopup));
    }

    public void CloseWatchPopup()
    {
        StartCoroutine(ClosePopupCoroutine(darkPanel, watchAdsPopup));
        if (GameManager.instance != null) GameManager.instance.isLocked = false;
    }

    public void ClickOpenShop()
    {
        if (GameManager.instance != null && GameManager.instance.isLocked) return;
        
        if (watchAdsPopup != null) watchAdsPopup.SetActive(false);
        if (darkPanel != null) darkPanel.SetActive(false);
        
        StartCoroutine(PopupCoroutine(null, shopPopup));
        if (backButton != null) backButton.SetActive(true);
    }

    public void ClickCloseShop()
    {
        if (shopPopup != null) StartCoroutine(ClosePopupCoroutine(null, shopPopup));
    }

    public void OpenSettingPopup()
    {
        if (settingPopup != null) StartCoroutine(PopupCoroutine(settingDarkPanel, settingPopup));
    }

    public void CloseSettingPopup()
    {
        if (settingPopup != null) StartCoroutine(ClosePopupCoroutine(settingDarkPanel, settingPopup));
    }

    public void OpenReloadPopup()
    {
        if (reloadPopup != null) StartCoroutine(PopupCoroutine(reloadDarkPanel, reloadPopup));
    }

    public void CloseReloadPopup()
    {
        if (reloadPopup != null) StartCoroutine(ClosePopupCoroutine(reloadDarkPanel, reloadPopup));
    }

    public void CloseComingSoonPopup()
    {
        if (comingSoonPopup != null) StartCoroutine(ClosePopupCoroutine(comingSoonDarkPanel, comingSoonPopup));
    }

    public async void ClickTestRankButton()
    {
        if (LeaderboardManager.Instance != null)
        {
            await LeaderboardManager.Instance.AddScore(15);
            LeaderboardManager.Instance.FetchLeaderboard();
        }
    }
}
