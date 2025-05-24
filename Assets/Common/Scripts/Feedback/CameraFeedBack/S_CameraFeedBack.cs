using System;
using UnityEngine;
using Cinemachine;

public class S_CameraFeedback : MonoBehaviour
{
    [Header("References")]
    public CinemachineVirtualCamera _cinemachineVirtualCamera;
    private CinemachineInputProvider _cinemachineInputProvider;
    private CinemachineImpulseSource _impulseSource;
    private CinemachineRecomposer _recomposer;

    [Header("Modules")]
    [SerializeField] private S_CameraMovementFeedback _movementFeedback;
    [SerializeField] private S_CameraCACFeedback _cacFeedback;
    [SerializeField] private S_CameraGroundPoundFeedback _groundPoundFeedback;
    [SerializeField] private S_CameraSprintFeedback _sprintFeedback;

    // Caches last frame’s enable-state to avoid unnecessary writes
    private bool _lastCameraEnabled = true;

    private void Awake()
    {
        // Ensure we have a virtual camera; disable script if missing
        if (_cinemachineVirtualCamera == null)
            _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (_cinemachineVirtualCamera == null)
        {
            Debug.LogError("[S_CameraFeedback] No CinemachineVirtualCamera found in scene.");
            enabled = false;
            return;
        }

        // Defensive component fetches
        _cinemachineVirtualCamera.TryGetComponent(out _cinemachineInputProvider);
        _cinemachineVirtualCamera.TryGetComponent(out _impulseSource);
        _cinemachineVirtualCamera.TryGetComponent(out _recomposer);
    }

    private void Start()
    {
        // Setup child modules only when their dependencies exist
        if (_sprintFeedback != null)
            _sprintFeedback.Setup(_cinemachineVirtualCamera);

        if (_movementFeedback != null && _recomposer != null)
            _movementFeedback.Setup(_cinemachineVirtualCamera, _recomposer);

        if (_cacFeedback != null && _impulseSource != null)
            _cacFeedback.Setup(_cinemachineVirtualCamera, _impulseSource);

        if (_groundPoundFeedback != null && _impulseSource != null)
            _groundPoundFeedback.Setup(_cinemachineVirtualCamera, _impulseSource);

        SubscribeEvents(true);
    }

    private void Update()
    {
        bool shouldEnable = S_PlayerStateObserver.Instance?.LastGroundPoundState == null;

        // Write to the input provider only when state actually changes
        if (shouldEnable != _lastCameraEnabled)
        {
            EnableCamera(shouldEnable);
            _lastCameraEnabled = shouldEnable;
        }
    }

    private void EnableCamera(bool state)
    {
        if (_cinemachineInputProvider != null)
            _cinemachineInputProvider.enabled = state;
    }

    /// <summary>
    /// Centralised event subscription / unsubscription.
    /// </summary>
    private void SubscribeEvents(bool subscribe)
    {
        if (S_PlayerStateObserver.Instance == null) return;

        if (subscribe)
        {
            if (_movementFeedback != null)
                S_PlayerStateObserver.Instance.OnMoveStateEvent += _movementFeedback.ReceiveMoveEvent;

            if (_cacFeedback != null)
                S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += _cacFeedback.ReceiveMeleeAttackEvent;

            if (_groundPoundFeedback != null)
                S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += _groundPoundFeedback.ReceiveGroundPoundEvent;

            if (_sprintFeedback != null)
                S_PlayerStateObserver.Instance.OnSprintStateEvent += _sprintFeedback.ReceiveSprintEvent;
        }
        else
        {
            if (_movementFeedback != null)
                S_PlayerStateObserver.Instance.OnMoveStateEvent -= _movementFeedback.ReceiveMoveEvent;

            if (_cacFeedback != null)
                S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent -= _cacFeedback.ReceiveMeleeAttackEvent;

            if (_groundPoundFeedback != null)
                S_PlayerStateObserver.Instance.OnGroundPoundStateEvent -= _groundPoundFeedback.ReceiveGroundPoundEvent;

            if (_sprintFeedback != null)
                S_PlayerStateObserver.Instance.OnSprintStateEvent -= _sprintFeedback.ReceiveSprintEvent;
        }
    }

    private void OnDisable() => SubscribeEvents(false);
    private void OnDestroy() => SubscribeEvents(false); // Handles object destruction as well
}
