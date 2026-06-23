using UnityEngine;

public class MonsterObject : MonoBehaviour
{
    public event System.Action<MonsterObject> OnCaptured;

    private bool _isDone = false;

    public void Capture()
    {
        if (_isDone) return;
        _isDone = true;
        OnCaptured?.Invoke(this);
        Destroy(gameObject);
    }
}
