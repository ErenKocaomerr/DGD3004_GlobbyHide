using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class CameraFocusController : MonoBehaviour
{
    public static CameraFocusController Instance;

    [Tooltip("Varsayýlan etkileþim kamerasý (opsiyonel). Eðer trigger kendi camera'sýný vermezse bu kullanýlýr).")]
    public CinemachineCamera interactCam;
    public CinemachineCamera interactCam2;
    public CinemachineCamera interactCam3;

    [Tooltip("Ana kamera (virtual camera). Genelde player takip eden vcam buraya atanýr.")]
    public CinemachineCamera mainCam;

    [Tooltip("Zoom edildikten sonra hedef orthographic size (eðer trigger tarafý override etmezse interactCam'ýn size'ý kullanýlýr).")]
    public float zoomedSize = 3f;

    [Tooltip("Zoom Lerp hýzý (büyük -> daha hýzlý).")]
    public float transitionSpeed = 3f;

    [Header("Runtime state (read-only)")]
    public bool zoomEnd = false;

    // Event: zoom bittiðinde tetiklenir
    public UnityEvent OnZoomComplete;

    float defaultSize;
    Transform defaultFollow;
    int mainCamDefaultPriority;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCam == null)
        {
            Debug.LogWarning("CameraFocusController: mainCam atanmamýþ. Inspector'dan atayýn.");
        }
        else
        {
            defaultSize = mainCam.Lens.OrthographicSize;
            defaultFollow = mainCam.Follow;
            mainCamDefaultPriority = mainCam.Priority;
        }

        // initialize flag
        zoomEnd = false;
    }

    // debug istersen aç, istersen kaldýr
    //private void Update()
    //{
    //    Debug.Log($"CameraFocusController.zoomEnd = {zoomEnd}");
    //}

    /// <summary>
    /// Verilen virtual camera'ya geçiþ yapar, eðer followTarget verilmiþse atar.
    /// Eðer overrideZoom > 0 ise hedef zoom o olacak; deðilse cinemachineCamera.m_Lens.OrthographicSize kullanýlýr.
    /// </summary>
    public void FocusOn(CinemachineCamera cinemachineCamera, Transform target, float overrideZoom = -1f)
    {
        if (cinemachineCamera == null)
        {
            // fallback: eðer controller'ýn interactCam'i varsa onu kullan
            if (interactCam != null)
                cinemachineCamera = interactCam;
            else
            {
                Debug.LogWarning("CameraFocusController.FocusOn: verilen cinemachineCamera null ve fallback yok.");
                return;
            }
        }

        zoomEnd = false;

        // set follow target
        if (target != null)
            cinemachineCamera.Follow = target;

        // set priorities (simple: bring target cam up, push main down)
        cinemachineCamera.Priority = 20;
        if (mainCam != null)
            mainCam.Priority = 10;

        // compute end size
        float endSize = overrideZoom > 0f ? overrideZoom : cinemachineCamera.Lens.OrthographicSize;

        // stop previous coroutines
        StopAllCoroutines();
        StartCoroutine(LerpZoom(mainCam.Lens.OrthographicSize, endSize));
    }

    /// <summary>
    /// Kamera ayarlarýný eski haline çeker (mainCam priority, interact cam follow temizleme).
    /// </summary>
    public void ResetCamera()
    {
        zoomEnd = false;

        if (interactCam != null)
            interactCam.Priority = 5;

        interactCam2.Priority = 5;
        interactCam3.Priority = 5;

        if (mainCam != null)
            mainCam.Priority = mainCamDefaultPriority;

        if (interactCam != null)
            interactCam.Follow = null;

        StopAllCoroutines();
        StartCoroutine(LerpZoom(mainCam.Lens.OrthographicSize, defaultSize));
    }

    IEnumerator LerpZoom(float start, float end)
    {
        float t = 0f;
        float duration = Mathf.Max(0.01f, 1f / transitionSpeed);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float v = Mathf.Lerp(start, end, t);
            // set mainCam lens size smoothly
            if (mainCam != null)
            {
                var lens = mainCam.Lens;
                lens.OrthographicSize = v;
                mainCam.Lens = lens;
            }
            yield return null;
        }

        // ensure final
        if (mainCam != null)
        {
            var lens = mainCam.Lens;
            lens.OrthographicSize = end;
            mainCam.Lens = lens;
        }

        zoomEnd = true;
        OnZoomComplete?.Invoke();
    }
}
