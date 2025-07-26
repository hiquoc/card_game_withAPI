using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource audioSource;
    public SoundLibrary soundLibrary;
    public float coolDown = 0.02f;
    Dictionary<string, float> lastPlayDict = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void Play(string name)
    {
        if (lastPlayDict.TryGetValue(name, out float lastTime) && Time.time - lastTime < coolDown) return;
        AudioClip clip = soundLibrary.GetClip(name);
        if (clip)
        {
            /*lastPlayDict.Clear();*/
            lastPlayDict[name] = Time.time;
            audioSource.PlayOneShot(clip);
            /*Debug.Log(name);*/
        }
        else
        {
            Debug.LogWarning("Can't find sound effect :" + name);
        }
    }
}