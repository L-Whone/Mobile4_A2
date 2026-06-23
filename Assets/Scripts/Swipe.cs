using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class Swipe : MonoBehaviour
{
    [SerializeField] private float MinSwipeDistInches = 0.2f; // pixels

    private Vector2 _startPos;
    private bool _isSwiping;

    [SerializeField] private float _stationaryThreshold = 0.1f;
    private float _lastMoveTime;
    public UnityEvent<SwipeInfo> OnSwipe;

    [SerializeField] private float MagnitudeDivider = 1000f;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Update()
    {
        if (Touch.activeTouches.Count == 0) return;

        var touch = Touch.activeTouches[0];

        if (touch.phase == TouchPhase.Began)
        { // initial touch
            _startPos = touch.screenPosition;
            _isSwiping = true;
            _lastMoveTime = Time.time;
            Debug.Log("Started");
        }
        else if (touch.phase == TouchPhase.Stationary && _isSwiping)
        { // if you don't move
            if (Time.time - _lastMoveTime >= _stationaryThreshold)
            {
                _startPos = touch.screenPosition;
            }
        }
        else if (touch.phase == TouchPhase.Moved && _isSwiping)
        { // if you start moving after being stationary
            if (Time.time - _lastMoveTime >= _stationaryThreshold)
            {
                _startPos = touch.screenPosition;
            }

            _lastMoveTime = Time.time;
        }
        else if (touch.phase == TouchPhase.Ended && _isSwiping)
        { // finger has left screen
            Vector2 swipeDelta = touch.screenPosition - _startPos;

            if (swipeDelta.magnitude >= MinSwipeDistInches * Screen.dpi)
            {
                float angle = CalculateAngle(swipeDelta);
                float angleDeg = CalculateAngleDegrees(swipeDelta);
                Debug.Log($"Swipe Radians: {angle:F1}");
                Debug.Log($"Swipe Degrees: {angleDeg:F1}");
                Debug.Log($"MAGNITUDE: {swipeDelta.magnitude / MagnitudeDivider}");

                OnSwipe.Invoke(new SwipeInfo() { Angle = angle, AngleDegrees = angleDeg, Magnitude = swipeDelta.magnitude / MagnitudeDivider });
            }

            _isSwiping = false;
        }
    }

    public float CalculateAngle(Vector2 delta)
    {
        return Mathf.Atan2(delta.y, delta.x);
    }

    public float CalculateAngleDegrees(Vector2 delta)
    {
        float angle = CalculateAngle(delta) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360f;

        return angle;
    }

    private void OnDisable()
    {
        _isSwiping = false;
        EnhancedTouchSupport.Disable();
    }
}


