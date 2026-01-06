using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource musicSource;
    public AudioClip[] playlist;
    public static float MusicVolume = 1f;
    public static float SFXVolume = 1f;

    int currentTrack = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; 
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayTrack();
    }

    void Update()
    {
        musicSource.volume = MusicVolume;

        if (!musicSource.isPlaying)
            NextTrack();
    }

    void PlayTrack()
    {
        musicSource.clip = playlist[currentTrack];
        musicSource.Play();
    }

    void NextTrack()
    {
        currentTrack = (currentTrack + 1) % playlist.Length;
        PlayTrack();
    }
}
