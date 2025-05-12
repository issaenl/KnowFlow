using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KnowFlow
{
    public class Logger
    {
        private static readonly string logFilePath = "log.txt";

        public static void Log(string message)
        {
            try
            {
                string fullMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(logFilePath, fullMessage + Environment.NewLine);
            }
            catch
            {
                
            }
        }
    }
}
