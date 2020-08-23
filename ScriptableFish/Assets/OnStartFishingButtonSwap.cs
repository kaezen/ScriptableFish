using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnStartFishingButtonSwap : MonoBehaviour
{
    public GameObject OtherButton;
    private void Awake()
    {
        FishingEventsController.current.onStartFishing += SwapActive;
    }

    private void SwapActive()
    {
        OtherButton.SetActive(true);
        gameObject.SetActive(false);
    }
}
