﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMonoBehaviour : MonoBehaviour {

    protected IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}
