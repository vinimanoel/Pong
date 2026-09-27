using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [SerializeField] private Key upKey = Key.W;
    [SerializeField] private Key downKey = Key.S;

    private void Update()
    {
        float input = 0f;

        if (Keyboard.current[upKey].isPressed)
        {
            input = 1f;
        }
        else if (Keyboard.current[downKey].isPressed)
        {
            input = -1f;
        }

        Vector3 movement = Vector3.up * input * speed * Time.deltaTime;

        transform.position += movement;

        Vector3 position = transform.position;

        position.y = Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
    }
}