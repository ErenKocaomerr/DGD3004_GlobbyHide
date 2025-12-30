using UnityEngine;

public class EndManager : MonoBehaviour
{
    public static EndManager Instance;

    public int value; // taþýnacak deðer

    void Awake()
    {
        // Eðer baþka bir GameManager varsa yok et
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Sahne deðiþince yok olmasýn
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            EndManager.Instance.value++;
        }
    }
}
