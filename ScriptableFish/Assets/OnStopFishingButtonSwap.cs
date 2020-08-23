using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnStopFishingButtonSwap : MonoBehaviour
{
    public GameObject OtherButton;
    private void Awake()
    {
        FishingEventsController.current.onStopFishing += SwapActive;
    }

    private void SwapActive()
    {
        OtherButton.SetActive(true);
        gameObject.SetActive(false);
    }
}