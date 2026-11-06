using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    public float positionSmooth = 5f;
    public float rotationSmooth = 3f;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        if (target != null) SnapToTarget();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmooth * Time.deltaTime);

        Vector3 lookPoint = target.TransformPoint(lookOffset);
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmooth * Time.deltaTime);
    }

    private void SnapToTarget()
    {
        transform.position = target.TransformPoint(offset);
        Vector3 lookPoint = target.TransformPoint(lookOffset);
        transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
    }
}