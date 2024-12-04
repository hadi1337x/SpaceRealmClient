using LiteNetLib;
using LiteNetLib.Utils;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 movement;

    public TMP_Text playerName;

    private NetDataWriter writer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        writer = new NetDataWriter();
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }
    }

    void FixedUpdate()
    {
        SendMovementToServer();
    }

    private void SendMovementToServer()
    {
        writer.Reset();
        writer.Put("Movement");
        writer.Put(transform.position.x);
        writer.Put(transform.position.y);
        writer.Put(movement.x);
        writer.Put(isGrounded);

        Connection.Instance.Server.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void UpdatePositionFromServer(Vector2 position)
    {
        transform.position = position;
    }
}
