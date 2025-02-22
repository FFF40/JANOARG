using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogMaker : MonoBehaviour
{
    private string logFilePath;

    public bool AddDebugLogMessages;
    public bool AddDebugWarningMessages;

    void Awake()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "debug_log.txt");

        Application.logMessageReceived += LogToFile;
        File.WriteAllText(logFilePath, $"{System.DateTime.Now}: Start\n\n");
    }

    void AddLog(string condition, string stackTrace, LogType type)
    {
        string logEntry = $"{System.DateTime.Now}: [{type}] {condition}\n{stackTrace}\n\n";
        File.AppendAllText(logFilePath, logEntry);
    }

    void LogToFile(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                if (AddDebugLogMessages)
                {
                    AddLog(condition, stackTrace, type);
                }
                break;
            case LogType.Warning:
                if (AddDebugWarningMessages)
                {
                    AddLog(condition, stackTrace, type);
                }
                break;
            default:
                AddLog(condition, stackTrace, type);
                break;
        }
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= LogToFile;
    }

}
