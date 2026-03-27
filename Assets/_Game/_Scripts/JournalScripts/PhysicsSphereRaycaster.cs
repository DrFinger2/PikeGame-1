using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Event/Physics Sphere Raycaster")]
[RequireComponent(typeof(Camera))]
public class PhysicsSphereRaycaster : BaseRaycaster
{
    [Header("Sphere Settings")]
    [Tooltip("How thick the click 'laser' is. Increase this if you still have to be too precise.")]
    [SerializeField] private float m_ClickRadius = 0.5f;

    [Tooltip("Layer mask. ENSURE your Bookmarks are on this layer!")]
    [SerializeField] private LayerMask m_EventMask = -1;

    [Header("Debug Visualization")]
    [SerializeField] private bool m_ShowDebug = true;
    [SerializeField] private Color m_RayColor = Color.red;
    [SerializeField] private Color m_SphereColor = Color.cyan;

    protected Camera m_EventCamera;
    
    // Memory for Gizmos
    private Ray m_LastRay;
    private List<Vector3> m_LastHitCenters = new List<Vector3>();
    private float m_LastDistance = 0f;

    public override Camera eventCamera
    {
        get
        {
            if (m_EventCamera == null)
                m_EventCamera = GetComponent<Camera>();
            return m_EventCamera != null ? m_EventCamera : Camera.main;
        }
    }


    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        Ray ray = eventCamera.ScreenPointToRay(eventData.position);
        float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

        // Perform the thick cast
        RaycastHit[] hits = Physics.SphereCastAll(ray, m_ClickRadius, dist, m_EventMask);

        // --- DEBUG STORAGE ---
        if (m_ShowDebug)
        {
            m_LastRay = ray;
            m_LastDistance = dist;
            m_LastHitCenters.Clear();
        }

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

            foreach (var hit in hits)
            {
                if (m_ShowDebug)
                {
                    // Calculate where the CENTER of the sphere was at the moment of impact
                    Vector3 sphereCenterAtHit = ray.origin + (ray.direction * hit.distance);
                    m_LastHitCenters.Add(sphereCenterAtHit);
                }

                resultAppendList.Add(new RaycastResult
                {
                    gameObject = hit.collider.gameObject,
                    module = this,
                    distance = hit.distance,
                    worldPosition = hit.point,
                    worldNormal = hit.normal,
                    screenPosition = eventData.position,
                    index = resultAppendList.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                });
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!m_ShowDebug || eventCamera == null) return;

        // 1. Draw the "Laser" path
        Gizmos.color = m_RayColor;
        Gizmos.DrawRay(m_LastRay.origin, m_LastRay.direction * m_LastDistance);

        // 2. Draw the "Click Bubble" at the start
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(m_LastRay.origin, m_ClickRadius);

        // 3. Draw a sphere for every hit detected
        Gizmos.color = m_SphereColor;
        foreach (Vector3 hitCenter in m_LastHitCenters)
        {
            // This sphere represents the "thickness" that actually touched the collider
            Gizmos.DrawWireSphere(hitCenter, m_ClickRadius);
            
            // Draw a small solid sphere at the center point
            Gizmos.DrawSphere(hitCenter, m_ClickRadius * 0.1f);
        }
    }
#endif
}