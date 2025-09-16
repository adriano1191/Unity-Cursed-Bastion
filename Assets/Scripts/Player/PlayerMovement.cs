using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer gfx;
    Rigidbody2D rb;
    public Vector2 move; // wype�niane przez akcj� "Move"
    // Start is called before the first frame update


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void FixedUpdate()
    {
        /*float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);*/
        rb.linearVelocity = move * speed; // ruch w fizyce
       //if (move.x > 0 && transform.localScale.x < 0) Flip();
       // else if (move.x < 0 && transform.localScale.x > 0) Flip();
       Flip();
    }

    private void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    private void Flip()
    {
        //var scale = gfx.localScale;
        //scale.x *= -1;
        //gfx.localScale = scale;
        if (move.x > 0 && !gfx.flipX) gfx.flipX=true;
        else if (move.x < 0 && gfx.flipX) gfx.flipX = false;
        //gfx.flipX = !gfx.flipX;

    }
}
