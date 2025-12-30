using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("--- Kazanýlan Yetenekler ---")]
    public bool hasDash = false;
    public bool hasDoubleJump = false;
    public bool hasWallJump = false;
    public bool hide = false;

    public Vector2 lastHubPosition;
    public bool isReturningToHub;

    [Header("--- Diyalog Hafýzasý ---")]
    public List<string> talkedNpcIDs = new List<string>();

    public Vector2 currentCheckpointPos;
    public bool hasActiveCheckpoint = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // --- TEST ÝÇÝN KISAYOL ---
        // Sadece Unity Editöründe çalýþýr, oyunun son halinde çalýþmaz (Güvenli)
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P)) // 'P' tuþuna basýnca hafýzayý sil
        {
            ResetDialogueMemory();
        }
#endif
    }

    // --- YENÝ: SIFIRLAMA FONKSÝYONU ---
    // [ContextMenu] sayesinde oyun çalýþýrken GameManager componentine 
    // sað týklayýp bu fonksiyonu çalýþtýrabilirsin.
    [ContextMenu("Reset Dialogue Memory")]
    public void ResetDialogueMemory()
    {
        talkedNpcIDs.Clear(); // Listeyi temizle
        Debug.Log("<color=red>--- DÝYALOG HAFIZASI SIFIRLANDI! ---</color>");
    }

    // Yeteneði Açmak Ýçin Fonksiyon
    public void UnlockAbility(string abilityName)
    {
        switch (abilityName)
        {
            case "Dash": hasDash = true; break;
            case "DoubleJump": hasDoubleJump = true; break;
            case "WallJump": hasWallJump = true; break;
            case "Hide": hide = true; break;
        }
    }

    public bool HasTalkedTo(string npcID)
    {
        return talkedNpcIDs.Contains(npcID);
    }

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
