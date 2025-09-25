using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ET
{
    public class UnityLogger : ILog
    {
        public void Trace(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public void Debug(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public void Info(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public void Warning(string msg)
        {
            UnityEngine.Debug.LogWarning(msg);
            Write("Warning", msg);
        }

        public void Error(string msg)
        {
#if UNITY_EDITOR
            msg = Msg2LinkStackMsg(msg);
#endif
            UnityEngine.Debug.LogError(msg);
        }

        private static string Msg2LinkStackMsg(string msg)
        {
            msg = Regex.Replace(msg, @"at (.*?) in (.*?\.cs):(\w+)", match =>
            {
                string path = match.Groups[2].Value;
                string line = match.Groups[3].Value;
                return $"{match.Groups[1].Value}\n<a href=\"{path}\" line=\"{line}\">{path}:{line}</a>";
            });
            return msg;
        }

        public void Error(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        public void Trace(string message, params object[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(message, args);
        }

        public void Info(string message, params object[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Error(string message, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(message, args);
        }

        ///<summary>日志写入</summary>
        private static void Write(string type, string message)
        {
            if (!Logger.Instance.GetIsWriteLockStep()) return;
            if (!message.StartsWith("-.-")) return;

     

            FileStream fs = new FileStream(Logger.Instance.GetLockStepLogPath(), FileMode.Append);
            StreamWriter streamWriter = new StreamWriter(fs);
            string value = $"{DateTime.Now} [{type}] \n{message}\n\n";
            streamWriter.Write(value);
            streamWriter.Close();
            fs.Close();
        }

      
    }
}