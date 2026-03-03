using UnityEngine;

public class ParallaxState
{
    public Vector3 CameraPosition { get; private set; }
    public Quaternion CameraRotation { get; private set; }
    public float CameraAspectRatio { get; private set; }
    public float CameraFieldOfView { get; private set; }
    public Vector3 ParallaxPosition { get; private set; }
    public float ZoomLevel { get; private set; }
    public bool VisibleInEditMode { get; private set; }

    public bool HasChanged(Camera camera, Vector3 parallaxPosition, float parallaxZoom, bool visible)
    {
        if (camera == null)
        {
            return false;
        }

        return camera.transform.position != CameraPosition ||
               camera.transform.rotation != CameraRotation ||
               camera.aspect != CameraAspectRatio ||
               parallaxZoom != ZoomLevel ||
               parallaxPosition != ParallaxPosition ||
               camera.fieldOfView != CameraFieldOfView ||
               visible != VisibleInEditMode;
    }

    public void Update(Camera camera, Vector3 parallaxPosition, float parallaxZoom, bool visible)
    {
        if (camera == null)
        {
            return;
        }

        CameraPosition = camera.transform.position;
        CameraRotation = camera.transform.rotation;
        CameraAspectRatio = camera.aspect;
        ZoomLevel = parallaxZoom;
        ParallaxPosition = parallaxPosition;
        CameraFieldOfView = camera.fieldOfView;
        VisibleInEditMode = visible;
    }

    public void Clear()
    {
        CameraPosition = Vector3.zero;
        CameraRotation = Quaternion.identity;
        CameraAspectRatio = 0f;
        ZoomLevel = 0f;
        ParallaxPosition = Vector3.zero;
        CameraFieldOfView = 0f;
        VisibleInEditMode = false;
    }
}