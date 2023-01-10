using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;
using System.IO;
using UnityEngine.Events;
using System.Linq;

public class PDollarGestureDetector : MonoBehaviour
{
    public PointVisualiserManager DebugPointVisualiserManager;

    public bool CreationMode = false;
    public string NewGestureName = "NewGesture";

    public float RecognitionThreshold = 0.9f;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecogniseGesture;

    private List<Gesture> trainingSet= new List<Gesture>();
    private int currentStroke = 0;
    private List<List<Vector3>> strokeList = new List<List<Vector3>>();
    

    // Start is called before the first frame update
    void Start()
    {
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (var file in gestureFiles)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(file));
        }
    }

    public void AddStroke(List<Vector3> positionList)
    {
        Debug.Log("adding stroke");
        AddDebugStroke(positionList);
        currentStroke++;
        strokeList.Add(positionList);
    }

    public void ToggleCreationMode()
    {
        CreationMode = !CreationMode;
        Debug.Log("creation mode set to " + CreationMode.ToString());
    }

    public void ConvertToPDollarGesture()
    {
        // don't if the list is empty
        if (!strokeList.Any())
        {
            return;
        }

        // get the nr of points in the whole gesture
        int nrOfPoints = 0;
        foreach(var stroke in strokeList)
        {
            nrOfPoints += stroke.Count;
        }
        // Create a point array of fitting size
        PDollarGestureRecognizer.Point[] points = new PDollarGestureRecognizer.Point[nrOfPoints];

        int currentPoint = 0;
        // fill the point array per stroke
        for (int i = 0; i < strokeList.Count; i++)
        {
            for (int j = 0; j < strokeList[i].Count; j++)
            {
                Vector2 screenPosition = Camera.main.WorldToScreenPoint(strokeList[i][j]);
                points[currentPoint] = new Point(screenPosition.x, screenPosition.y, i);
                currentPoint++;
            }
        }

        // create a gesture from the point array
        PDollarGestureRecognizer.Gesture currentGesture = new PDollarGestureRecognizer.Gesture(points);


        HandleNewGesture(currentGesture, points);

        EndGesture();

    }

    void EndGesture()
    {
        strokeList.Clear();
        RemoveDebugPoints();
    }

    void HandleNewGesture(PDollarGestureRecognizer.Gesture NewGesture, PDollarGestureRecognizer.Point[] PointArray)
    {
        // add new gesture to training set
        if (CreationMode)
        {
            SaveNewGesture(NewGesture, PointArray);
        }
        // recognise the current gesture
        else
        {
            RecogniseGesture(NewGesture);
        }
    }

    void SaveNewGesture(PDollarGestureRecognizer.Gesture NewGesture, PDollarGestureRecognizer.Point[] PointArray)
    {
        Debug.Log("New Gesture created - " + NewGestureName);
        NewGesture.Name = NewGestureName;
        trainingSet.Add(NewGesture);

        string filename = Application.persistentDataPath + "/" + NewGesture.Name + ".xml";
        GestureIO.WriteGesture(PointArray, NewGesture.Name, filename);
    }

    void RecogniseGesture(PDollarGestureRecognizer.Gesture NewGesture)
    {
        Result result = PointCloudRecognizer.Classify(NewGesture, trainingSet.ToArray());
        Debug.Log(result.GestureClass + " " + result.Score);
        if (result.Score > RecognitionThreshold)
        {
            OnRecogniseGesture.Invoke(result.GestureClass);
        }
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
}
