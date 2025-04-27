using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

#region —— Data Structures ——
public enum TweenType {
    Move,
    Scale,
    Rotate,
    ContinuousRotate
}

[Serializable]
public class TweenData {
    public TweenType type;
    
    [Tooltip("Additional delay (seconds) relative to TweenItem.startDelay")]
    public float startDelay;
    
    [Tooltip("Duration of this tween (seconds)")]
    public float duration;
    
    [Tooltip("Easing function for this tween")]
    public Ease ease = Ease.Linear;

    // Move parameters
    [Tooltip("Target position (local or world, depending on useLocalMove)")]
    public Vector3 targetPosition;
    [Tooltip("Use localPosition instead of world position")]
    public bool useLocalMove = true;

    // Scale parameters
    [Tooltip("Scale multiplier (initial localScale * multiplier)")]
    public float scaleMultiplier = 1f;

    // Rotate parameters
    [Tooltip("Target rotation in Euler angles (degrees)")]
    public Vector3 rotationEuler;
    [Tooltip("Rotation mode (Fast, FastBeyond360, etc.)")]
    public RotateMode rotateMode = RotateMode.Fast;

    // ContinuousRotate parameters
    [Tooltip("Axis to rotate around")]
    public Vector3 rotateAxis = Vector3.up;
    [Tooltip("Rotation speed in degrees per second")]
    public float rotateSpeed = 90f;
}

[Serializable]
public class TweenItem {
    [Tooltip("The GameObject this item targets")]
    public GameObject target;
    
    [Tooltip("Overall delay (seconds) before this item's animations start")]
    public float startDelay;
    
    [Tooltip("List of individual tween configurations")]
    public List<TweenData> tweens = new List<TweenData>();
}
#endregion

public class S_DotweenPlayer : MonoBehaviour {
    [Tooltip("All animation items to play")]
    public List<TweenItem> items = new List<TweenItem>();

    [Tooltip("Play automatically on Start()")]
    public bool playOnStart = true;

    private List<Sequence> sequences = new List<Sequence>();

    // Cached initial states
    private Dictionary<Transform, Vector3> initPos = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> initRot = new Dictionary<Transform, Quaternion>();
    private Dictionary<Transform, Vector3> initScale = new Dictionary<Transform, Vector3>();
    private bool hasCached = false;

    void Awake() {
        CacheInitialStates();
    }

    void Start() {
        if (playOnStart) Play();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Play();
        }
    }

    /// <summary>
    /// Cache the initial transform of all target objects.
    /// </summary>
    private void CacheInitialStates() {
        if (hasCached) return;
        foreach (var item in items) {
            if (item.target == null) continue;
            var tr = item.target.transform;
            if (!initPos.ContainsKey(tr)) {
                initPos[tr] = tr.localPosition;
                initRot[tr] = tr.localRotation;
                initScale[tr] = tr.localScale;
            }
        }
        hasCached = true;
    }

    /// <summary>
    /// Play all configured animations.
    /// Each call will:
    ///   1) Kill any existing tweens
    ///   2) Reset all targets to their initial states
    ///   3) Create and play new Sequences
    /// </summary>
    public void Play() {
        // Ensure initial states are cached
        CacheInitialStates();

        // 1) Kill old tweens
        foreach (var seq in sequences) {
            if (seq.IsActive()) seq.Kill();
        }
        sequences.Clear();

        // 2) Reset all targets to their cached initial transforms
        foreach (var kv in initPos) {
            var tr = kv.Key;
            tr.localPosition = initPos[tr];
            tr.localRotation = initRot[tr];
            tr.localScale    = initScale[tr];
        }

        // 3) Create and play new Sequences
        foreach (var item in items) {
            if (item.target == null) continue;

            var tr = item.target.transform;
            var seq = DOTween.Sequence().SetAutoKill(false);

            foreach (var td in item.tweens) {
                float t0 = item.startDelay + td.startDelay;
                Tween t = null;

                switch (td.type) {
                    case TweenType.Move:
                        t = td.useLocalMove
                            ? tr.DOLocalMove(td.targetPosition, td.duration)
                            : tr.DOMove(td.targetPosition, td.duration);
                        break;

                    case TweenType.Scale:
                        Vector3 baseScale = initScale[tr];
                        t = tr.DOScale(baseScale * td.scaleMultiplier, td.duration);
                        break;

                    case TweenType.Rotate:
                        t = tr.DORotate(td.rotationEuler, td.duration, td.rotateMode);
                        break;

                    case TweenType.ContinuousRotate:
                        float loopDur = 360f / Mathf.Max(td.rotateSpeed, 1e-3f);
                        t = tr.DORotate(td.rotateAxis.normalized * 360f, loopDur, RotateMode.FastBeyond360)
                              .SetRelative()
                              .SetLoops(-1, LoopType.Incremental);
                        break;
                }

                if (t != null) {
                    t.SetEase(td.ease)
                     .SetDelay(t0);
                    seq.Insert(t0, t);
                }
            }

            seq.Play();
            sequences.Add(seq);
        }
    }
}
