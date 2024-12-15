using UnityEngine.Audio;
using UnityEngine;


// System serializable makes objects appear on the inspector
[System.Serializable]
public class Sound
{
    public string name;


    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(0.1f, 3f)]
    public float pitch;

    // source is hidden in the inspector window
    [HideInInspector]
    public AudioSource source;
}
