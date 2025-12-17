using UnityEngine;

public class PuzzleInteractable : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public GameObject promptObject; // "Press E" UI

    bool playerInside = false;

    void Start()
    {
        if (promptObject != null) promptObject.SetActive(false);
        if (puzzleManager == null) Debug.LogWarning($"{name}: puzzleManager atanmamýþ!");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        playerInside = true;
        if (promptObject != null) promptObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        playerInside = false;
        if (promptObject != null) promptObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInside || puzzleManager == null) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            puzzleManager.OpenPuzzle();
            if (promptObject != null) promptObject.SetActive(false);
        }
    }
}
