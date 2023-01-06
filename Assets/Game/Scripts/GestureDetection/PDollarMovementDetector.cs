using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;
using System.IO;
using UnityEngine.Events;

public class PDollarMovementDetector : MonoBehaviour
{
    public XRNode inputSource;
    public float inputThreshold = 0f;
    public Transform ControllerMovementSource;
    public Transform HandMovementSource;


    public float PositionThresholdDistanceSqrd = 0.025f;
    public GameObject DebugCubePrefab;

    public bool creationMode = false;
    public string newGestureName = "NewGesture";

    public float RecognitionThreshold = 0.9f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecogniseGesture;


    private List<Gesture> trainingSet= new List<Gesture>();
    private Transform currentMovementSource;

    private bool isMoving = false;
    private List<Vector3> positionList = new List<Vector3>();
    private bool PrevButtonPress = false;

    // Start is called before the first frame update
    void Start()
    {
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (var file in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(file));
        }
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
        //Debug.Log("StartMovement");
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

        //Debug.Log("EndMovement");
        isMoving = false;

        ConvertToPDollarGesture();
    }

    void ConvertToPDollarGesture()
    {
        // POINT is a PDollarGestureRecognizer class
        PDollarGestureRecognizer.Point[] points = new PDollarGestureRecognizer.Point[positionList.Count];

        for (int i = 0; i < positionList.Count; i++)
        {
           Vector2 screenPosition = Camera.main.WorldToScreenPoint(positionList[i]);
            // stroke index is 0 -> only 1 stroke
           points[i] = new Point(screenPosition.x, screenPosition.y, 0);
        }

        // PDollar Gresture
        PDollarGestureRecognizer.Gesture currentGesture = new PDollarGestureRecognizer.Gesture(points);
        // add new gesture to training set
        if (creationMode)
        {
            Debug.Log("Gesture created");
            currentGesture.Name = newGestureName;
            trainingSet.Add(currentGesture);

            string filename = Application.persistentDataPath + "/" + currentGesture.Name + ".xml";
            GestureIO.WriteGesture(points, currentGesture.Name, filename);
        }
        // recognise
        else
        {
            Result result = PointCloudRecognizer.Classify(currentGesture, trainingSet.ToArray());
            Debug.Log(result.GestureClass + result.Score);
            if(result.Score > RecognitionThreshold)
            {
                OnRecogniseGesture.Invoke(result.GestureClass);
            }
        }
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
