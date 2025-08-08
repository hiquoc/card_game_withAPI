using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class DataFetcher
{   
    public static string address= "";
    public static void LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ConfigData config = JsonUtility.FromJson<ConfigData>(json);
            address = config.apiUrl;
            Debug.Log(address);
        }
        else
        {
            Debug.LogWarning("Config file not found.");
        }
    }
    [Serializable]
    public class ConfigData
    {
        public string apiUrl;
    }
    public static string GetReplacedPortUrl(int newPort)
    {
        UriBuilder uriBuilder = new(address);
        uriBuilder.Port = newPort;
        return uriBuilder.ToString();
    }
}