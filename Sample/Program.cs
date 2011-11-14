using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Sample
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Logger.Debug("Debug message 1");
            Logger.Debug("Debug message 2");
            Logger.Error("Error message 1");
        }
    }
}
