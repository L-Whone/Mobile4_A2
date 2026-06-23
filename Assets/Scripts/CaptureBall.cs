using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class CaptureBall : MonoBehaviour
{
    [Header("Hold Position")]
    [SerializeField] private float _holdDistance = 0.6f;
    [SerializeField] private Vector2 _holdViewportPos = new(0.5f, 0.12f); // where the ball is on the screen

    [Header("Throw")]
    [SerializeField] private float _forceMultiplier = 10f;
    [SerializeField] private float _upwardBias = 0.5f;
    [SerializeField] private float _sideInfluence = 0.4f;
    [SerializeField] private float _minMagnitude = 0.15f;
    [SerializeField] private float _minUpwardAngleDeg = 30f;

    [SerializeField] private float _maxLifetime = 4f;

    public event System.Action OnBallDestroyed;

    [SerializeField] private Rigidbody _rb;
    private Camera _camera;
    private bool _isHeld = true;


    private void Reset()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
    }
    private void Start()
    {
        _camera = Camera.main;
    }
    private void Update()
    {
        if (!_isHeld) return;

        Vector3 holdPos = _camera.ViewportToWorldPoint(new Vector3(_holdViewportPos.x, _holdViewportPos.y, _holdDistance));
        transform.position = holdPos;
    }


    public void OnSwipeReceived(SwipeInfo info)
    {
        if (!_isHeld) return;
        if (info.Magnitude < _minMagnitude) return;

        float a = info.AngleDegrees;
        if (a < _minUpwardAngleDeg || a > (180f - _minUpwardAngleDeg)) return; // must be an upward throw

        Throw(info);
    }


    private void Throw(SwipeInfo info)
    {
        _isHeld = false;
        _rb.isKinematic = false;

        Vector3 dir = _camera.transform.forward + _camera.transform.up * _upwardBias; // up

        float horizontalComponent = Mathf.Cos(info.Angle);
        dir += _camera.transform.right * (horizontalComponent * _sideInfluence); // add some side movement

        dir.Normalize();

        _rb.AddForce(dir * (info.Magnitude * _forceMultiplier), ForceMode.VelocityChange);

        Destroy(gameObject, _maxLifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isHeld) return;

        MonsterObject monster = collision.gameObject.GetComponent<MonsterObject>();
        if (monster != null)
        {
            monster.Capture();
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnBallDestroyed?.Invoke();
    }
}