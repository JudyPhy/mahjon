using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MJLog {

    private static string filename = Application.persistentDataPath + "/log";
    private static bool startGame = false;

    public static void Log(string str)
    {
        if (!startGame)
        {
            StreamWriter sw1 = new StreamWriter(filename, true);
            sw1.WriteLine("\n\n\n==============================================================================");
            sw1.Close();
            startGame = true;
        } 
        Debug.Log(str);
        StreamWriter sw = new StreamWriter(filename, true);
        sw.WriteLine(str);
        sw.Close();
    }

    public static void LogError(string str)
    {
        Debug.LogError(str);
        StreamWriter sw = new StreamWriter(filename, true);
        sw.WriteLine(str);
        sw.Close();
    }

}
