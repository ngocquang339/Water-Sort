using UnityEngine;

public class GameWinManager : MonoBehaviour
{
	public static GameWinManager instance;
	private CurrencyManager currencyManager;
	private void Awake()
	{
		if(instance == null){
			instance = this;
		}
		else{
			Destroy(gameObject);
		}
	}

}
