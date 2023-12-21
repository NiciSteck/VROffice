using System;
using System.Collections;
using System.Collections.Generic;
using Proyecto26;
using UnityEditor;
using UnityEngine;

public class RestTest : MonoBehaviour
{
    public bool go = false;
    void Update()
    {
        if (go)
        {
            RestClient.Get("http://127.0.0.1:5005/test").Then(response =>
            {
                EditorUtility.DisplayDialog("Response", response.Text, "Ok");
            });
            RestClient.Put("http://127.0.0.1:5005/test", JsonUtility.ToJson(new JsonRepresentation{fml = 10})).Then(response =>
            {
                EditorUtility.DisplayDialog("Response", response.Text, "Ok");
            });
            RestClient.Get("http://127.0.0.1:5005/test").Then(response =>
            {
                EditorUtility.DisplayDialog("Response", response.Text, "Ok");
            });

            
            
            float[]pointArray = {0.0f,0.0f,0.0f};
               
            Debug.Log("Utility: " + JsonUtility.ToJson(new JsonRepresentation2{label = "tv",point = pointArray}));
            Debug.Log("Utility: " + JsonHelper.ArrayToJsonString(new int[]{1,2,3}));
            Debug.Log(JsonUtility.ToJson(new JsonRepresentation[]{new JsonRepresentation{fml = 10},new JsonRepresentation{fml = 10},new JsonRepresentation{fml = 10}}));
            Debug.Log(JsonHelper.ArrayToJsonString<JsonRepresentation>(new JsonRepresentation[]{new JsonRepresentation{fml = 10},new JsonRepresentation{fml = 10},new JsonRepresentation{fml = 10}}));
            
            JsonRepresentation[] arr = new JsonRepresentation[3];
            arr[0] = new JsonRepresentation{fml = 10};
            arr[1] = new JsonRepresentation{fml = 11};
            arr[2] = new JsonRepresentation{fml = 12};
            String jsonString = "{\"points\":[";
            for (int i = 0; i < arr.Length; i++)
            {
                jsonString += JsonUtility.ToJson(arr[i]) + ",";
            }
            jsonString = jsonString.Substring(0,jsonString.Length-1) + "]}";
            Debug.Log(jsonString);
            
            Debug.Log(JsonUtility.ToJson(arr[0]));
            Debug.Log(JsonUtility.ToJson(new JsonRepresentation{fml = 10}));
            Debug.Log("Helper: " + JsonHelper.ArrayToJsonString<JsonRepresentation>(arr));
            Debug.Log("Utility: " + JsonUtility.ToJson(arr));
            
            
            RestClient.Put("http://127.0.0.1:5005/test2", jsonString).Then(response =>
            {
                EditorUtility.DisplayDialog("Response", response.Text, "Ok");
            });
            
            // RestClient.Put("http://127.0.0.1:5005//test2", JsonHelper.ArrayToJsonString(arr)).Then(response =>
            // {
            //     EditorUtility.DisplayDialog("Response", response.Text, "Ok");
            // });
        }

        go = false;
    }
    
    
    private struct JsonRepresentation
    {
        public int fml;
    }
    
    [Serializable]
    private struct JsonRepresentation2
    {
        public String label;
        //public Vector3 point;
        public float[] point;
    }
}

