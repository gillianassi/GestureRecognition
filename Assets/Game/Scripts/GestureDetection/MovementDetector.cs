using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

public class MovementDetector : MonoBehaviour
{
    public PointVisualiserManager DebugPointVisualiserManager;
    public XRNode InputSource;
    public Transform ControllerMovementSource;
    public Transform HandMovementSource;
    public float PositionThresholdDistanceSqrd = 0.025f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<List<Vector3>> { }
    public UnityStringEvent OnEndLine;

    private Transform currentMovementSource;
    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();
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
        // insert the first position
        positionList.Add(currentMovementSource.position);
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
            AddDebugPoint();
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
}
