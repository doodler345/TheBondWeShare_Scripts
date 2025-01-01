using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class RopeController : MonoBehaviour
{
    [SerializeField] ObiParticleAttachment _startAttachment, _endAttachement;
    [SerializeField] float _minLength = 2.0f, _maxLength = 5.0f;

    [SerializeField] float _maxPlayerDistance = 2f; 
    private float _initMaxDistance;
    [SerializeField] float _getBackBuffer = 5f;
    private float _ropeOffsetZ;
    private Transform _playerMid;
    public bool limitExeeded;
    StageController _stageController;
    PlayerMovement _p1, _p2;
    private bool _ropeCut;


    [SerializeField] bool _tearingPossible;
    ObiRopeCursor _cursor;
    ObiRope _rope;
    ObiParticleAttachment.AttachmentType _type;

    private void Awake()
    {
        _cursor = GetComponent<ObiRopeCursor>();
        _rope = GetComponent<ObiRope>();
        _playerMid = StageController.instance.playerMid;
        _ropeOffsetZ = transform.localPosition.z;
        _initMaxDistance = _maxPlayerDistance;
    }

    private void Start()
    {
        _stageController = StageController.instance;
        _p1 = _stageController.player1.GetComponent<PlayerMovement>();
        _p2 = _stageController.player2.GetComponent<PlayerMovement>();

        ResetLength();
    }

    private void Update()
    {
        if (!_ropeCut) CheckPlayerDistance();
    }

    private void CheckPlayerDistance()
    {
        float playerDistSqr = StageController.instance.currentPlayerDistance.sqrMagnitude;
        if (playerDistSqr >= (_maxPlayerDistance * _maxPlayerDistance) && !limitExeeded)
        {
            limitExeeded = true;

            _p1.ObiKinematicManagement(false);
            _p2.ObiKinematicManagement(false); 

            _p1.RopeLimitExeeded();
            _p2.RopeLimitExeeded();
        }
        else if (limitExeeded)

        {
            if (playerDistSqr < (_maxPlayerDistance * _maxPlayerDistance) - _getBackBuffer)
            {
                limitExeeded = false;
                _p1.ObiKinematicManagement(true);
                _p2.ObiKinematicManagement(true);
            }
            else
            {
                _p1.ObiKinematicManagement(false);
                _p2.ObiKinematicManagement(false);
            }
        }
    }

    public void SetMaxPlayerDistance(int multiply)
    {
        _maxPlayerDistance = _initMaxDistance * multiply;
    }

    public void StaticDynamicSwitch(bool isStatic, int playerID)
    {
        switch (playerID)
        {
            case 0:
                if (isStatic) _startAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
                else _startAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
                break;
            case 1:
                if (isStatic) _endAttachement.attachmentType = ObiParticleAttachment.AttachmentType.Static;
                else _endAttachement.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
                break;
            default:
                break;
        }

        if (_startAttachment.attachmentType != _endAttachement.attachmentType)
        {
            if (_startAttachment.attachmentType == ObiParticleAttachment.AttachmentType.Static)
            {
                _cursor.direction = true;
                _cursor.cursorMu = 0;
                _cursor.sourceMu = 1;
            }
            else
            {
                _cursor.direction = false;
                _cursor.cursorMu = 1;
                _cursor.sourceMu = 0;
            }
        }

    }

    public void Crane(bool up)
    {
        if (_startAttachment.attachmentType == _endAttachement.attachmentType) return;

        if (up)
        {
            if (_rope.restLength > _minLength)
                _cursor.ChangeLength(_rope.restLength - 1f * Time.deltaTime);
        }
        else
        {
            if (_rope.restLength < _maxLength)
                _cursor.ChangeLength(_rope.restLength + 1f * Time.deltaTime);
        }
    }

    public void ResetLength()
    {
        StartCoroutine(EnableTearing(false, 0));
        _cursor.ChangeLength(_maxLength);
        StartCoroutine(EnableTearing(true, 0.2f));
    }

    public IEnumerator EnableTearing(bool isActive, float delay)
    {
        if (!_tearingPossible) yield break;

        yield return new WaitForSeconds(delay);
       _rope.tearingEnabled = isActive;
    }

    public IEnumerator CutRope(float timeTillDisappear)
    {
        _ropeCut = true;
        int midIndex = _rope.elements.Count / 2;
        _rope.Tear(_rope.elements[midIndex]);
        _rope.RebuildConstraintsFromElements();
        StageController.instance.isUnbound = true;

        yield return new WaitForSeconds(timeTillDisappear);

        if (this != null) Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        if(_playerMid) Gizmos.DrawWireSphere(_playerMid.position + new Vector3 (0,0,_ropeOffsetZ), _maxPlayerDistance / 2);
    }
}
