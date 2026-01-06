using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        SoundManager.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SoundManager.SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSlider.value = SoundManager.MusicVolume;
        sfxSlider.value = SoundManager.SFXVolume;
    }

    public void ApplySettings()
    {
        SoundManager.MusicVolume = musicSlider.value;
        SoundManager.SFXVolume = sfxSlider.value;

        PlayerPrefs.SetFloat("MusicVolume", SoundManager.MusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", SoundManager.SFXVolume);
    }
}
