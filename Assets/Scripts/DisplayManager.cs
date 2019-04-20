using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DisplayManager : MonoBehaviour
{    
    public static DisplayManager Instance { get; private set; }
    void OnEnable() { Instance = this; }

    public Text idxText;
    public Slider slider;
    public Button playButton;
    public Button stopButton;

    void Awake() {
        playButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        idxText.gameObject.SetActive(false);
    }

    public void Init(int totalSamples) {
        playButton.gameObject.SetActive(true);
        GameManager.Instance.AddListenerToCounter(OnCounterChange);
        GameManager.Instance.AddListenerToPlayButton(OnPlayButtonPressed);
        GameManager.Instance.AddListenerToSimulationStateChange(OnSimulationStateChange);
        slider.maxValue = totalSamples-1;
    }

    void OnCounterChange(int value) {
        idxText.text = value.ToString();
        slider.value = value;        
    }

    void OnPlayButtonPressed(bool isPaused) {
        playButton.GetComponentInChildren<Text>().text = isPaused ? "Resume" : "Pause";
    }

    void OnSimulationStateChange(bool started) {
        stopButton.gameObject.SetActive(started);
        slider.gameObject.SetActive(started);
        idxText.gameObject.SetActive(started);
        if(!started)
            playButton.GetComponentInChildren<Text>().text = "Start";
    }

    
    
    
}
