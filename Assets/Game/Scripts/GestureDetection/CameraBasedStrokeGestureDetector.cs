using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;
using System.IO;
using UnityEngine.Events;
using System.Linq;
using System;
using static Oculus.Interaction.PoseDetection.JointVelocityActiveState;
using Oculus.Interaction;

public class CameraBasedStrokeGestureDetector : MonoBehaviour, IActiveState
{
    // TODO add options to save and detect for  camerabased detectors

    public enum ActiveStateGroupLogicOperator
    {
        AND = 0,
        OR = 1,
        XOR = 2
    }

    [Serializable]
    public struct StrokeGestureState
    {
        public PointToPlaneConverter.DetectionPlane CurrentDetectionPlane;
        public string GestureName;
        public bool IsRecognised;

    }

    // Setup

    [SerializeField]
    private PointToPlaneConverter pointToPlaneConverter;

    // Public variables


    public List<StrokeGestureState> StrokeGesturesToRecognize;
    public ActiveStateGroupLogicOperator LogicOperator;
    public float RecognitionThreshold = 0.8f;
    public float ActivationDuration = 1f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecogniseGesture;

    // Private variables

    private List<Gesture> trainingSet= new List<Gesture>();
    private List<bool> gestureResults = new List<bool>();

    private bool currentActiveState = false;
    private float currentActiveTimer = 0f;

    public bool Active
    {
        get
        {
            if (!isActiveAndEnabled)
            {
                return false;
            }

            return currentActiveState;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("StrokeGestures/");
        foreach (TextAsset gestureXml in gesturesXml)
        {
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        }
        // Get all local gestures
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (var file in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(file));
        }
        // check if the gestures in  the strokes to recognise are found beforehand
        if(!pointToPlaneConverter)
        {
            Debug.LogWarning("No converter assigned to " + gameObject.name);
        }
    }

    void Update()
    {
        if(currentActiveTimer > 0f)
        {
            currentActiveTimer -= Time.deltaTime;
            if(currentActiveTimer <= 0f)
            {
                currentActiveState = false;
            }
        }
    }


    public void RecogniseGestures()
    {
        if (!pointToPlaneConverter)
        {
            return;
        }

        gestureResults.Clear();

        CheckAllGestrures();
    }

    void CheckAllGestrures()
    {
        // go over all gestures
        foreach (StrokeGestureState state in StrokeGesturesToRecognize)
        {
            // get the points from the correct plane
            PDollarGestureRecognizer.Point[] currentPoints = GetCurrentPoints(state.CurrentDetectionPlane);

            // create a gesture from the point array
            PDollarGestureRecognizer.Gesture currentGesture = new PDollarGestureRecognizer.Gesture(currentPoints);

            bool isRecognised = IsGestureDetected(currentGesture, state);

            // Check AND operator

            if (LogicOperator == ActiveStateGroupLogicOperator.AND &&
                isRecognised != state.IsRecognised)
            {
                //Debug.Log("Detection failed [AND]");
                return;
            }

            // Check OR operator

            if (LogicOperator == ActiveStateGroupLogicOperator.OR &&
                isRecognised == state.IsRecognised)
            {
                HandleSuccessfullRecognition();
                return;
            }
            gestureResults.Add(isRecognised);
        }

        HandleRemainingEndResult();
    }

    PDollarGestureRecognizer.Point[] GetCurrentPoints(PointToPlaneConverter.DetectionPlane CurrentDetectionPlane)
    {
        PDollarGestureRecognizer.Point[] currentPoints;
        switch (CurrentDetectionPlane)
        {
            case PointToPlaneConverter.DetectionPlane.FrontPlane:
                currentPoints = pointToPlaneConverter.GetFrontPlanePoints();
                break;
            case PointToPlaneConverter.DetectionPlane.SidePlane:
                currentPoints = pointToPlaneConverter.GetSidePlanePoints();
                break;
            case PointToPlaneConverter.DetectionPlane.GroundPlane:
                currentPoints = pointToPlaneConverter.GetGroundPlanePoints();
                break;
            case PointToPlaneConverter.DetectionPlane.ScreenSpace:
                currentPoints = pointToPlaneConverter.GetScreenSpacePoints();
                break;
            default:
                currentPoints = pointToPlaneConverter.GetScreenSpacePoints();
                break;
        }
        return currentPoints;
    }

    bool IsGestureDetected(PDollarGestureRecognizer.Gesture CurrentGesture, StrokeGestureState State)
    {
        // if there is no gesture inserted, simply go trough the full database
        if(State.GestureName == "")
        {
            return IsGestureRecognised(CurrentGesture);
        }

        Gesture currentRef = GetGestureOfName(State.GestureName);
        if (currentRef == null)
        {
            Debug.Log("Gesture called " + State.GestureName + "Does not exist");
            return false;
        }

        Debug.Log("[" + gameObject.name + "] Testing " + State.GestureName + " in " + State.CurrentDetectionPlane.ToString());
        return IsSpecificGestureRecognized(CurrentGesture, currentRef);
    }

    void HandleRemainingEndResult()
    {
        // handle the end result after we went trough all gestures 
        // in case of AND, it means that all of them were true
        // in case of OR, it means none of them were true
        // XOR has to look at all results to see of only one was true

        switch (LogicOperator)
        {
            case ActiveStateGroupLogicOperator.AND:
                HandleSuccessfullRecognition();
                break;
            case ActiveStateGroupLogicOperator.OR:
                //Debug.Log("Detection failed [OR]");
                break;
            case ActiveStateGroupLogicOperator.XOR:
                bool foundRecognised = false;
                foreach (bool isRecognised in gestureResults)
                {
                    if (isRecognised)
                    {
                        if (foundRecognised)
                        {
                            //Debug.Log("Detection failed [XOR]");
                            return;
                        }
                        foundRecognised = true;
                    }
                }
                if (foundRecognised)
                {
                    HandleSuccessfullRecognition();
                }
                break;
            default:
                break;
        }
    }

    void HandleSuccessfullRecognition()
    {
        Debug.Log("Detection Successfull\n" + gameObject.name);
        currentActiveState = true;
        currentActiveTimer = ActivationDuration;
        OnRecogniseGesture.Invoke(gameObject.name);
    }



    // go trough the whole database
    bool IsGestureRecognised(PDollarGestureRecognizer.Gesture NewGesture)
    {
        Result result = PointCloudRecognizer.Classify(NewGesture, trainingSet.ToArray());
       
        if (result.Score > RecognitionThreshold)
        {
            Debug.Log("Detected " + result.GestureClass + " " + result.Score);
            return true;
        }
        Debug.Log("Detection failed" + result.GestureClass + " " + result.Score);
        return false;
    }

    // compare it to a specific gesture
    bool IsSpecificGestureRecognized(PDollarGestureRecognizer.Gesture NewGesture, PDollarGestureRecognizer.Gesture reference)
    {
        Gesture[] gestureSingleTraining = new Gesture[1];
        gestureSingleTraining[0] = reference;

        Result result = PointCloudRecognizer.Classify(NewGesture, gestureSingleTraining);

        if (result.Score > RecognitionThreshold)
        {
            Debug.Log("Detected " + result.GestureClass + " " + result.Score);
            return true;
        }
        Debug.Log("Detection failed" + result.GestureClass + " " + result.Score);
        return false;
    }

    Gesture GetGestureOfName(string name)
    {
        Gesture currentGesture = null;
        // loop over the training set to find the correct gesture
        foreach(Gesture gesture in trainingSet)
        {
            if(gesture.Name == name)
            {
                return gesture;
            }
        }

        return currentGesture;
    }
}
