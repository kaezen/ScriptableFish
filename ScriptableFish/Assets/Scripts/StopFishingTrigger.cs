using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopFishingTrigger : SetterClass
{
    public override void TriggerEvent()
    {
        FishingEventsController.current.StopFishing();
    }
}
