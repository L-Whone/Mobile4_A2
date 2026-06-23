using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager _arRaycastManager;
    [SerializeField] private Camera _arCamera;
    [SerializeField] private GameObject _pointerPrefab;

    [Header("Monsters")]
    [SerializeField] private GameObject[] _placeableMonsters;

    [Header("Capture Ball")]
    [SerializeField] private GameObject _captureBallPrefab;
    [Tooltip("The Swipe component on the scene that generates SwipeInfo events.")]
    [SerializeField] private Swipe _swipeHandler;

    private readonly List<ARRaycastHit> _arHits = new();
    private readonly List<MonsterObject> _placedMonsters = new();
    private CaptureBall _activeBall;

    private UnityAction<SwipeInfo> _ballSwipeListener;

    public int MonstersCount => _placedMonsters.Count;

    public event System.Action OnMonsterCaptured;

    private void Update()
    {
        UpdatePointer();
    }


    public void TrySpawnMonster()
    {
        if (_placeableMonsters == null || _placeableMonsters.Length == 0) return;

        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        if (_arRaycastManager.Raycast(screenCenter, _arHits, TrackableType.PlaneWithinPolygon))
        {
            PlaceMonster(_arHits[0].pose);
        }
    }

    public void TrySpawnCaptureBall()
    {
        if (_activeBall != null) return;
        if (_captureBallPrefab == null || _swipeHandler == null) return;

        GameObject ballGo = Instantiate(_captureBallPrefab);
        _activeBall = ballGo.GetComponent<CaptureBall>();

        _ballSwipeListener = _activeBall.OnSwipeReceived; // cache ball's swipe function
        _swipeHandler.OnSwipe.AddListener(_ballSwipeListener); 
        _activeBall.OnBallDestroyed += HandleBallDestroyed;
    }

    private void PlaceMonster(Pose pose)
    {
        int index = Random.Range(0, _placeableMonsters.Length);
        GameObject go = Instantiate(_placeableMonsters[index], pose.position, pose.rotation);

        MonsterObject monster = go.GetComponent<MonsterObject>() ?? go.AddComponent<MonsterObject>();
        monster.OnCaptured += HandleMonsterCaptured;

        _placedMonsters.Add(monster);
    }

    private void UpdatePointer()
    {
        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        bool valid = _arRaycastManager.Raycast(screenCenter, _arHits, TrackableType.PlaneWithinPolygon);
        _pointerPrefab.SetActive(valid);
        if (valid)
        {
            Pose pose = _arHits[0].pose;
            _pointerPrefab.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }

    private void HandleMonsterCaptured(MonsterObject monster)
    {
        _placedMonsters.Remove(monster);
        monster.OnCaptured -= HandleMonsterCaptured;
        OnMonsterCaptured?.Invoke();
    }

    private void HandleBallDestroyed()
    {
        if (_ballSwipeListener != null)
        {
            _swipeHandler.OnSwipe.RemoveListener(_ballSwipeListener); // remove cached ball function
            _ballSwipeListener = null; // remove function pointer
        }

        _activeBall = null;
    }
}