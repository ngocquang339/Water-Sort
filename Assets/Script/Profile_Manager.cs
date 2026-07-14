using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Profile_Manager : MonoBehaviour
{
    public static Profile_Manager instance;
    public GameObject darkPanel;
    public GameObject profile_Panel;
    public TextMeshProUGUI coinAmount;
    public TextMeshProUGUI levelNumber;
    public TextMeshProUGUI player_Name;
    public GameObject rename_Area;
    public GameObject confirmButton;
	public RectTransform nameGroupRect;
	[SerializeField] private TMP_InputField usernameInputField;
	private const string USERNAME_KEY = "Player_Username";
	void Awake(){
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        UpdateUI();
    }

	public void OnClickProfile()
	{
		darkPanel.SetActive(true);
		profile_Panel.SetActive(true);

		// [THÊM ĐOẠN NÀY] Ép hệ thống tính toán lại co giãn ngay lập tức khi mở Popup
		UpdateUI();
		Canvas.ForceUpdateCanvases();
		if (nameGroupRect != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(nameGroupRect);
		}
	}

	public void CloseProfilePopup(){
        darkPanel.SetActive(false); 
        profile_Panel.SetActive(false);
        rename_Area.SetActive(false);
        confirmButton.SetActive(false);
	}

    private void UpdateUI(){
        float coin = PlayerPrefs.GetFloat("Player_Coin", 0);
        if (coin == 0) { 
            Debug.Log("No coin found, setting default value to 0.");
		}
		coinAmount.text = coin.ToString();
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        levelNumber.text = level.ToString();
        player_Name.text = PlayerPrefs.GetString("Player_Username", "Player");
	}

    public void SaveNewName(){
		TMP_Text placeholder = usernameInputField.placeholder as TMP_Text;
		string newUsername = usernameInputField.text.Trim();
        if (string.IsNullOrEmpty(newUsername)) {
			placeholder.text = "Tên không được để trống!";
			return;
        }
		PlayerPrefs.SetString(USERNAME_KEY, newUsername);
        CloseProfilePopup();
        UpdateUI();
	}

    public void ClickReNameButton(){
        player_Name.text = "";
        rename_Area.SetActive(true);
        confirmButton.SetActive(true);
	}
}
