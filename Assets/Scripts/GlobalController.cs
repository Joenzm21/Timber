using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GlobalController : MonoBehaviour
{
    AudioSource aso;
    Quaternion defaultqua = new Quaternion(0, 0, 0, 0);
    Animator amt;
    bool doing;
    GameObject ctrunk;
    List<GameObject> trunks = new List<GameObject>();
    int playerdirection = 1;
    float cspeed = 1;
    GameObject ripobj = null;
    long scores = 0;
    int state = 0;

    float accl = 0.00091f;
    float increasev = 10f;
    float decreasev = 30f;
    float chance = 2;
    float trunkoffset = 2.49f;
    float maxspeed = 5f;
    int basev = 55;
    float bonusratio = 0.3f;
    float basebonus = 0.2f;
    float anispeed = 0.15f;

    public FPSDisplay fPSDisplay;
    public Canvas canvas;
    public GameObject Seff;
    public GameObject GOEf2;
    public Text cScores;
    public GameObject ripPreB;
    public Slider healthbar;
    public GameObject left, middle, right;
    public Vector3 basepos, lastpos;

    void Start()
    {
        try
        {
            if (File.Exists("config.ini"))
            {
                ConfigFile cfg = JsonUtility.FromJson<ConfigFile>(File.ReadAllText("config.ini"));
                if (cfg != null)
                {
                    maxspeed = cfg.maximumspeed;
                    accl = cfg.acceleration;
                    increasev = cfg.increasedvalue;
                    decreasev = cfg.decreasedvalue;
                    basev = cfg.basevalue;
                    basebonus = cfg.basebonus;
                    bonusratio = cfg.bonusratio;
                    anispeed = cfg.animationspeed;
                }
            }
        }
        catch { }
        GOEf2.SetActive(false);
        state = 1;
    }

    void StartGame()
    {
        GOEf2.SetActive(false);
        foreach (GameObject obj in trunks)
            Destroy(obj);
        scores = 0;
        trunks.Clear();
        playerdirection = 1;
        cScores.text = "Scores: 0";
        if (ripobj != null)
            Destroy(ripobj);
        transform.position = new Vector3(1.75f, -1.9f, -1f);
        transform.rotation = defaultqua;
        cspeed = 1;
        healthbar.value = basev;
        amt = GetComponent<Animator>();
        aso = GetComponent<AudioSource>();
        GameObject ctrunk = Instantiate(middle, basepos, defaultqua);
        trunks.Add(ctrunk);
        lastpos = basepos;
        for (int i = 0; i < 4; i++)
        {
            lastpos.y += trunkoffset;
            CreateTrunk(lastpos);
        }
    }

    void CreateTrunk(Vector3 pos)
    {
        if (UnityEngine.Random.Range(0, chance) > 1)
        {
            ctrunk = Instantiate(middle, pos, defaultqua);
            ctrunk.AddComponent<TrunkController>().TrunkType = 0;
        }
        else if (UnityEngine.Random.Range(0, chance) < 1)
        {
            ctrunk = Instantiate(left, pos, defaultqua);
            ctrunk.GetComponent<TrunkController>().TrunkType = -1;
        }
        else
        {
            ctrunk = Instantiate(right, pos, defaultqua);
            ctrunk.GetComponent<TrunkController>().TrunkType = 1;
        }
        trunks.Add(ctrunk);
    }

    void GameOver()
    {
        state = 3;
        ripobj = Instantiate(ripPreB, new Vector3(1.9f * playerdirection, -2.1f, 0), defaultqua);
        transform.position = new Vector3(1.75f, -10, -1f);
        GOEf2.SetActive(true);
        GOEf2.GetComponent<Animator>().Play("Base Layer.ShowText");
    }

    void MoveTrunks()
    {
        if (trunks.Count > 0 && trunks[0].transform.position.y != basepos.y)
        {
            float step = Mathf.Min(trunkoffset * Mathf.Max(anispeed, 1 / (fPSDisplay.FPS * 0.12f)), trunks[0].transform.position.y - basepos.y);
            foreach (GameObject obj in trunks)
                obj.transform.position = new Vector3(0, obj.transform.position.y - step, 0);
        }
    }

    void Update()
    {
        MoveTrunks();
        switch (state)
        {
            case 1:
                if (Input.GetKeyDown("space"))
                {
                    StartGame();
                    Seff.SetActive(false);
                    state = 2;
                }
                break;
            case 2:
                if (scores > 0)
                {
                    if (cspeed < maxspeed)
                        cspeed *= (1 + accl);
                    healthbar.value = Mathf.Max(healthbar.value - decreasev * Time.deltaTime * cspeed, 7);
                }
                if (healthbar.value == 7)
                    GameOver();
                if (Input.GetKeyDown("q") && !doing)
                {
                    if (playerdirection == 1) Change();
                    Cutdown();
                }
                if (Input.GetKeyDown("e") && !doing)
                {
                    if (playerdirection == -1) Change();
                    Cutdown();
                }
                break;
            case 3:
                if (Input.GetKeyDown("space"))
                {
                    StartGame();
                    Seff.SetActive(false);
                    state = 2;
                }
                break;
        }       
    }
    void Change()
    {
        Vector3 v = transform.position;
        v.x *= -1;
        transform.position = v;
        playerdirection *= -1;
        transform.Rotate(transform.up, 180);
    }
    void Cutdown()
    {
        doing = true;
        amt.enabled = true;
        amt.Play("Base Layer.Cutdown", 0, 0.12f);
        aso.Play();
        StartCoroutine(Wait());
        if (trunks[1].GetComponent<TrunkController>().TrunkType == playerdirection)
        {
            Destroy(trunks[0]);
            GameOver();
        }
        else
        {
            if (trunks[1].GetComponent<TrunkController>().TrunkType == 0)
                scores += 5;
            else scores += 5 + (long)Mathf.Round(5 * (cspeed - 1 + basebonus) * bonusratio);
            cScores.text = "Scores: " + scores;
            trunks[0].GetComponent<TrunkController>().FlyOut(playerdirection);
            healthbar.value = Mathf.Min(healthbar.value + increasev * cspeed, 100);
        }
        CreateTrunk(new Vector3(0, trunks[trunks.Count - 1].transform.position.y + trunkoffset, 0));
        trunks.RemoveAt(0);
        Destroy(trunks[0]);
        trunks[0] = Instantiate(middle, new Vector3(0, basepos.y + trunkoffset, 0), defaultqua);
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.12f);
        doing = false;
    }
}
[Serializable]
public class ConfigFile
{
    public float basebonus = 0.2f;
    public float bonusratio = 0.3f;
    public int basevalue = 35;
    public float acceleration = 0.0003f;
    public float maximumspeed = 5f;
    public float increasedvalue = 10f;
    public float decreasedvalue = 15f;
    public float animationspeed = 0.15f;
}