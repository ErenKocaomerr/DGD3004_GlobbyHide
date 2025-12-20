using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Heryerden eriþim için Singleton

    [Header("--- Kazanýlan Yetenekler ---")]
    public bool hasDash = false;
    public bool hasDoubleJump = false;
    public bool hasWallJump = false;
    public bool hide = false;

    public Vector2 lastHubPosition; // Oyuncunun son durduðu yer
    public bool isReturningToHub;

    public List<string> talkedNpcIDs = new List<string>();

    public Vector2 currentCheckpointPos; // Son checkpoint koordinatý
    public bool hasActiveCheckpoint = false;

    void Awake()
    {
        // Singleton Yapýsý: Bu objeden sadece bir tane olsun ve sahne geçiþinde yok olmasýn.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // BÜYÜ BURADA
        }
        else
        {
            Destroy(gameObject); // Eðer 2. bir GameManager oluþursa onu yok et
        }
    }

    // Yeteneði Açmak Ýçin Fonksiyon
    public void UnlockAbility(string abilityName)
    {
        switch (abilityName)
        {
            case "Dash":
                hasDash = true;
                break;
            case "DoubleJump":
                hasDoubleJump = true;
                break;
            case "WallJump":
                hasWallJump = true;
                break;
            case "Hide":
                hide = true;
                break;
        }

        // Ýstersen burada PlayerPrefs.SetInt ile Kayýt (Save) iþlemi de yapabilirsin.
    }

    public bool HasTalkedTo(string npcID)
    {
        return talkedNpcIDs.Contains(npcID);
    }

    // NPC'yi "Konuþuldu" olarak iþaretle
    public void MarkNpcAsTalked(string npcID)
    {
        if (!talkedNpcIDs.Contains(npcID))
        {
            talkedNpcIDs.Add(npcID);
        }
    }

    public void SetCheckpoint(Vector2 pos)
    {
        currentCheckpointPos = pos;
        hasActiveCheckpoint = true;
        Debug.Log("Checkpoint Kaydedildi: " + pos);
    }
}
