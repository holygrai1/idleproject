using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Log管理器
/// </summary>
public class LogManager : MonoBehaviour
{
    #region Singleton and DontDestroyOnLoad
    private static LogManager sInstance;
    public static LogManager Instance => sInstance;
    private void Awake()
    {
        if (sInstance != null && sInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sInstance = this;
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceivedThreaded += ApplicationLogMessageReceived;
        }
    }
    #endregion

    private List<string> mErrorLogMessages = new List<string>();
    private float mSendLogMessageCD = 0.0f;
    private int mMaxSendLogMessageNumPerSec = 5;
    private int mCurSendIndex = 0;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("LOGGER_ON")]
    static public void Log(string s, params object[] p)
    {
        Debug.Log(DateTime.Now + " -- " + (p != null && p.Length > 0 ? string.Format(s, p) : s));
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("LOGGER_ON")]
    static public void Log(object o)
    {
        Debug.Log(o);
    }

    static public void Error(string s, params object[] p)
    {
        Debug.LogError(DateTime.Now + " -- " + (p != null && p.Length > 0 ? string.Format(s, p) : s));
    }
    static public void Error(object o)
    {
        Debug.LogError(o);
    }

    public void Update()
    {
#if UNITY_EDITOR
        return;
#endif
    }

    private void ApplicationLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            var msg = condition + "\n" + stackTrace;
            if (mErrorLogMessages.Contains(msg) == false)
            {
                mErrorLogMessages.Add(msg);
            }
        }
    }
}