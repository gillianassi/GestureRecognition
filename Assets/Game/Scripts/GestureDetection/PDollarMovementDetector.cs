using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PDollarMovementDetector : MonoBehaviour
{
    public XRNode inputSource;
    public float inputThreshold = 0f;
    public Transform ControllerMovementSource;
    public Transform HandMovementSource;


    public float PositionThresholdDistanceSqrd = 0.025f;
    public GameObject DebugCubePrefab;

    private Transform currentMovementSource;

    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();
    private bool PrevButtonPress = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdateMovement();
    }

    void HandleInput()
    {
        bool isPressed;
        var device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.triggerButton, out isPressed);

        // check if a change happened
        if(isPressed != PrevButtonPress)
        {
            if(isPressed)
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
        Debug.Log("StartMovement");
        isMoving = true;
        positionList.Clear();
        // insert the first position
        positionList.Add(currentMovementSource.position);
        AddDebugCube();
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

        Debug.Log("EndMovement");
        isMoving = false;
    }

    void UpdateMovement()
    {
        if(!isMoving)
        {
            return;
        }

        Vector3 lastPos = positionList[positionList.Count-1];
        Vector3 currentPos = currentMovementSource.position;
        Vector3 movement = lastPos - currentPos;
        if (movement.sqrMagnitude > PositionThresholdDistanceSqrd)
        {
            positionList.Add(currentMovementSource.position);
            AddDebugCube();
        }
    }

    void AddDebugCube()
    {
        if (DebugCubePrefab)
        {
            //Debug.Log(currentMovementSource.position.ToString());
            Destroy(Instantiate(DebugCubePrefab, currentMovementSource.position, Quaternion.identity), 3f);

        }
    }
}
