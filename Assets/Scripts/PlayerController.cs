using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 dir;
    [SerializeField] float speed;
    [SerializeField, Range(0,1)] float smooth;
    Vector2 vel;


    public void OnMove(InputAction.CallbackContext ctx)
    {
        dir = ctx.ReadValue<Vector2>();
    }
    
    
    private void Update()
    {
        Move();
    }

    void Move()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = currentPos + (dir * speed);
        Vector2 newPos = Vector2.SmoothDamp(currentPos, targetPos, ref vel, smooth);
        transform.position = newPos;
    }
}
