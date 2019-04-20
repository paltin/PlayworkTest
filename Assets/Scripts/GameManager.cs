using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour{
    public string inputDataFileName;
    public Transform ball;
    public Transform smallBall;
    public float timeStep = 0.05f;
    public Text uxText;
    public int stepSize = 12;

    List<Quaternion> inputs;
    int currInputIdx = 0;
    float ballRadius = -1f;
    bool isTestMode = false;
    Vector3 cumdelta;    

    void Start(){
        inputs = new List<Quaternion>();
        LoadData();
        if(ball != null)
            ballRadius = smallBall.GetComponent<Renderer>().bounds.extents.x;        
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

        StartSimulation();
    }

    void PrintQuaternion(Quaternion q) {
        Debug.LogFormat("q = ({0},{1},{2},{3})", q[0], q[1], q[2], q[3]);
    }

    
    void StartSimulation() {
        if(ballRadius > 0f) {            
            StartCoroutine(SimulationTimer(timeStep));                    
        } else
            Debug.LogError("Can't simulate, radius has negative value");
    }

    private void Update() {
        if(!isTestMode)
            return;

        if (Input.GetKeyDown(KeyCode.N)) {
            NextStep();
        }
    }

    void TestMode() {
        currInputIdx = 0;
        Quaternion Q = inputs[currInputIdx];
        ball.rotation = Q;
        Vector3 a = Q.eulerAngles;
        uxText.text = a.ToString();     
        isTestMode = true;
        cumdelta = Vector3.zero;
    }

    void NextStep() {
        currInputIdx += stepSize;
        if(currInputIdx < inputs.Count) {
            Quaternion Q = inputs[currInputIdx];
            ball.rotation = Q;
            Vector3 a = Q.eulerAngles;
            uxText.text = a.ToString();        
            Vector3 delta = inputs[currInputIdx-stepSize].eulerAngles - a;
            cumdelta += delta;
            uxText.text += "\n" + cumdelta.ToString();            
        }
    }

    IEnumerator SimulationTimer(float delay) {
        currInputIdx = 1;
        while(currInputIdx < inputs.Count) {
            ApplyInputData(currInputIdx++);            
            yield return new WaitForSeconds(delay);
        }     
    }

    void ApplyInputData(int idx) {
        
        ball.rotation = inputs[idx];        
        int prevIdx = idx == 0 ? 0 : idx-1;
        Quaternion dQ = inputs[idx]*Quaternion.Inverse(inputs[prevIdx]);
        Vector3 deltaRot = dQ.eulerAngles;
        smallBall.Rotate(deltaRot);
        
        float dx = AdjustAnlge(deltaRot.x)*Mathf.PI*ballRadius/180f;
        float dy = AdjustAnlge(deltaRot.y)*Mathf.PI*ballRadius/180f;

        smallBall.position += new Vector3(-dy, dx, 0f);

    }

    float AdjustAnlge(float a) {
        return a > 180f ? a - 360f : a;
    }

}
