using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

public class BaseMovementDetector : MonoBehaviour
{
    public PointVisualiserManager DebugPointVisualiserManager;
    public XRNode InputSource;
    public Transform ControllerMovementSource;
    public Transform HandMovementSource;
    public float PositionThresholdDistanceSqrd = 0.025f;

    [System.Serializable]
    public class UnityPointListEvent : UnityEvent<List<Vector3>> { }
    public UnityPointListEvent OnEndLine;

    public UnityEvent OnUpdateMovement;



    private Transform currentMovementSource;
    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();
    private List<Vector2> screenSpacePositionList = new List<Vector2>();
    private bool PrevButtonPress = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // TODO: Create timer for end -> this way accidental ends are filtered out
    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdateMovement();
    }


    void HandleInput()
    {
        bool isPressed;
        var device = InputDevices.GetDeviceAtXRNode(InputSource);
        device.TryGetFeatureValue(CommonUsages.triggerButton, out isPressed);

        // check if a change happened
        if (isPressed != PrevButtonPress)
        {
            if (isPressed)
            {
                HandleTriggerDown();
            }
            else
            {
                HandleTriggerUp();
            }
        }


        PrevButtonPress = isPressed;

    }


    void HandleTriggerDown()
    {
        currentMovementSource = ControllerMovementSource;
        StartMovement();
    }

    public void StartGestureMovement()
    {
        currentMovementSource = HandMovementSource;
        StartMovement();
    }

    void StartMovement()
    {
        isMoving = true;
        positionList.Clear();
        screenSpacePositionList.Clear();
        // insert the first position
        positionList.Add(currentMovementSource.position);
        screenSpacePositionList.Add(Camera.main.WorldToScreenPoint(currentMovementSource.position));
        AddDebugPoint();
    }


    void HandleTriggerUp()
    {
        EndMovement();
    }

    public void EndGestureMovement()
    {
        EndMovement();
    }

    void EndMovement()
    {

        //Debug.Log("EndMovement");
        isMoving = false;
        RemoveDebugPoints();

        Debug.Log("Invoke On End line");
        OnEndLine.Invoke(positionList);
    }


    void UpdateMovement()
    {
        if (!isMoving)
        {
            return;
        }

        Vector3 lastPos = positionList[positionList.Count - 1];
        Vector3 currentPos = currentMovementSource.position;
        Vector3 movement = lastPos - currentPos;
        if (movement.sqrMagnitude > PositionThresholdDistanceSqrd)
        {
            positionList.Add(currentMovementSource.position);
            screenSpacePositionList.Add(Camera.main.WorldToScreenPoint(currentMovementSource.position));
            AddDebugPoint();
            OnUpdateMovement.Invoke();
        }
    }


    void AddDebugPoint()
    {
        if (DebugPointVisualiserManager)
        {
            DebugPointVisualiserManager.AddPoint(currentMovementSource.position);
        }
    }

    void RemoveDebugPoints()
    {
        if (DebugPointVisualiserManager)
        {
            DebugPointVisualiserManager.RemoveAllPoints();
        }
    }

    public int GetPositionListCount()
    {
        return positionList.Count;
    }

    public Vector3 GetCurrentDirection()
    {
        if (positionList.Count < 2)
        {
            return Vector3.zero;
        }
        return (positionList[positionList.Count - 2] - positionList[positionList.Count - 1]).normalized ;
    }

    public Vector3 GetCurrentMovementPosition()
    {
        if(positionList.Count == 0)
        {
            return Vector3.zero ;
        }

        return positionList[positionList.Count - 1] ;
    }
    public Vector3 GetPreviousMovementPosition()
    {
        if(positionList.Count < 2)
        {
            return Vector3.zero;
        }

        return positionList[positionList.Count - 2] ;
    }
    public Vector2 GetCurrentDirectionSS()
    {
        if (screenSpacePositionList.Count < 2)
        {
            return Vector2.zero;
        }
        return (screenSpacePositionList[screenSpacePositionList.Count - 2] - screenSpacePositionList[screenSpacePositionList.Count - 1]).normalized;
    }
    public Vector2 GetCurrentMovementPositionSS()
    {
        if (screenSpacePositionList.Count == 0)
        {
            return Vector2.zero;
        }

        return screenSpacePositionList[screenSpacePositionList.Count - 1];
    }
    public Vector2 GetPreviousMovementPositionSS()
    {
        if(screenSpacePositionList.Count < 2)
        {
            return Vector2.zero;
        }

        return screenSpacePositionList[screenSpacePositionList.Count - 2] ;
    }


}
