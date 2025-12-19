using Unity.Cinemachine;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker instance;

    public CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void Shake(float force)
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse(Vector3.down * force); // Aþaðý doðru vuruþ hissi
    }
}
