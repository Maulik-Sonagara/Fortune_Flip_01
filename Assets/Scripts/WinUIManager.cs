using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinUIManager : MonoBehaviour
{

    public static WinUIManager Instance;


    public GameObject goodLuckImage;
    public GameObject winDataImage;

    public TextMeshProUGUI rewardAmountText;
    public TextMeshProUGUI rewardText;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        setGoodLuck();
    }

    public void setGoodLuck()
    {
        rewardAmountText.text = "";
        rewardText.text = "";
        winDataImage.SetActive(false);
        goodLuckImage.SetActive(true);
    }

    public void offGoodLuck()
    {
        winDataImage.SetActive(true);
        goodLuckImage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
