using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/SoundEffect")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public struct SoundStruct
    {
        public string name;
        public AudioClip clip;
    }
    public SoundStruct[] sounds;
    Dictionary<string, AudioClip> soundMap;
    public AudioClip GetClip(string name)
    {
        if (soundMap == null)
        {
            soundMap = new();
            foreach (var sound in sounds)
            {
                soundMap[sound.name] = sound.clip;
            }
        }
        return soundMap.TryGetValue(name, out AudioClip clip) ? clip : null;
    }
}