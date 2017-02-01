// FlareTestHarness\Program.cs - By Tom Weatherhead - August 21, 2009

using FlareLib;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareTestHarness
{
    public sealed class Program : FlareMain
    {
        public Program()
            : base()
        {
        }

        protected override void LogToConsole(string strMessage)
        {
            Console.WriteLine(strMessage);
        }

        protected override void LogToDatabase(string strMessage)    // Add other parameters later.
        {
            Console.WriteLine(string.Format(@"Attempt to log to database: ", strMessage));
        }

        static void Main(string[] args)
        {
            Console.WriteLine(@"Flare test harness : begin");

            Program program = new Program();

            program.NumberOfMainLoopIterations = 5;
            program.Run();

            Console.WriteLine(@"Flare test harness : end");
        }
    } // class Program
} // namespace FlareTestHarness

// **** End of File ****
