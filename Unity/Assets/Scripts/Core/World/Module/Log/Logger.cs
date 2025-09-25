using System;
using System.IO;

#if !DOTNET
using UnityEngine;
#endif

namespace ET
{
    public class Logger: Singleton<Logger>, ISingletonAwake
    {
        /// <summary>
        /// 日志写入的地址
        /// </summary>
        private string LockStepLogPath;

        /// <summary>
        /// 是否写入锁步日志
        /// </summary>
        private bool IsWriteLockStep;

        private ILog log;

        public ILog Log
        {
            set
            {
                this.log = value;
            }
            get
            {
                return this.log;
            }
        }

        public void Awake()
        {
        }

        public void SetIsWriteLockStep(bool value)
        {
            this.IsWriteLockStep = value;
            if (value)
                SetPathValue();
        }

        ///<summary>设置路径值</summary>
        public void SetPathValue()
        {
#if !DOTNET
            string time = DateTime.Now.ToString("yyyyMMddHHmm");
            string path = string.Empty;

            if (Application.isMobilePlatform)
                path = $"{Application.persistentDataPath}/{Application.productName}/Log/{time}/";
            else
            {
                path = $"{Application.dataPath}/Log/{time}/";
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "Log.txt";
            if (!File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                fs.Close();
            }

            this.LockStepLogPath = path;
#endif
        }

        public bool GetIsWriteLockStep()
        {
            return this.IsWriteLockStep;
        }

        public string GetLockStepLogPath()
        {
            return this.LockStepLogPath;
        }
    }
}