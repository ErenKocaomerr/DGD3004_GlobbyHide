using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance; // Singleton

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip hitClip;
    public AudioClip missClip;
    public AudioClip buttonClip;
    public AudioClip enemyHurtClip;
    public AudioClip cryClip;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Sahne geçiþinde kaybolmasýn
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayHit() => PlaySFX(hitClip);
    public void PlayMiss() => PlaySFX(missClip);
    public void PlayButton() => PlaySFX(buttonClip);
    public void PlayEnemyHurt() => PlaySFX(enemyHurtClip);
    public void PlayCry() => PlaySFX(cryClip);

}
