using UnityEngine;
using UnityEngine.Scripting;

public class AudioManager : MonoBehaviour {
    
    public static AudioManager instance;

    [Header("Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private GameObject HUDText;
    [SerializeField] private AudioLowPassFilter lowPassFilter;
    [SerializeField] private float lowPassFilterChangeSpeed = 1000f;
    [SerializeField] private float lowPassFilterCutoff = 1000f;
    [SerializeField] private float lowPassFilterCutoffMax = 13000f;
    [SerializeField] private float menuPitch = 0.9f;
    [SerializeField] private float pitchChangeSpeed = 1f;
    public bool gameplay;

    [Header("Zombie Horde")]
    [SerializeField] private AudioSource groanAudioSource;
    public float groanVolume = 1f;
    [SerializeField] private AudioSource buttonAudioSource;

    [Header("SFX")] // fall over noise, getting eaten noise, button press noise, zombie ambience noise
    [SerializeField] private AudioClip[] keyboardClips;
    [SerializeField] private float keyboardVolume;
    [SerializeField] private AudioClip fallOverClip;
    [SerializeField] private float fallOverVolume;
    [SerializeField] private AudioClip gettingEatenClip;
    [SerializeField] private float gettingEatenVolume;
    [SerializeField] private AudioClip buttonPressClip;
    [SerializeField] private float buttonPressVolume;

    // singleton
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    // enable music muffling
    public void EnableLowPassFilterCutoff() {
        musicAudioSource.pitch = 0.9f;
        lowPassFilter.cutoffFrequency = lowPassFilterCutoff;
        gameplay = false;
    }

    void Update() {
        // if in menus, muffle the music and stop zombie groans
        // if in game, unmuffle the music and play zombie groans

        if (gameplay) {
            if (musicAudioSource.pitch < 1f) {
                musicAudioSource.pitch += pitchChangeSpeed * Time.deltaTime;
            }
            if (lowPassFilter.cutoffFrequency < lowPassFilterCutoffMax) {
                lowPassFilter.cutoffFrequency += lowPassFilterChangeSpeed * Time.deltaTime;
            }
            groanAudioSource.enabled = true;
            groanAudioSource.volume = groanVolume;
        }
        else {
            if (musicAudioSource.pitch > menuPitch) {
                musicAudioSource.pitch -= pitchChangeSpeed * Time.deltaTime;
            }
            if (lowPassFilter.cutoffFrequency > lowPassFilterCutoff) {
                lowPassFilter.cutoffFrequency -= lowPassFilterChangeSpeed * Time.deltaTime;
            }
            groanAudioSource.enabled = false;
        }

        
    }

    public void PlayKeyboardSFX() {

        PlayClip(keyboardClips[Random.Range(0, keyboardClips.Length)], keyboardVolume);
    }

    public void PlayFallOverSFX() {
        
        PlayClip(fallOverClip, fallOverVolume);
    }

    public void PlayEatingSFX() {

        PlayClip(gettingEatenClip, gettingEatenVolume);
    }

    public void PlayButtonPressSFX() {

        buttonAudioSource.enabled = false;
        buttonAudioSource.enabled = true;
    }

    void PlayClip(AudioClip clip, float volume) {

        AudioSource.PlayClipAtPoint(clip, Vector2.zero, volume);
    }
}
