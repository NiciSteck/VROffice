using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StudyTimer : MonoBehaviour
{
    [NonSerialized] public static StudyTimer Timer;

    public enum Latin
    {
        MD,
        MB,
        AD,
        AB
    }
    public string participant;
    public Latin latin;
    private string prototype;

    private Stopwatch stopwatch;

    // Start is called before the first frame update
    void Start()
    {
        Timer = this;
    }

    // Update is called once per frame
    public void startTimer(string proto)
    {
        if (stopwatch == null)
        {
            this.prototype = proto;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }
    }

    public void stopTimer()
    {
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        string fileName = participant + latin;
        Debug.Log(fileName);
        string fullPath = Path.Combine("StudyTimes", fileName);

        using (FileStream stream = new FileStream(fullPath, FileMode.Append))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(prototype);
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(elapsedTime);
                writer.WriteLine();
            }
        }
    }
}