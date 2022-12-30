using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PDollarMovementDetector : MonoBehaviour
{
    public XRNode inputSource;
    public float inputThreshold = 0f;
    public Transform movementSource;

    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();

    public float PositionThresholdDistanceSqrd = 0.025f;
    public GameObject DebugCubePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool isPressed;
        var device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.triggerButton, out isPressed);

        if (!isMoving && isPressed)
        {
            StartMovement();
        }
        else if (isMoving && !isPressed)
        {
            EndMovement();
        }
        else if (isMoving && isPressed)
        {
            UpdateMovement();
        }
    }

    void StartMovement()
    {
        Debug.Log("StartMovement");
        isMoving = true;
        positionList.Clear();
        // insert the first position
        positionList.Add(movementSource.position);
        AddDebugCube();
    }

        void EndMovement()
    {

        Debug.Log("EndMovement");
        isMoving = false;
    }

    void UpdateMovement()
    {

        Vector3 lastPos = positionList[positionList.Count-1];
        Vector3 currentPos = movementSource.position;
        Vector3 movement = lastPos - currentPos;
        if (movement.sqrMagnitude > PositionThresholdDistanceSqrd)
        {
            positionList.Add(movementSource.position);
            AddDebugCube();
        }
    }

    void AddDebugCube()
    {
        if (DebugCubePrefab)
        {
            Debug.Log(movementSource.position.ToString());
            Destroy(Instantiate(DebugCubePrefab, movementSource.position, Quaternion.identity), 3f);

        }
    }
}
