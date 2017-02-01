// FlareService\FlareService.cs - Tom Weatherhead - August 22, 2009

using FlareLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
//using System.Timers;

namespace FlareService
{
    public partial class FlareService : ServiceBase
    {
        private readonly FlareModule m_fm = new FlareModule();

        public FlareService()
        {
            //ServiceName = @"FlareService";    // Set in InitializeComponent()
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;

            const string strEventLog = @"FlareServiceEventLog";
            const string strEventLogSource = @"FlareServiceEventLogSource";

            InitializeComponent();

            if (!System.Diagnostics.EventLog.SourceExists(strEventLogSource))
            {
                System.Diagnostics.EventLog.CreateEventSource(strEventLogSource, strEventLog);
            }

            // The event log source by which the application is registered on the computer.
            eventLog.Source = strEventLogSource;

            eventLog.Log = strEventLog;
        }

        // Dispose(bool disposing) ? -> Implemented in FlareService.Designer.cs

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            eventLog.WriteEntry(@"FlareService : OnStart()");
            m_fm.StartService();
        }

        protected override void OnPause()
        {
            eventLog.WriteEntry(@"FlareService : OnPause()");
            m_fm.StopService();
        }

        protected override void OnContinue()
        {
            eventLog.WriteEntry(@"FlareService : OnContinue()");
            m_fm.StartService();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            eventLog.WriteEntry(@"FlareService : OnStop()");
            m_fm.StopService();
        }

        protected override void OnShutdown()
        {
            eventLog.WriteEntry(@"FlareService : OnShutdown()");
            m_fm.StopService();
        }
    } // class FlareService

    public class FlareModule : FlareMain
    {
        public FlareModule()
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
    } // class FlareModule
} // namespace FlareService

// **** End of File ****
