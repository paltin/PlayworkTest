using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class DisplayManager : MonoBehaviour
{
    public Text clickCounterText;

    ReactiveProperty<int> testCounter = new ReactiveProperty<int>();
    
    void Start()
    {
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .ThrottleFirst(System.TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => testCounter.Value++);

        testCounter.SubscribeToText(clickCounterText, i => $"Click:{i}");
    }

    
}
