using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementDirectionDetector : MonoBehaviour
{
    public BaseMovementDetector MovementDetector;
    public PointVisualiserManager DebugPointVisualiserManager;
    public float DirectionChangeThreshold = 0.8f;

    // TODO: Change queue to list
    private Queue<Vector3> directionQueue3D = new Queue<Vector3>();
    private Queue<Vector2> directionQueue2D = new Queue<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CalculateNewDirection()
    {
        if(!MovementDetector)
        {
            return; 
        }

        if(MovementDetector.GetPositionListCount() < 2)
        {
            return;
        }


        Vector2 currentDirection = MovementDetector.GetCurrentDirectionSS();
        if(directionQueue2D.Count > 0)
        {
            float dirrectionAllignment = Vector2.Dot(currentDirection, directionQueue2D.Last());
           
            // direction is too similar to the previous one
            if (dirrectionAllignment > DirectionChangeThreshold)
            {
                return;
            }
           
        }
        Debug.Log(currentDirection.ToString());
        directionQueue2D.Enqueue(currentDirection);
        AddDebugPoint();
    }

    public void EndMovement()
    {

        RemoveDebugPoints();
        directionQueue2D.Clear();
    }


    void AddDebugPoint()
    {
        if (DebugPointVisualiserManager)
        {
            DebugPointVisualiserManager.AddPoint(MovementDetector.GetPreviousMovementPosition());
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
