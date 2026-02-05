using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;

public class CameraZoom : MonoBehaviour
{

    private Camera cam;
    public float zoomSensitivity;
    public float panSensitivity;
    [SerializeField] private Vector2 touchPosition1;
    [SerializeField] private Vector2 touchPosition2;
    [SerializeField] private int touchCount;
    [SerializeField] private float zoom;
    [SerializeField] private float zoomValue;
    [SerializeField] private float prevDistance;
    [SerializeField] private float currentDistance;
    private ReadOnlyArray<TouchControl> touches;
    private mouseRaycaster mr;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
    void Start()
    {   
        cam = this.GetComponent<Camera>();
        StartCoroutine(FrameWait());
        mr = TurnManager.Instance.gameObject.GetComponent<mouseRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        touches = UnityEngine.InputSystem.Touchscreen.current.touches;
        touchCount = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
    }

    private IEnumerator FrameWait()
    {
        while (true)
        {   
            yield return new WaitForEndOfFrame();
            touchPosition1 = touches[0].position.ReadValue();
            touchPosition2 = touches[1].position.ReadValue();
                
            currentDistance = Vector2.Distance(touchPosition1, touchPosition2);
            
            var midPoint = Vector3.Lerp(touchPosition1, touchPosition2, 0.5f);
            midPoint.y = 24f;
            
            //if pinching
            if (touchCount == 2)
            {
                zoom = cam.fieldOfView;
                //zoom in
                if (currentDistance > prevDistance)
                {
                    zoom -= 1;
                    cam.fieldOfView = Mathf.Clamp(Mathf.Lerp(cam.fieldOfView, zoom, Time.deltaTime * zoomSensitivity), 10f, 27f);
                    
                }
                //zoom out
                else if (currentDistance < prevDistance)
                {
                    zoom += 1;
                    cam.fieldOfView = Mathf.Clamp(Mathf.Lerp(cam.fieldOfView, zoom, Time.deltaTime * zoomSensitivity), 10f, 27f);
                    
                }
                
                prevDistance = currentDistance;
                
            }
            else if (touchCount == 1 && tileManager.Instance.toolBeingUsed == false)
            {
                Vector3 touchDelta = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.delta.ReadValue();
                touchDelta *= 0.01f;
                Vector3 positionDelta = new Vector3(touchDelta.y, 0, touchDelta.x);
                
                cam.transform.position += new Vector3(positionDelta.x * -1, 0, positionDelta.z).normalized * panSensitivity;
            }
            
            //clamp camera position
            Vector3 camPos;
            
            float FOVchange = -27 + cam.fieldOfView;
            
            camPos.x = Mathf.Clamp(cam.transform.position.x, -30f+FOVchange, -30-FOVchange);
            camPos.y = cam.transform.position.y;
            camPos.z = Mathf.Clamp(cam.transform.position.z, 3f + FOVchange, 3f - FOVchange);
            
            cam.transform.position = camPos;
            
        }
    }
}
