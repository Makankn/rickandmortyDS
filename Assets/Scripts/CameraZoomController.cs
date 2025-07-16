using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    public Transform startTarget;  // Rick's face
    public Transform endTarget;    // Full scene
    public float zoomDuration = 2.0f;
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isZooming = false;
    private float zoomTimer = 0f;
    private Vector3 initialPos;
    private Quaternion initialRot;

    void Start()
    {
        // Start camera at Rick's face
        if (startTarget)
        {
            transform.position = startTarget.position;
            transform.rotation = startTarget.rotation;
        }
        
    }

    public void StartZoomOut()
    {
        if (!isZooming && endTarget != null)
        {
            isZooming = true;
            zoomTimer = 0f;
            initialPos = transform.position;
            initialRot = transform.rotation;
        }
    }

    void Update()
    {
        if (isZooming)
        {
            zoomTimer += Time.deltaTime;
            float t = Mathf.Clamp01(zoomTimer / zoomDuration);
            float easedT = easing.Evaluate(t);

            transform.position = Vector3.Lerp(initialPos, endTarget.position, easedT);
            transform.rotation = Quaternion.Slerp(initialRot, endTarget.rotation, easedT);

            if (t >= 1f)
            {
                isZooming = false;
            }
        }
    }
}
