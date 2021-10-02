using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    [Range(0f,1f)]
    public float AutoCollectPercentage = 0.1f;
    public float SaveDelay = 5f;

    public ResourceConfig[] ResourceConfigs;
    public Sprite[] ResourcesSprites;

    public Transform ResourcesParent;
    public ResourceController ResourcePrefab;
    public TapText TapTextPrefab;

    public Transform CoinIcon;
    public Text GoldInfo;
    public Text AutoCollectInfo;

    private List<ResourceController> _activeResources = new List<ResourceController>();
    private List<TapText> _tapTextPool = new List<TapText>();
    
    private float _collectSecond;
    private float _saveDelayCounter;


    // Start is called before the first frame update
    void Start()
    {
        AddAllResources();

        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0") }";

        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        _saveDelayCounter -= deltaTime;

        //fungsi untuk selalu mengeksekusi CollectPerSecond setiap detik
        _collectSecond += deltaTime;
        _collectSecond += Time.unscaledDeltaTime;

        if( _collectSecond >= 1f )
        {
            CollectPerSecond();
            _collectSecond = 0f;
        }

        CheckResourceCost();

        CoinIcon.transform.localScale = Vector3.LerpUnclamped(CoinIcon.transform.localScale, Vector3.one * 2f, 0.15f);
        CoinIcon.transform.Rotate (0f, 0f, Time.deltaTime * -100f);
            
    }

    

    private void AddAllResources()
    {
        bool showResources = true;
        int index = 0;
        
        foreach (ResourceConfig config in ResourceConfigs)
        {

            GameObject obj = Instantiate(ResourcePrefab.gameObject, ResourcesParent, false);
            ResourceController resource = obj.GetComponent<ResourceController>();

            resource.SetConfig(index, config);
            obj.gameObject.SetActive(showResources);

            if(showResources && !resource.isUnlocked)
            {
                showResources = false;
            }

            _activeResources.Add(resource);
            index++;
        }
    }

    public void ShowNextResource()
    {
        foreach(ResourceController resource in _activeResources)
        {
            if(!resource.gameObject.activeSelf)
            {
                resource.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void CheckResourceCost()
    {
        foreach(ResourceController resource in _activeResources)
        {
            bool isBuyable = false;
            if(resource.isUnlocked)
            {
                isBuyable = UserDataManager.Progress.Gold >= resource.GetUpgradeCost();
            }
            else
            {
                isBuyable = UserDataManager.Progress.Gold >= resource.GetUnlockCost();
            }

            resource.ResourceImage.sprite = ResourcesSprites[isBuyable ? 1 : 0];
        }
    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach(ResourceController resource in _activeResources)
        {
            if(resource.isUnlocked)
            {
                output += resource.GetOutput();
            }
            
        }

        output *= AutoCollectPercentage;

        AutoCollectInfo.text = $"Auto Collect: { output.ToString("F1") }/ second";

        AddGold(output);
    }

    public void AddGold(double value)
    {
        UserDataManager.Progress.Gold += value;
        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0")}";

        AchievementController.Instance.GoldAchievement(AchievementType.GoldReach, UserDataManager.Progress.Gold);

        UserDataManager.Save(_saveDelayCounter < 0f);

        if(_saveDelayCounter < 0f)
        {
            _saveDelayCounter = SaveDelay;
        }
    }

    public void CollectByTap (Vector3 tapPosition, Transform parent)
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if(resource.isUnlocked)
            {
                output += resource.GetOutput();
            }
            
        }

        TapText tapText = GetOrCreateTapText();
        tapText.transform.SetParent(parent, false);
        tapText.transform.position = tapPosition;

        tapText.Text.text = $"+ { output.ToString("0") }";
        tapText.gameObject.SetActive(true);
        CoinIcon.transform.localScale = Vector3.one * 1.75f;

        AddGold(output);
    }

    private TapText GetOrCreateTapText()
    {
        TapText tapText = _tapTextPool.Find (t => !t.gameObject.activeSelf);
        if(tapText == null)
        {
            tapText = Instantiate(TapTextPrefab).GetComponent<TapText>();
            _tapTextPool.Add(tapText);
        }
        return tapText;
    }

}//class

[System.Serializable]
    public struct ResourceConfig
    {
        public string Name;
        public double UnlockCost;
        public double UpgradeCost;
        public double Output;
    }
