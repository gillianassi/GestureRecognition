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

public class CameraBasedStrokeDetector : MonoBehaviour
{
    // TODO add options to save and detect for  camerabased detectors
    // TODO Save strokes somewhere else, check the complex gestures elswere
    // TODO Make this an activestate
    // TODO create the timer
    public enum DetectionPlane
    {
        FrontPlane = 0,
        SidePlane = 1,
        GroundPlane = 2,
        ScreenSpace = 3,
    }

    public enum ActiveStateGroupLogicOperator
    {
        AND = 0,
        OR = 1,
        XOR = 2
    }

    [Serializable]
    public struct StrokeGestureState
    {
        public DetectionPlane CurrentDetectionPlane;
        public string GestureName;
        public bool IsRecognised;

    }

    // Public variables

    public PointVisualiserManager DebugPointVisualiserManager;

    public List<StrokeGestureState> StrokesToRecognize;
    public ActiveStateGroupLogicOperator LogicOperator;
    public float RecognitionThreshold = 0.8f;
    public float ActivationDuration = 1f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecogniseGesture;

    // Private variables

    private List<Gesture> trainingSet= new List<Gesture>();

    private int currentStroke = 0;
    private List<List<Vector3>> strokeList = new List<List<Vector3>>();

    private PDollarGestureRecognizer.Point[] frontPoints;
    private PDollarGestureRecognizer.Point[] sidePoints;
    private PDollarGestureRecognizer.Point[] groundPoints;
    private PDollarGestureRecognizer.Point[] ssPoints;

    // Start is called before the first frame update
    void Start()
    {
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (var file in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(file));
        }
        // check if the gestures in  the strokes to recognise are found beforehand
    }

    public void AddStroke(List<Vector3> positionList)
    {
        //Debug.Log("adding stroke");
        AddDebugStroke(positionList);
        currentStroke++;
        strokeList.Add(positionList);
    }

    public void ConvertToPDollarGesture()
    {
        // don't if the list is empty
        if (!strokeList.Any())
        {
            return;
        }

        CalculateProjectedPoints();

        CheckAllGestrures();

        EndGesture();
    }

    void CalculateProjectedPoints()
    {
        // get the nr of points in the whole gesture
        int nrOfPoints = 0;
        foreach (var stroke in strokeList)
        {
            nrOfPoints += stroke.Count;
        }

        // Calculate the projected points beforehand

        frontPoints = new PDollarGestureRecognizer.Point[nrOfPoints];
        sidePoints = new PDollarGestureRecognizer.Point[nrOfPoints];
        groundPoints = new PDollarGestureRecognizer.Point[nrOfPoints];
        ssPoints = new PDollarGestureRecognizer.Point[nrOfPoints];

        // TODO don't calculate it for the ones that are not needed especially the screenspace if it's not needed
        // TODO save the cameraToWorld matrix on first stroke or do this already on the add stroke

        int currentPoint = 0;
        // fill the point array per stroke
        for (int i = 0; i < strokeList.Count; i++)
        {
            for (int j = 0; j < strokeList[i].Count; j++)
            {
                // transform the point to camera space
                Vector3 pointCS = Camera.main.worldToCameraMatrix.MultiplyPoint(strokeList[i][j]);

                // Get the points from the desired axis plane
                Vector2 screenPosition = Get2DPointsOnPlane(pointCS, DetectionPlane.FrontPlane);
                frontPoints[currentPoint] = new Point(screenPosition.x, screenPosition.y, i);

                screenPosition = Get2DPointsOnPlane(pointCS, DetectionPlane.SidePlane);
                sidePoints[currentPoint] = new Point(screenPosition.x, screenPosition.y, i);

                screenPosition = Get2DPointsOnPlane(pointCS, DetectionPlane.GroundPlane);
                groundPoints[currentPoint] = new Point(screenPosition.x, screenPosition.y, i);

                screenPosition = Camera.main.WorldToScreenPoint(strokeList[i][j]);
                ssPoints[currentPoint] = new Point(screenPosition.x, screenPosition.y, i);

                // increment the point
                currentPoint++;
            }
        }
    }

    void CheckAllGestrures()
    {
        List<bool> results = new List<bool>();
        // go over all gestures
        foreach (StrokeGestureState state in StrokesToRecognize)
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
                Debug.Log("Detection failed [AND]");
                return;
            }

            // Check OR operator

            if (LogicOperator == ActiveStateGroupLogicOperator.OR &&
                isRecognised == state.IsRecognised)
            {
                HandleSuccessfullRecognition();
                return;
            }
            results.Add(isRecognised);
        }

        // handle the end result

        switch (LogicOperator)
        {
            case ActiveStateGroupLogicOperator.AND:
                HandleSuccessfullRecognition();
                break;
            case ActiveStateGroupLogicOperator.OR:
                Debug.Log("Detection failed [OR]");
                break;
            case ActiveStateGroupLogicOperator.XOR:
                break;
                bool foundRecognised = false;
                foreach (bool isRecognised in results)
                {
                    if (isRecognised)
                    {
                        if (foundRecognised)
                        {
                            Debug.Log("Detection failed [XOR]");
                            return;
                        }
                        foundRecognised = true;
                    }
                }
                if (foundRecognised)
                {
                    HandleSuccessfullRecognition();
                }
            default:
                break;
        }


    }

    PDollarGestureRecognizer.Point[] GetCurrentPoints(DetectionPlane CurrentDetectionPlane)
    {
        PDollarGestureRecognizer.Point[] currentPoints;
        switch (CurrentDetectionPlane)
        {
            case DetectionPlane.FrontPlane:
                currentPoints = frontPoints;
                break;
            case DetectionPlane.SidePlane:
                currentPoints = sidePoints;
                break;
            case DetectionPlane.GroundPlane:
                currentPoints = groundPoints;
                break;
            case DetectionPlane.ScreenSpace:
                currentPoints = ssPoints;
                break;
            default:
                currentPoints = ssPoints;
                break;
        }
        return currentPoints;
    }

    bool IsGestureDetected(PDollarGestureRecognizer.Gesture CurrentGesture, StrokeGestureState State)
    {
        // if there is no gesture inserted, simply go trough the full database
        if(State.GestureName == "")
        {
            return RecogniseGesture(CurrentGesture);
        }

        Gesture currentRef = GetGestureOfName(State.GestureName);
        if (currentRef == null)
        {
            Debug.Log("Gesture called " + State.GestureName + "Does not exist");
            return false;
        }

        
        return RecogniseSpecificGesture(CurrentGesture, currentRef);
    }

    void HandleSuccessfullRecognition()
    {
        Debug.Log("Detection Successfull");
        OnRecogniseGesture.Invoke(gameObject.name);
    }

    void EndGesture()
    {
        strokeList.Clear();
        RemoveDebugPoints();
    }

    // go trough the whole database
    bool RecogniseGesture(PDollarGestureRecognizer.Gesture NewGesture)
    {
        Result result = PointCloudRecognizer.Classify(NewGesture, trainingSet.ToArray());
       
        if (result.Score > RecognitionThreshold)
        {
            Debug.Log("Detected: " + result.GestureClass + " " + result.Score);
            return true;
        }
        return false;
    }

    // compare it to a specific gesture
    bool RecogniseSpecificGesture(PDollarGestureRecognizer.Gesture NewGesture, PDollarGestureRecognizer.Gesture reference)
    {
        Gesture[] gestureSingleTraining = new Gesture[1];
        gestureSingleTraining[0] = reference;

        Result result = PointCloudRecognizer.Classify(NewGesture, gestureSingleTraining);

        if (result.Score > RecognitionThreshold)
        {
            Debug.Log("Detected: " + result.GestureClass + " " + result.Score);
            return true;
        }
        return false;
    }



    void AddDebugStroke(List<Vector3> Points)
    {
        if (DebugPointVisualiserManager)
        {
            foreach (var point in Points)
            {
                DebugPointVisualiserManager.AddPoint(point);
            }
        }
    }

    void RemoveDebugPoints()
    {
        if (DebugPointVisualiserManager)
        {
            DebugPointVisualiserManager.RemoveAllPoints();
        }
    }



    Vector2 Get2DPointsOnPlane(Vector3 pointCS, DetectionPlane detecionPlane)
    {   

        // return the points we're investigating
        Vector2 projectedPoint = Vector2.zero;        
        switch (detecionPlane)
        {
            case DetectionPlane.FrontPlane:
                // XY plane
                projectedPoint = new Vector2(pointCS.x, pointCS.y);
                break;
            case DetectionPlane.SidePlane:
                // XZ plane
                projectedPoint = new Vector2(pointCS.z, pointCS.y);
                break;
            case DetectionPlane.GroundPlane:
                // ZY plane
                projectedPoint = new Vector2(pointCS.x, pointCS.z);
                break;
        }

        return projectedPoint;
    }

    Vector3 TransformWorldToCameraSpace(Vector3 worldSpacePoint)
    {
        return Camera.main.worldToCameraMatrix.MultiplyPoint(worldSpacePoint);
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
