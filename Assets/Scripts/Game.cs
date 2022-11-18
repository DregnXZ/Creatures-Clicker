using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using System.Linq;
using System.Data;
using System.IO;

public class Game : MonoBehaviour
{
    [SerializeField] int EnergyScore;

    private int ClickEnergyScore = 1;
    private int EnergyIncome = 0;

    private int ClickEgg = 1;

    [SerializeField] private int MoneyScore;

    private int ClickMoneyScore = 1;
    private int MoneyIncome = 0;

    public int[] UpgradeCost;

    public GameObject LocationsPanel;
    public GameObject LibraryPanel;
    public GameObject SummonPanel;
    public GameObject CavePanel;
    public GameObject TemplePanel;
    public GameObject StoragePanel;
    public GameObject UpgradePanel;
    public GameObject CollectionPanel;
    public GameObject ProgressGridPanel;
    public GameObject AchivementsPanel;

    public Sprite[] EggsImages;
    public Sprite[] ChickensSprites;

    public Image CurrentEggImage;
    public Image[] ChickensImages;

    public Text EnergyScoreText;
    public Text EggDurabilityText;
    public Text[] EggsCountText;
    public Text MoneyScoreText;
    public Text[] ChickensDescription;

    private Egg CurrentEgg;

    private List<List<Egg>> MyEggs = new List<List<Egg>>();

    private List<List<Creature>> MyCreatures = new List<List<Creature>>();

    private List<Egg> Eggs = new List<Egg>();
    private List<Creature> Creatures = new List<Creature>();

    private Save sv = new Save();

    private delegate void SetCreaturePictures();

    private int timer = 1;
    
    public void Awake()
    {
        while (timer == 1)
        {
            Eggs.Add(new Egg("DefaultEgg", "Common", 85, 100));
            Eggs.Add(new Egg("FireEgg", "Uncommon", 10, 300));
            Eggs.Add(new Egg("DarkEgg", "Epic", 4, 800));
            Eggs.Add(new Egg("StarEgg", "Legendary", 1, 2000));

            Creatures.Add(new Creature("Chicken", "Common"));
            Creatures.Add(new Creature("FireChicken", "Uncommon"));
            Creatures.Add(new Creature("DarkChicken", "Epic"));
            Creatures.Add(new Creature("StarChicken", "Legendary"));

            timer--;
        }

        if (PlayerPrefs.HasKey("SV"))
        {
            sv = JsonUtility.FromJson<Save>(PlayerPrefs.GetString("SV"));

            EnergyScore = sv.EnergyScore;
            ClickEnergyScore = sv.ClickEnergyScore;
            EnergyIncome = sv.EnergyIncome;
            MoneyScore = sv.MoneyScore;
            ClickMoneyScore = sv.ClickMoneyScore;
            MoneyIncome = sv.MoneyIncome;
            ClickEgg = sv.ClickEgg;

            for (int i = 0; i < 6; i++)
            {
                UpgradeCost[i] = sv.UpgradeCost[i];
            }

            while (MyEggs.Count < sv.EggCount.Length)
            {
                MyEggs.Add(new List<Egg>());
            }
            while (MyCreatures.Count < sv.Creatures.Length)
            {
                MyCreatures.Add(new List<Creature>());
            }

            for (int i = 0; i < MyEggs.Count; i++)
            {
                while (MyEggs[i].Count < sv.EggCount[i])
                {
                    MyEggs[i].Add(Eggs[i].Clone());
                }
                if (MyEggs[i].Count > 0)
                {
                    MyEggs[i][0].EggDurability = sv.EggDurability[i];
                }
            }

            for (int i = 0; i < MyCreatures.Count; i++)
            {
                if (sv.Creatures[i])
                {
                    MyCreatures[i].Add(Creatures[i].Clone());
                }
            }
        }
    }

    public void Start()
    {
        while (MyEggs.Count < 4)
        {
            MyEggs.Add(new List<Egg>());
        }
        while (MyCreatures.Count < 4)
        {
            MyCreatures.Add(new List<Creature>());
        }

        StartCoroutine(EnergyIncomeTimer());
        StartCoroutine(MoneyIncomeTimer());

        DateTime dt = new DateTime(sv.Date[0], sv.Date[1], sv.Date[2], sv.Date[3], sv.Date[4], sv.Date[5]);
        TimeSpan ts = DateTime.Now - dt;
        EnergyScore += (int)ts.TotalSeconds * EnergyIncome;
        MoneyScore += ((int)ts.TotalSeconds / 60) * MoneyIncome;
    }

    public void OnClickEnergyButton()
    {
        EnergyScore += ClickEnergyScore;
    }

    public void OnClickMoneyButton()
    {
        MoneyScore += ClickMoneyScore;
    }

    public void OnClickEggButton()
    {
        if (CurrentEgg != null)
        {
            CurrentEgg.EggDurability -= ClickEgg;

            if (CurrentEgg.EggDurability < 1)
            {
                EggDestroy();

                if (CurrentEgg == null)
                {
                    EggDurabilityText.text = "Выберите яйцо";
                }
            }
        } 
    }

    public void ChooseDefaultEgg()
    {
        if (MyEggs[0].Count > 0)
        {
            CurrentEgg = MyEggs[0][0];
        }
    }

    public void ChooseFireEgg()
    {
        if (MyEggs[1].Count > 0)
        {
            CurrentEgg = MyEggs[1][0];
        }
    }
    public void ChooseDarkEgg()
    {
        if (MyEggs[2].Count > 0)
        {
            CurrentEgg = MyEggs[2][0];
        }
    }
    public void ChooseStarEgg()
    {
        if (MyEggs[3].Count > 0)
        {
            CurrentEgg = MyEggs[3][0];
        }
    }
    
    public void SummonEgg()
    {
        SummonRandomEggLevel1(Eggs);
    }

    public void SummonRandomEggLevel1(List<Egg> eggs)
    {
        int i;
        List<Egg> ItemsInBag = new List<Egg>();

        foreach (var egg in eggs)
        {
            i = egg.IntEggRarity;
            while (i > 0)
            {
                ItemsInBag.Add(egg);
                i--;
            }
        }

        Egg NewEgg = ItemsInBag[new System.Random().Next(ItemsInBag.Count)];

        if (EnergyScore >= 300)
        {
            EnergyScore -= 300;
            
            if (NewEgg.EggName == "DefaultEgg")
            {
                MyEggs[0].Add(Eggs[0].Clone());
            }

            if (NewEgg.EggName == "FireEgg")
            {
                MyEggs[1].Add(Eggs[1].Clone());
            }

            if (NewEgg.EggName == "DarkEgg")
            {
                MyEggs[2].Add(Eggs[2].Clone());
            }

            if (NewEgg.EggName == "StarEgg")
            {
                MyEggs[3].Add(Eggs[3].Clone());
            }
        }
    }

    public void EggDestroy()
    {
        if (MyEggs[0].Count > 0)
        {
            if (CurrentEgg == MyEggs[0][0] && MyCreatures[0].Count == 0)
            {
                MyCreatures[0].Add(Creatures[0].Clone());
                MyEggs[0].RemoveAt(0);

                if (MyEggs[0].Count > 0)
                {
                    CurrentEgg = MyEggs[0][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
            else if (CurrentEgg == MyEggs[0][0] && MyCreatures[0].Count > 0)
            {
                MoneyScore += 200;
                MyEggs[0].RemoveAt(0);

                if (MyEggs[0].Count > 0)
                {
                    CurrentEgg = MyEggs[0][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
        }
        
        if (MyEggs[1].Count > 0)
        {
            if (CurrentEgg == MyEggs[1][0] && MyCreatures[1].Count == 0)
            {
                MyCreatures[1].Add(Creatures[1].Clone());
                MyEggs[1].RemoveAt(0);

                if (MyEggs[1].Count > 0)
                {
                    CurrentEgg = MyEggs[1][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
            else if (CurrentEgg == MyEggs[1][0] && MyCreatures[1].Count > 0)
            {
                MoneyScore += 500;
                MyEggs[1].RemoveAt(0);

                if (MyEggs[1].Count > 0)
                {
                    CurrentEgg = MyEggs[1][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
        }
        
        if (MyEggs[2].Count > 0)
        {
            if (CurrentEgg == MyEggs[2][0] && MyCreatures[2].Count == 0)
            {
                MyCreatures[2].Add(Creatures[2].Clone());
                MyEggs[2].RemoveAt(0);

                if (MyEggs[2].Count > 0)
                {
                    CurrentEgg = MyEggs[2][0];
                }
                else
                {
                    CurrentEgg = null;
                }

            }
            else if (CurrentEgg == MyEggs[2][0] && MyCreatures[2].Count > 0)
            {
                MoneyScore += 1200;
                MyEggs[2].RemoveAt(0);

                if (MyEggs[2].Count > 0)
                {
                    CurrentEgg = MyEggs[2][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
        }
        
        if (MyEggs[3].Count > 0)
        {
            if (CurrentEgg == MyEggs[3][0] && MyCreatures[3].Count == 0)
            {
                MyCreatures[3].Add(Creatures[3].Clone());
                MyEggs[3].RemoveAt(0);

                if (MyEggs[3].Count > 0)
                {
                    CurrentEgg = MyEggs[3][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
            else if (CurrentEgg == MyEggs[3][0] && MyCreatures[3].Count > 0)
            {
                MoneyScore += 3000;
                MyEggs[3].RemoveAt(0);

                if (MyEggs[3].Count > 0)
                {
                    CurrentEgg = MyEggs[3][0];
                }
                else
                {
                    CurrentEgg = null;
                }
            }
        }
    }

    private void SetDefaultChicken()
    {
        ChickensImages[0].GetComponent<Image>().sprite = ChickensSprites[0];
        ChickensDescription[0].text = "Название: Курица\r\nРедкость: Обычная\r\nОписание: Обычная курица";
        ChickensDescription[0].fontSize = 45;
        ChickensDescription[0].fontStyle = FontStyle.Italic;
        ChickensDescription[0].lineSpacing = 1.1f;
        ChickensDescription[0].alignment = TextAnchor.UpperLeft;
    }

    private void SetFireChicken()
    {
        ChickensImages[1].GetComponent<Image>().sprite = ChickensSprites[1];
        ChickensDescription[1].text = "Название: Огненная курица\r\nРедкость: Необычная\r\nОписание: Обитает рядом с вулканами";
        ChickensDescription[1].fontSize = 45;
        ChickensDescription[1].fontStyle = FontStyle.Italic;
        ChickensDescription[1].lineSpacing = 1.1f;
        ChickensDescription[1].alignment = TextAnchor.UpperLeft;
    }

    private void SetDarkChicken()
    {
        ChickensImages[2].GetComponent<Image>().sprite = ChickensSprites[2];
        ChickensDescription[2].text = "Название: Тёмная курица\r\nРедкость: Эпическая\r\nОписание: Создание ночи";
        ChickensDescription[2].fontSize = 45;
        ChickensDescription[2].fontStyle = FontStyle.Italic;
        ChickensDescription[2].lineSpacing = 1.1f;
        ChickensDescription[2].alignment = TextAnchor.UpperLeft;
    }

    private void SetStarChicken()
    {
        ChickensImages[3].GetComponent<Image>().sprite = ChickensSprites[3];
        ChickensDescription[3].text = "Название: Звёздная курица\r\nРедкость: Легендарная\r\nОписание: Бороздит просторы космоса";
        ChickensDescription[3].fontSize = 45;
        ChickensDescription[3].fontStyle = FontStyle.Italic;
        ChickensDescription[3].lineSpacing = 1.1f;
        ChickensDescription[3].alignment = TextAnchor.UpperLeft;
    }

    public void SetDefaultEggSprite()
    {
        if (CurrentEgg != null && MyEggs[0].Count > 0)
        {
            if (CurrentEgg == MyEggs[0][0])
            {
                CurrentEggImage.GetComponent<Image>().sprite = EggsImages[0];
            }
        }
    }
    public void SetFireEggSprite()
    {
        if (CurrentEgg != null && MyEggs[1].Count > 0)
        {
            if (CurrentEgg == MyEggs[1][0])
            {
                CurrentEggImage.GetComponent<Image>().sprite = EggsImages[1];
            }
        }
    }
    public void SetDerkEggSprite()
    {
        if (CurrentEgg != null && MyEggs[2].Count > 0)
        {
            if (CurrentEgg == MyEggs[2][0])
            {
                CurrentEggImage.GetComponent<Image>().sprite = EggsImages[2];
            }
        }
    }
    public void SetStarEggSprite()
    {
        if (CurrentEgg != null && MyEggs[3].Count > 0)
        {
            if (CurrentEgg == MyEggs[3][0])
            {
                CurrentEggImage.GetComponent<Image>().sprite = EggsImages[3];
            }
        }
    }

    private void Update()
    {
        SetCreaturePictures[] setCreaturePictures = new SetCreaturePictures[Creatures.Count];
        setCreaturePictures[0] = SetDefaultChicken;
        setCreaturePictures[1] = SetFireChicken;
        setCreaturePictures[2] = SetDarkChicken;
        setCreaturePictures[3] = SetStarChicken;
        
        EnergyScoreText.text = EnergyScore.ToString();
        MoneyScoreText.text = MoneyScore.ToString();

        try
        {
            EggDurabilityText.text = CurrentEgg.EggDurability.ToString();
        }
        catch
        {

        }

        for (int i = 0; i < EggsCountText.Length; i++)
        {
            EggsCountText[i].text = "Количество " + MyEggs[i].Count.ToString();
        }
        
        for (int i = 0; i < Creatures.Count; i++)
        {
            if (MyCreatures[i].Count > 0)
            {
                setCreaturePictures[i]();
            }
        }
        
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    IEnumerator EnergyIncomeTimer()
    {
        while(true)
        {
            EnergyScore += EnergyIncome;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator MoneyIncomeTimer()
    {
        while(true)
        {
            MoneyScore += MoneyIncome;
            yield return new WaitForSeconds(60);
        }
    }

    public void UpgradeEnergyClick()
    {
        if (MoneyScore >= UpgradeCost[0])
        {
            MoneyScore -= UpgradeCost[0];
            UpgradeCost[0] *= 2;
            ClickEnergyScore += 1;
        }
    }

    public void UpgradeEggClick()
    {
        if (MoneyScore >= UpgradeCost[1])
        {
            MoneyScore -= UpgradeCost[1];
            UpgradeCost[1] *= 2;
            ClickEgg += 1;
        }
    }

    public void UpgradeEnergyIncome()
    {
        if (MoneyScore >= UpgradeCost[2])
        {
            MoneyScore -= UpgradeCost[2];
            UpgradeCost[2] *= 2;
            EnergyIncome += 1;
        }
    }

    public void UpgradeMoneyIncome()
    {
        if (MoneyScore >= UpgradeCost[3])
        {
            MoneyScore -= UpgradeCost[3];
            UpgradeCost[3] *= 2;
            MoneyIncome += 1;
        }
    }

    public void ShowLibraryPanel()
    {
        LibraryPanel.SetActive(true);
        SummonPanel.SetActive(false);
        CavePanel.SetActive(false);
        TemplePanel.SetActive(false);
    }

    public void ShowSummonPanel()
    {
        LibraryPanel.SetActive(false);
        SummonPanel.SetActive(true);
        CavePanel.SetActive(false);
        TemplePanel.SetActive(false);
    }

    public void ShowCavePanel()
    {
        LibraryPanel.SetActive(false);
        SummonPanel.SetActive(false);
        CavePanel.SetActive(true);
        TemplePanel.SetActive(false);
    }

    public void ShowTemplePanel()
    {
        LibraryPanel.SetActive(false);
        SummonPanel.SetActive(false);
        CavePanel.SetActive(false);
        TemplePanel.SetActive(true);
    }

    public void ShowAndHideStoragePanel()
    {
        LocationsPanel.SetActive(!LocationsPanel.activeSelf);
        ProgressGridPanel.SetActive(!ProgressGridPanel.activeSelf);
        StoragePanel.SetActive(!StoragePanel.activeSelf);
    }

    public void ShowAndHideUpgragePanel()
    {
        LocationsPanel.SetActive(!LocationsPanel.activeSelf);
        ProgressGridPanel.SetActive(!ProgressGridPanel.activeSelf);
        UpgradePanel.SetActive(!UpgradePanel.activeSelf);
    }
    public void ShowAndHideCollectionPanel()
    {
        LocationsPanel.SetActive(!LocationsPanel.activeSelf);
        ProgressGridPanel.SetActive(!ProgressGridPanel.activeSelf);
        CollectionPanel.SetActive(!CollectionPanel.activeSelf);
    }

    public void ShowAndHideAchivementsPanel()
    {
        LocationsPanel.SetActive(!LocationsPanel.activeSelf);
        ProgressGridPanel.SetActive(!ProgressGridPanel.activeSelf);
        AchivementsPanel.SetActive(!AchivementsPanel.activeSelf);
    }

    private void OnApplicationQuit()
    {
        sv.EnergyScore = EnergyScore;
        sv.ClickEnergyScore = ClickEnergyScore;
        sv.EnergyIncome = EnergyIncome;
        sv.MoneyScore = MoneyScore;
        sv.ClickMoneyScore = ClickMoneyScore;
        sv.MoneyIncome = MoneyIncome;
        sv.ClickEgg = ClickEgg;
        sv.UpgradeCost = new int[6];
        sv.EggCount = new int[MyEggs.Count];
        sv.EggDurability = new int [MyEggs.Count];
        sv.Creatures = new bool[MyCreatures.Count];

        for (int i = 0; i < 6; i++)
        {
            sv.UpgradeCost[i] = UpgradeCost[i];
        }

        for (int i = 0; i < MyEggs.Count; i++)
        {
            sv.EggCount[i] = MyEggs[i].Count;
            if (MyEggs[i].Count > 0)
            {
                sv.EggDurability[i] = MyEggs[i][0].EggDurability;
            }
        }

        for (int i = 0; i < MyCreatures.Count; i++)
        {
            if (MyCreatures[i].Count > 0)
            {
                sv.Creatures[i] = true;
            }
            else
            {
                sv.Creatures[i] = false;
            }
        }

        sv.Date[0] = DateTime.Now.Year;
        sv.Date[1] = DateTime.Now.Month;
        sv.Date[2] = DateTime.Now.Day;
        sv.Date[3] = DateTime.Now.Hour;
        sv.Date[4] = DateTime.Now.Minute;
        sv.Date[5] = DateTime.Now.Second;

        PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
    }
}

public class Egg
{
    public string EggName;
    public string EggRarity;
    public int IntEggRarity;

    public int EggDurability;

    public string[] Creatures;

    public Egg(string name, string rarity, int intRarity, int durability)
    {
        EggName = name;
        EggRarity = rarity;
        IntEggRarity = intRarity;
        EggDurability = durability;
    }

    public Egg Clone()
    {
        Egg clone = new Egg(EggName, EggRarity, IntEggRarity, EggDurability);
        return clone;
    }

    public static List<Egg> CloneList(List<Egg> EggList)
    {
        List<Egg> list = new List<Egg>(EggList.Count);
        foreach (var item in EggList)
        {
            list.Add(item.Clone());
        }
        return list;
    }
}

public class Creature
{
    public string CreatureName;
    public string CreatureRarity;

    public Creature(string creatureName, string creatureRarity)
    {
        CreatureName = creatureName;
        CreatureRarity = creatureRarity;
    }

    public Creature Clone()
    {
        Creature clone = new Creature(CreatureName, CreatureRarity);
        return clone;
    }
}

[Serializable]

public class Save
{
    public int EnergyScore;
    public int ClickEnergyScore;
    public int EnergyIncome;
    public int MoneyScore;
    public int ClickMoneyScore;
    public int MoneyIncome;
    public int ClickEgg;
    public int[] UpgradeCost;
    public int[] Date = new int[6];
    public int[] EggCount;
    public int[] EggDurability;
    public bool[] Creatures;
}