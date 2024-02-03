using UnityEngine;
using DG.Tweening;

public class Zooming : MonoBehaviour
{
    public Camera cam;
    public GameObject background;
    public float zoomMultiplier = 2;
    public float originalFov;
    public float defaultFov = 60;
    public float zoomDuration = 2;
    public bool allowToZoom = false;

    private void Start()
    {
        if(this.cam != null)
            originalFov = this.cam.fieldOfView;
    }
    void Update()
    {
        if (Input.GetKeyDown("z"))
        {
            allowToZoom = !allowToZoom;
        }

        if (allowToZoom) {
            if (cam.fieldOfView != defaultFov)
            {
                if (allowToZoom) ZoomCamera(defaultFov, 0.8f);
            }
        }
        else
        {
            if (cam.fieldOfView >= defaultFov)
                ResetCamera();
        }

    }
    public void ZoomCamera(float targetFov, float bgScale)
    {
        cam.DOFieldOfView(targetFov, zoomDuration);

        if (background != null) { 
            background.transform.DOScale(bgScale, zoomDuration);
        }
    }

    public void ResetCamera()
    {
        cam.fieldOfView = originalFov;
        if (background != null) background.transform.DOScale(1f, 0f);
    }
}
