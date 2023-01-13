using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using PDollarGestureRecognizer;
using System.Linq;

// This code is responsible for storing strokes and converting them onto all available planes

public class PointToPlaneConverter : MonoBehaviour
{
    public enum DetectionPlane
    {
        FrontPlane = 0,
        SidePlane = 1,
        GroundPlane = 2,
        ScreenSpace = 3,
    }

    // Public variables
    public PointVisualiserManager DebugPointVisualiserManager;

    private int currentStroke = 0;
    private List<List<Vector3>> strokeList = new List<List<Vector3>>();

    private PDollarGestureRecognizer.Point[] frontPoints;
    private PDollarGestureRecognizer.Point[] sidePoints;
    private PDollarGestureRecognizer.Point[] groundPoints;
    private PDollarGestureRecognizer.Point[] ssPoints;

    public UnityEvent OnConversionCompleted;

    //----------------
    // Getters
    //----------------
    public PDollarGestureRecognizer.Point[] GetFrontPlanePoints() { return frontPoints; }
    public PDollarGestureRecognizer.Point[] GetSidePlanePoints() { return sidePoints; }
    public PDollarGestureRecognizer.Point[] GetGroundPlanePoints() { return groundPoints; }
    public PDollarGestureRecognizer.Point[] GetScreenSpacePoints() { return ssPoints; }


    //----------------
    // Main functionality
    //----------------

    public void AddStroke(List<Vector3> positionList)
    {
        AddDebugStroke(positionList);
        currentStroke++;
        strokeList.Add(positionList);
    }

    public void StartConversion()
    {
        CalculateProjectedPoints();

        // clear the stroke list to prepare for the next one
        // All needed information for the cameraBasedDetector can be found in the point arrays

        strokeList.Clear();
        RemoveDebugPoints();
    }

    void CalculateProjectedPoints()
    {
        if (!strokeList.Any())
        {
            return;
        }

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

        OnConversionCompleted.Invoke();
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

    //----------------
    // Debug options
    //----------------

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
