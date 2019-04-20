using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WWWHelper : MonoBehaviour {
    public IEnumerator LoadTextFileFromStreamingAssets(string filename, UnityAction<string> callback) {
        string url = GetStreamingAssetsPath() + filename;
        using (WWW www = new WWW(url)){
            yield return www;
            if(callback != null)
                callback(www.text);            
        }
    }

	 public static string GetStreamingAssetsPath() {
        if (Application.platform == RuntimePlatform.Android)
            return "jar:file://" + Application.dataPath + "!/assets/";
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
            return  Application.dataPath + "/Raw/";
        else
            return Application.dataPath + "/StreamingAssets/";
    }
}
