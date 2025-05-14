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

    private void Start()
    {
        if (_cinemachineVirtualCamera == null) _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _cinemachineInputProvider = _cinemachineVirtualCamera.GetComponent<CinemachineInputProvider>();
        _impulseSource = _cinemachineVirtualCamera.GetComponent<CinemachineImpulseSource>();
        _recomposer = _cinemachineVirtualCamera.GetComponent<CinemachineRecomposer>();
        
        if (_sprintFeedback != null) _sprintFeedback.Setup(_cinemachineVirtualCamera);
        if (_movementFeedback != null) _movementFeedback.Setup(_cinemachineVirtualCamera, _recomposer);
        if (_cacFeedback != null) _cacFeedback.Setup(_cinemachineVirtualCamera, _impulseSource);
        if (_groundPoundFeedback != null) _groundPoundFeedback.Setup(_cinemachineVirtualCamera, _impulseSource);

        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent += _movementFeedback.ReceiveMoveEvent;
            S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += _cacFeedback.ReceiveMeleeAttackEvent;
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += _groundPoundFeedback.ReceiveGroundPoundEvent;
            S_PlayerStateObserver.Instance.OnSprintStateEvent += _sprintFeedback.ReceiveSprintEvent;
        }
    }

    private void Update()
    {
        EnableCamera(S_PlayerStateObserver.Instance?.LastGroundPoundState == null);
    }

    private void EnableCamera(bool state)
    {
        if (_cinemachineInputProvider != null)
            _cinemachineInputProvider.enabled = state;
    }

    private void OnDisable()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent -= _movementFeedback.ReceiveMoveEvent;
            S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent -= _cacFeedback.ReceiveMeleeAttackEvent;
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent -= _groundPoundFeedback.ReceiveGroundPoundEvent;
            S_PlayerStateObserver.Instance.OnSprintStateEvent -= _sprintFeedback.ReceiveSprintEvent;
        }
    }
} 