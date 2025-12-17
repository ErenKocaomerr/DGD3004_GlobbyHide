using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.2f;
    public float dampingSpeed = 1.5f;

    private Vector3 initialPosition;
    private float currentShakeTime = 0f;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (currentShakeTime > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            currentShakeTime -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            transform.localPosition = initialPosition;
            currentShakeTime = 0f;
        }
    }

    public void Shake()
    {
        currentShakeTime = shakeDuration;
    }
}
