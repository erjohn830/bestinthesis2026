using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyVolumes();
    }

    // =========================
    // MUSIC
    // =========================

    public void SetMusicVolume(float value)
    {
        musicVolume = value;

        SetMixerVolume(
            "MusicVolume",
            musicVolume
        );

        PlayerPrefs.SetFloat(
            MUSIC_KEY,
            musicVolume
        );

        PlayerPrefs.Save();
    }

    // =========================
    // SOUND EFFECTS
    // =========================

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;

        SetMixerVolume(
            "SFXVolume",
            sfxVolume
        );

        PlayerPrefs.SetFloat(
            SFX_KEY,
            sfxVolume
        );

        PlayerPrefs.Save();
    }

    // =========================
    // GET VALUES
    // =========================

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    // =========================
    // LOAD
    // =========================

    private void LoadSettings()
    {
        musicVolume =
            PlayerPrefs.GetFloat(
                MUSIC_KEY,
                1f
            );

        sfxVolume =
            PlayerPrefs.GetFloat(
                SFX_KEY,
                1f
            );

        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        SetMixerVolume(
            "MusicVolume",
            musicVolume
        );

        SetMixerVolume(
            "SFXVolume",
            sfxVolume
        );
    }

    private void SetMixerVolume(
        string parameter,
        float value)
    {
        if (audioMixer == null)
            return;

        if (value <= 0.001f)
        {
            audioMixer.SetFloat(
                parameter,
                -80f
            );
        }
        else
        {
            float volume =
                Mathf.Log10(value) * 20f;

            audioMixer.SetFloat(
                parameter,
                volume
            );
        }
    }
}