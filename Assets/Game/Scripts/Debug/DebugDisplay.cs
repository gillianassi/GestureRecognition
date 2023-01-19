using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;




public class DebugDisplay : MonoBehaviour
{

    struct LogMessage
    {

        public LogMessage(string message, LogType type)
        {
            Message = message;
            MessageType = type;
        }
        public string Message;
        public LogType MessageType;

    }

    public TMPro.TextMeshProUGUI Display;

    public GameObject Panel;

    public int MaxNrOfLines = 8;

    private Queue<LogMessage> logs = new Queue<LogMessage>();

    private void OnEnable()
    {
        // subscribe
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public void ClearDebugLog()
    {
        logs.Clear();
        Display.text = "";
    }

    public void DeactivatePanel()
    {
        Panel.SetActive(false);
    }

    public void ActivatePanel()
    {
        Panel.SetActive(true);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {

        string[] splitString = logString.Split(char.Parse(":"));
        string debugKey = splitString[0];

        LogMessage newLog;
        newLog.MessageType = type;
        newLog.Message = splitString[0];

        if (logs.Count >= MaxNrOfLines)
        {

            logs.Dequeue();
        }

        logs.Enqueue(newLog);

        Display.text = "";

        string displayText = "";
        foreach (var log in logs) 
        {
            // get the color
            Color color = Color.white;
            switch (log.MessageType)
            {
                case LogType.Error:
                    color = Color.red;
                    break;
                case LogType.Assert:
                    color = Color.gray;
                    break;
                case LogType.Warning:
                    color = Color.yellow;
                    break;
                case LogType.Log:
                    color = Color.white;
                    break;
                case LogType.Exception:
                    color = Color.blue;
                    break;
                default:
                    break;
            }


            displayText +="<#" + color.ToHexString() + ">" + log.Message + "\n";
          
        }
        Display.text = displayText;
    }
}
