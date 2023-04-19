using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockAnimator : MonoBehaviour
{
    [SerializeField] public Transform hours, minutes, seconds;

    void Update()
    {
        float hourAngle = DateTime.Now.Hour * (360 / 12);
        float minuteAngle = DateTime.Now.Minute * (360 / 60);
        float secondAngle = DateTime.Now.Second * (360 / 60);

        hours.transform.localRotation = Quaternion.Euler(90 + hourAngle, 0, -90);
        minutes.transform.localRotation = Quaternion.Euler(90 + minuteAngle, 0, -90);
        seconds.transform.localRotation = Quaternion.Euler(90 + secondAngle, 0, -90);
    }
}
