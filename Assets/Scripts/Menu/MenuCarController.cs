using UnityEngine;

public class MenuCarController : MonoBehaviour
{
    public float followSpeed = 5f;
    public float driftStrength = 2f;
    public Camera cam;

    Vector3 velocity;

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.transform.position.y;
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        Vector3 target = new Vector3(worldPos.x, transform.position.y, worldPos.z);

        velocity = Vector3.Lerp(velocity, (target - transform.position), Time.deltaTime * driftStrength);
        transform.position += velocity * Time.deltaTime * followSpeed;

        if (velocity != Vector3.zero)
            transform.forward = Vector3.Lerp(transform.forward, velocity.normalized, Time.deltaTime * 5f);
    }
}