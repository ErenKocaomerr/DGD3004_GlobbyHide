using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public HitZone hitZone;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            HandleHit(Direction.Up);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            HandleHit(Direction.Down);

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            HandleHit(Direction.Left);

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            HandleHit(Direction.Right);
    }

    void HandleHit(Direction dir)
    {
        if (hitZone == null) return;

        HitResult result = hitZone.TryHit(dir);

        // Görsel / audio feedback için burada kontrol edebilirsin
        switch (result)
        {
            case HitResult.Perfect:
                Debug.Log("PERFECT!");
                // ek efekt, ses, skor arttýrma vb.
                break;
            case HitResult.Good:
                Debug.Log("Good");
                break;
            case HitResult.Miss:
                Debug.Log("Miss");
                break;
        }
    }
}
