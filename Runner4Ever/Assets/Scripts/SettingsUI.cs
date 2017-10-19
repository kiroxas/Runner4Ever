using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsUI : MonoBehaviour
{
	public AudioMixer mixer;

	public Slider masterSlider;
    public Slider musicSlider;
    public Slider masterSFXSlider;

    protected float m_MasterVolume;
    protected float m_MusicVolume;
    protected float m_MasterSFXVolume;

    protected const float k_MinVolume = -80f;
    protected const string k_MasterVolumeFloatName = "MasterVolume";
    protected const string k_MusicVolumeFloatName = "MusicVolume";
    protected const string k_MasterSFXVolumeFloatName = "SFXVolume";

    private bool changed = false;

    public void Open()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        PlayerData.instance.Save();
    }

    void UpdateUI()
    {
        mixer.GetFloat(k_MasterVolumeFloatName, out m_MasterVolume);
        mixer.GetFloat(k_MusicVolumeFloatName, out m_MusicVolume);
        mixer.GetFloat(k_MasterSFXVolumeFloatName, out m_MasterSFXVolume);

        masterSlider.value = 1.0f - (m_MasterVolume / k_MinVolume);
        musicSlider.value = 1.0f - (m_MusicVolume / k_MinVolume);
        masterSFXSlider.value = 1.0f - (m_MasterSFXVolume / k_MinVolume);
    }

    public void MasterVolumeChangeValue(float value)
    {
        m_MasterVolume = k_MinVolume * (1.0f - value);
        mixer.SetFloat(k_MasterVolumeFloatName, m_MasterVolume);
		PlayerData.instance.masterVolume = m_MasterVolume;
		changed = true;
    }

    public void MusicVolumeChangeValue(float value)
    {
        m_MusicVolume = k_MinVolume * (1.0f - value);
        mixer.SetFloat(k_MusicVolumeFloatName, m_MusicVolume);
		PlayerData.instance.musicVolume = m_MusicVolume;
		changed = true;
    }

    public void MasterSFXVolumeChangeValue(float value)
    {
        m_MasterSFXVolume = k_MinVolume * (1.0f - value);
        mixer.SetFloat(k_MasterSFXVolumeFloatName, m_MasterSFXVolume);
		PlayerData.instance.masterSFXVolume = m_MasterSFXVolume;
		changed = true;
    }
}