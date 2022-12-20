using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Pose
{
    public string name;
    public List<Vector3> BonePositions;
    public UnityEvent onRecognized;
}


public class PureHandPoseDetector : MonoBehaviour
{
    public float threshold = 0.1f;
    public OVRSkeleton Skeleton;
    public List<Pose> Poses;
    public bool IsDebugging = false;


    private bool m_IsInitialized;
    private List<OVRBone> m_FingerBones;
    private Pose m_prevPose;
    // Start is called before the first frame update
    void Start()
    {
        // initialize with a delay, as the oculus hands get initialized later
        StartCoroutine(DelayRoutine(2.5f, Initialize));
    }

    // Coroutine used as a delay
    public IEnumerator DelayRoutine(float delay, Action actionToDo)
    {
        yield return new WaitForSeconds(delay);
        actionToDo.Invoke();
    }

    public void Initialize()
    {
        // Check the function for know what it does
        m_FingerBones = new List<OVRBone>(Skeleton.Bones);
        m_prevPose = new Pose();

        // After initialize the skeleton set a boolean to true to confirm the initialization
        m_IsInitialized = true;
    }


    // Update is called once per frame
    void Update()
    {
        if(!m_IsInitialized)
        {
            return;
        }
        HandlePoseDetection();


        HandleDebug();

    }
    
    void HandlePoseDetection()
    {
        if(Poses.Count < 2)
        {
            return;
        }
        Pose currentPose = Recognise();
        bool hasRecognised = !currentPose.Equals(new Pose());

        if(!hasRecognised)
        {
            return;
        }

        if(currentPose.Equals(m_prevPose)) 
        {
            return;
        }

        Debug.Log("New Pose Found!: " + currentPose.name);
        m_prevPose= currentPose;
        currentPose.onRecognized.Invoke();


    }

    void HandleDebug()
    {
        if (!IsDebugging)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SavePose();
        }
    }

    void SavePose()
    {
        Pose pose = new Pose();
        pose.name = "New Pose";
        List<Vector3> data = new List<Vector3>();
        foreach(var bone in m_FingerBones) 
        {
            // flex ect can also be checked
            // use relative position from hand
            data.Add(Skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        pose.BonePositions = data;
        Poses.Add(pose);
    }

    Pose Recognise()
    {
        Pose currentPose = new Pose();
        float currentMin = Mathf.Infinity;

        foreach(var pose in Poses)
        {

            float sumDistance = 0f;
            bool isDiscarded = false;

            for(int i = 0; i < m_FingerBones.Count; i++) 
            { 
                Vector3 currentData = Skeleton.transform.InverseTransformPoint(m_FingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, pose.BonePositions[i]);
                if(distance > threshold)
                {
                    // discard gesture
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentPose= pose;
            }
        }
        return currentPose;
    }    
}
