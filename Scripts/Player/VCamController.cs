using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VCamController : MonoBehaviour
{
    [Header("Camera_LevelOverview")]
    [SerializeField] bool _useOverview;
    [SerializeField] float _waitTillOverviewDuration = 1;
    [SerializeField] float _overviewDuration = 3;
    [SerializeField] CinemachineVirtualCamera vCam_LevelOverview;

    [Header("Camera_Player")]
    public Transform playerMid;
    public Transform cameraTarget;
    [SerializeField] Vector3 _offset;
    [SerializeField] Vector2 _zoomFactor;
    
    [Header("Camera_MovieMode")]
    [SerializeField] bool _useMovieMode;
    [SerializeField] CinemachineVirtualCamera _movieCam;
    [SerializeField] Vector3[] _moviePosBegin, _moviePosEnd;
    [SerializeField] Vector3[] _movieRotBegin, _movieRotEnd;
    [SerializeField] float[] _movieDuration;
    int _movieIndex = 0;
    Tween _movieCamMove, _movieCamRotate;

    private float _sqrDistX, _sqrDistY;
    private float _ratio;

    private void Awake()
    {
        _ratio = 16f / 9f;
    }

    private void Start()
    {
        if (_useMovieMode)
        {
            _movieCam.Priority = 75;
            LoopingMovieMode();
        }

        else if (_useOverview)
        {
            StartCoroutine(nameof(StartLevelOverview));
        }
    }

    private void Update()
    {
        if (_useMovieMode)
        {
            if (Input.GetKeyDown("."))
            {
                _movieIndex++;
                _movieIndex %= _moviePosBegin.Length;
                _movieCamMove.Kill();
                _movieCamRotate.Kill();

                LoopingMovieMode();
            }

            return;
        }

        Vector2 playerDistance = StageController.instance.currentPlayerDistance;
        _sqrDistX = Mathf.Abs(playerDistance.x);
        _sqrDistY = Mathf.Abs(playerDistance.y);

        cameraTarget.localPosition = new Vector3(0, 0, -((_sqrDistX * _ratio) * _zoomFactor.x + _sqrDistY * _zoomFactor.y)) + _offset;

        /*
        if (Input.GetKeyDown("v"))
            StartCoroutine(nameof(ShowLevelOverview));
        */
    }

    private IEnumerator StartLevelOverview()
    {
        yield return new WaitForSeconds(_waitTillOverviewDuration);
        vCam_LevelOverview.Priority = 50;
        yield return new WaitForSeconds(_overviewDuration);
        vCam_LevelOverview.Priority = 0;
    }

    public void ToggleLevelOverview(bool useOverviewCam)
    {
        if (useOverviewCam)
        {
            vCam_LevelOverview.Priority = 100;
        }
        else
        {
            vCam_LevelOverview.Priority = 0;
        }
    }

    private void LoopingMovieMode()
    {
        _movieCam.transform.position = _moviePosBegin[_movieIndex];
        _movieCam.transform.rotation = Quaternion.Euler(_movieRotBegin[_movieIndex]);

        _movieCamMove =_movieCam.transform.DOMove(_moviePosEnd[_movieIndex], _movieDuration[_movieIndex]);
        _movieCamRotate =_movieCam.transform.DORotate(_movieRotEnd[_movieIndex], _movieDuration[_movieIndex]).OnComplete(LoopingMovieMode);
    }
}
