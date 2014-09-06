using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMove : MonoBehaviour
{

    public float speed = 10f;
    public float jumpSpeed = 700f;

    public Transform groundChecker;
    public LayerMask groundMask;

    private bool _isGrounded;
    private bool _canJump = true;

    private float _groundRadius = 0.2f;
    private float _moveX;
    private bool _isFacingRight = true;
    private Animator _animator;
    private Vector2 _temp;

    void Awake()
    {
        _temp = new Vector2();
        _animator = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        if (_isGrounded && _canJump && Input.GetKeyDown(KeyCode.Space))
        {
            _temp.Set(0, jumpSpeed);
            rigidbody2D.AddForce(_temp);
        }
    }

    void FixedUpdate()
    {
        _moveX = Input.GetAxis("Horizontal");
        _isGrounded = Physics2D.OverlapCircle(groundChecker.position, _groundRadius, groundMask);

        Move(_moveX);
        Animate(_moveX);
    }

    private void Animate(float _movX)
    {
        _animator.SetFloat("Speed", _movX);
        _animator.SetBool("Grounded", _isGrounded);
        _animator.SetFloat("VSpeed", rigidbody2D.velocity.y);
    }

    private void Move(float moveX)
    {
        _temp.Set(_moveX * speed, rigidbody2D.velocity.y);        

        rigidbody2D.velocity = _temp;
        if (_moveX > 0 && !_isFacingRight)
            Flip();
        else if (_moveX < 0 && _isFacingRight)
            Flip();

    }

    void Flip()
    {
        _isFacingRight = !_isFacingRight;

      //  var theScale = transform.localScale;
    //    theScale.x *= -1;
      //  transform.localScale = theScale;
    }
}
