using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{

    public AudioSource AudioSource;
    public AudioClip speakClip;

    public void GoHub() 
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Speak() 
    {
        AudioSource.PlayOneShot(speakClip);
    }
}
