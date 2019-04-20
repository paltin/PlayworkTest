using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class GameManager : MonoBehaviour{
    public string inputDataFileName;    
    public Transform ball;
    public float timeStep = 0.05f;
    
    public bool isPlaying {get; private set;} 
    public bool isPaused{get; private set;}    

    List<Quaternion> inputs;    
    float ballRadius = -1f;        
    Coroutine timerCoroutine;
    Vector3 ballInitialPos;

    List<UnityAction<int>> counterListeners = new List<UnityAction<int>>();
    List<UnityAction<bool>> playButtonListeners = new List<UnityAction<bool>>();
    List<UnityAction<bool>> simulattionStateListeners = new List<UnityAction<bool>>();

    ReactiveProperty<int> counter;

    public static GameManager Instance { get; private set; }
    void OnEnable() { Instance = this; }

    public void AddListenerToCounter(UnityAction<int> f) {
        counterListeners.Add(f);
    }

    public void AddListenerToPlayButton(UnityAction<bool> f) {
        playButtonListeners.Add(f);
    }

    public void AddListenerToSimulationStateChange(UnityAction<bool> f) {
        simulattionStateListeners.Add(f);
    }

    void Start(){
        inputs = new List<Quaternion>();
        LoadData();
        if(ball != null){
            ballRadius = ball.GetComponent<Renderer>().bounds.extents.x;  
            ballInitialPos = ball.transform.position;
        } 
    }

    void InitCounter() {
        counter = new ReactiveProperty<int>();        
        counter.Subscribe( i => OnCounterChange(i));                        
    }
        
    void OnCounterChange(int value) {
        ApplyInputData(value);        
        foreach(var f in counterListeners)
            f?.Invoke(value);
    }

    public void OnPlayButton() {                
        if(isPlaying)
            isPaused = !isPaused;
        else
            StartSimulation();
        foreach(var f in playButtonListeners)
            f?.Invoke(isPaused);            
    }

    public void OnStopButton() {
        isPlaying = false;
        isPaused = false;
        counter.Value = 0;
        ResetSimulation();        
        foreach(var f in simulattionStateListeners)
            f?.Invoke(false);            
    }

    void LoadData() {
        StartCoroutine(GetComponent<WWWHelper>().LoadTextFileFromStreamingAssets(inputDataFileName, OnDataLoaded));
    }

    void OnDataLoaded(string data) {         
        string[] lines = data.Split("\n"[0]);

        for(int i = 0; i < lines.Length; i++) {            
            string[] lineData = (lines[i].Trim()).Split(","[0]);
            float[] q = new float[4];
            
            if(lineData.Length == 4){
                bool inputIsValid = true;
                for(int j=0; j<4; j++) {                    
                    inputIsValid = inputIsValid && float.TryParse(lineData[j], out q[j]);                    
                    if(!inputIsValid)
                        break;
                }            
                if(inputIsValid)
                    inputs.Add(new Quaternion(q[0], q[1], q[2], q[3]));
            }
        }

        DisplayManager.Instance.Init(inputs.Count);
    }

    void ResetSimulation() {
        if(timerCoroutine != null)
            StopCoroutine(timerCoroutine);        
        InitCounter();
        ResetTransformations();        
    }

    void StartSimulation() {
        ResetSimulation();
        if(ballRadius > 0f && inputs.Count > 0) {            
            isPlaying = true;
            isPaused = false;
            foreach(var f in simulattionStateListeners)
                f?.Invoke(true);   
            timerCoroutine = StartCoroutine(SimulationTimer());                    
        } else
            Debug.LogError("Can't simulate, radius has negative value");
    }
    
    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)) 
            Application.Quit(); 
    }

    IEnumerator SimulationTimer() {
        
        while(counter.Value < inputs.Count-1) {
            if(!isPaused)
                counter.Value++;            
            yield return new WaitForSeconds(timeStep);            
        }             
        counter.Dispose();
        OnStopButton();
    }

    void ResetTransformations() {        
        ball.rotation = Quaternion.identity;
        ball.position = ballInitialPos;
    }

    void ApplyInputData(int idx) {           
        ball.rotation = inputs[idx]*Quaternion.Inverse(inputs[0]);       
        int prevIdx = idx == 0 ? 0 : idx-1;
        Quaternion dQ = inputs[idx]*Quaternion.Inverse(inputs[prevIdx]);        
        Vector3 deltaRot = dQ.eulerAngles;        
        ball.Rotate(deltaRot);
        float radiusFactor = Mathf.PI*ballRadius/180f;
        float dx = AdjustAnlge(deltaRot.x) * radiusFactor;
        float dy = AdjustAnlge(deltaRot.y) * radiusFactor;
        ball.position += new Vector3(-dy, dx, 0f);
    }

    float AdjustAnlge(float a) {
        return a > 180f ? a - 360f : a;
    }
}
