// FlareMain.cs - By Tom Weatherhead - June 3, 2009
// Contains the main loop for the Flare library "FlareLib"

#define MULTITHREAD_TARGET_TESTING
#define DO_NOT_SEND_EMAILS

using SolarFlareCommon;
using SolarFlareCommon.Collections.Generic;
using MySql.Data.MySqlClient;
using MySql.Data.Types;         // Necessary?
using System;
using System.Collections.Generic;
using System.Collections.Specialized;   // For NameValueCollection
//using System.Linq;
using System.Net;
//using System.Text;
using System.Threading;

namespace FlareLib
{
    public enum FlareServiceStatus
    {
        Stopped,
        Started,
        StopRequested
    }

    // ThAW 2009/08/20 : Make this class abstract.  Derived classes will implement an abstract property that will allow FlareMain
    // to write messages to the console and log messages to a database table.

    public abstract class FlareMain
    {
        private const uint m_unLocationID = 1;  // Primary location
        private readonly PriorityQueue<Target> m_pqWaitingTargets = new PriorityQueue<Target>();
        private readonly Queue<Target> m_qTargetsReadyForTesting = new Queue<Target>();
        private readonly IMySqlConnectionProvider m_cpDatabaseConnectionProvider = new FlareSimplyCachedMySqlConnectionProvider();
        private FlareServiceStatus m_eServiceStatus = FlareServiceStatus.Stopped;
        private int m_nNumberOfMainLoopIterations = 0;
        private List<Contact> m_lstAllContacts = null;
        private Dictionary<string, string> m_dictSystemConfigurationEntries = null;

        protected FlareMain()
        {
        }

        protected IMySqlConnectionProvider DatabaseConnectionProvider
        {
            get
            {
                return m_cpDatabaseConnectionProvider;
            }
        }

        protected int NumberOfMainLoopIterations
        {
            get
            {
                return m_nNumberOfMainLoopIterations;
            }
            set
            {
                m_nNumberOfMainLoopIterations = value;
            }
        }

        protected abstract void LogToConsole(string strMessage);

        protected abstract void LogToDatabase(string strMessage); // Add other parameters later.

        private void ReadTargetsFromDatabase()
        {
            List<Target> lstEnabledTargets = new List<Target>();
            MySqlConnection con = DatabaseConnectionProvider.GetMySqlConnection();

            // ThAW TO_DO 2009/08/26 : Use a Select command that only selects enabled targets that are associated with enabled accounts.
            string strCommandText = Target.SelectAllCommand;

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        Target target = new Target(reader, false);  // Do not close the reader after constructing the object

                        if (target.Enabled)
                        {
                            lstEnabledTargets.Add(target);
                        }
                    }
                }
            }

            foreach (Target target in lstEnabledTargets)
            {
                target.FinishConstruction(con);
                m_pqWaitingTargets.Enqueue(target);
                LogToConsole(string.Format(@"Constructed and enqueued the enabled target named '{0}'", target.Name));
            }
        }

        private static uint HTTPStatusCodeToUInt(HttpStatusCode httpsc)
        {

            switch (httpsc)
            {
                case HttpStatusCode.OK:
                    return 200;

                case HttpStatusCode.NotFound:
                    return 404;

                default:
                    break;
            }

            return 0;
        }

        private TargetLogRecord GetFromTarget(Target target)    // This method is called if the target's monitor type is "HTTP Get"
        {
            TargetLogRecord tlr = null;

            try
            {
                PostSubmitter post = new PostSubmitter();

                post.Url = target.URL;
                post.Type = PostSubmitter.PostTypeEnum.Get;

                string strResult = string.Format(@"Result of get from target: '{0}'", post.Post());

                // At this point, we know that we got a response, but is it the right response?
                // Or is it something like a custom 404 HTTP error page?

                //LogToConsole(strResult);  // This result is a page of HTML; it can be rather long.

                // TO_DO : Also log to database via LogToDatabase()
                tlr = new TargetLogRecord(target.TargetID, DateTime.UtcNow, TargetLogRecordStatus.Pass,
                    strResult, 0 /* HTTP error code */, m_unLocationID, 0 /* Response time in milliseconds */ );
            }
            catch (Exception ex)
            {
                uint unHTTPStatusCode = 0;

                if (ex is WebException)
                {
                    WebException ex2 = ex as WebException;

                    if (ex2.Response is HttpWebResponse)
                    {
                        HttpWebResponse httpwr = ex2.Response as HttpWebResponse;
                        HttpStatusCode httpsc = httpwr.StatusCode;

                        unHTTPStatusCode = HTTPStatusCodeToUInt(httpsc);
                    }
                }

                string strMessage = string.Format(@"Get from target: Caught {0} : {1}", ex.GetType().FullName, ex.Message);

                LogToConsole(strMessage);
                // TO_DO : Also log to database via LogToDatabase()
                tlr = new TargetLogRecord(target.TargetID, DateTime.UtcNow, TargetLogRecordStatus.Fail,
                    strMessage, unHTTPStatusCode /* HTTP error code */, m_unLocationID, 0 /* Response time in milliseconds */ );
            }

            return tlr;
        }

        private TargetLogRecord PostToTarget(Target target)    // This method is called if the target's monitor type is "HTTP Post"
        {
            TargetLogRecord tlr = null;

            try
            {
                PostSubmitter post = new PostSubmitter();

                post.Url = target.URL;

                foreach (TargetFormField tff in target.FormFields)
                {
                    post.PostItems.Add(tff.FieldName, tff.FieldValue);
                }

                post.Type = PostSubmitter.PostTypeEnum.Post;

                string strResult = string.Format(@"Result of post to target: '{0}'", post.Post());

                // At this point, we know that we got a response, but is it the right response?
                // Or is it something like a custom 404 HTTP error page?  Or a "Login failed" page?

                //LogToConsole(strResult);  // This result is a page of HTML; it can be rather long.

                // TO_DO : Also log to database via LogToDatabase()
                tlr = new TargetLogRecord(target.TargetID, DateTime.UtcNow, TargetLogRecordStatus.Pass,
                    strResult, 0 /* HTTP error code */, m_unLocationID, 0 /* Response time in milliseconds */ );
            }
            catch (Exception ex)
            {
                uint unHTTPStatusCode = 0;

                if (ex is WebException)
                {
                    WebException ex2 = ex as WebException;

                    if (ex2.Response is HttpWebResponse)
                    {
                        HttpWebResponse httpwr = ex2.Response as HttpWebResponse;
                        HttpStatusCode httpsc = httpwr.StatusCode;

                        unHTTPStatusCode = HTTPStatusCodeToUInt(httpsc);
                    }
                }

                string strMessage = string.Format(@"Post to target: Caught {0} : {1}", ex.GetType().FullName, ex.Message);

                LogToConsole(strMessage);
                // TO_DO : Also log to database via LogToDatabase()
                tlr = new TargetLogRecord(target.TargetID, DateTime.UtcNow, TargetLogRecordStatus.Fail,
                    strMessage, unHTTPStatusCode /* HTTP error code */, m_unLocationID, 0 /* Response time in milliseconds */ );
            }

            return tlr;
        }

        private TargetLogRecord PingTarget(Target target)  // ICMP echo request and response.  This method is called if the target's monitor type is "Ping".
        {
            TargetLogRecord tlr = null;

            try
            {
                string strResultMessageForLog;
                long lResponseTime;
                bool bSuccess = NetworkPing.Send(target.URL, out strResultMessageForLog, out lResponseTime);

                string strResult = string.Format(@"Result of ping: '{0}'", strResultMessageForLog);

                LogToConsole(strResult);
                // TO_DO : Also log to database via LogToDatabase()
                tlr = new TargetLogRecord(target.TargetID, DateTime.UtcNow, TargetLogRecordStatus.Pass,
                    strResult, 0 /* HTTP error code */, m_unLocationID, Convert.ToUInt32(lResponseTime) /* Response time in milliseconds */ );
            }
            catch (Exception ex)
            {
                string strMessage = string.Format(@"Ping target: Caught {0} : {1}", ex.GetType().FullName, ex.Message);

                LogToConsole(strMessage);
                // TO_DO : Also log to database via LogToDatabase()
                tlr = new TargetLogRecord(target.TargetID, DateTime.UtcNow, TargetLogRecordStatus.Fail,
                    strMessage, 0 /* HTTP error code */, m_unLocationID, 0 /* Response time in milliseconds */ );
            }

            return tlr;
        }

        // Send an e-mail to each of the target's account's contacts.

        private void SendFailureNotificationEmails(Target target, TargetLogRecord tlr)
        {

            if (!m_dictSystemConfigurationEntries.ContainsKey(SystemConfigurationEntryKeys.SMTPServerAddress))
            {
                throw new Exception(@"Flare system configuration error: No SMTP server address is specified");
            }

            if (!m_dictSystemConfigurationEntries.ContainsKey(SystemConfigurationEntryKeys.SMTPServerUserName))
            {
                throw new Exception(@"Flare system configuration error: No SMTP server user name is specified");
            }

            if (!m_dictSystemConfigurationEntries.ContainsKey(SystemConfigurationEntryKeys.SMTPServerPassword))
            {
                throw new Exception(@"Flare system configuration error: No SMTP server password is specified");
            }

            string strSMTPServerAddress = m_dictSystemConfigurationEntries[SystemConfigurationEntryKeys.SMTPServerAddress];
            string strSMTPServerUserName = m_dictSystemConfigurationEntries[SystemConfigurationEntryKeys.SMTPServerUserName];
            string strSMTPServerPassword = m_dictSystemConfigurationEntries[SystemConfigurationEntryKeys.SMTPServerPassword];
            const string strFrom = @"Flare Server Monitoring";
            const string strSubject = @"Server monitoring failure notification";

            foreach (Contact contact in m_lstAllContacts)
            {

                if (contact.AccountID != target.AccountID || !contact.Enabled)  // We could filter out disabled contacts at the SQL level
                {
                    continue;
                }

                string strBody = string.Format(@"Hello {0} {1}; Your server '{2}' was unreachable at {3} Universal Time.",
                    contact.FirstName, contact.LastName, target.Name, MySqlUtils.DateTimeToString(tlr.TimeStamp));

#if DO_NOT_SEND_EMAILS
                LogToConsole(@"**** Simulating the sending of e-mail ****");
                LogToConsole(string.Format(@"SMTP Server Address : {0}", strSMTPServerAddress));
                LogToConsole(string.Format(@"SMTP Server User Name : {0}", strSMTPServerUserName));
                LogToConsole(string.Format(@"SMTP Server Password : {0}", strSMTPServerPassword));
                LogToConsole(string.Format(@"From : {0}", strFrom));
                LogToConsole(string.Format(@"To : {0}", contact.EmailAddress));
                LogToConsole(string.Format(@"Subject : {0}", strSubject));
                LogToConsole(string.Format(@"Body : {0}", strBody));
                LogToConsole(@"**** End of e-mail ****");
#else
                MailUtils.SendMail(strSMTPServerAddress, strSMTPServerUserName, strSMTPServerPassword,
                    strFrom, contact.EmailAddress, null, null, strSubject, strBody);
#endif
            }
        }

        private void ThreadMain_TestTarget()
        {
            LogToConsole(@"**** Thread starting ****");

            Target target = null;

            lock (m_qTargetsReadyForTesting)
            {

                if (m_qTargetsReadyForTesting.Count <= 0)
                {
                    // Throw an exception when we attempt to Dequeue() ?
                    return;
                }

                target = m_qTargetsReadyForTesting.Dequeue();
            }

            LogToConsole(string.Format(@"Testing the target {0} (at {1}).", target.Name, target.URL));

            // In a separate thread:
            // - HTTP Post to the target
            // - Handle any errors (by logging to a database and sending e-mail(s))
            // - Re-enqueue the TargetInfo object while the priority queue is locked.
            TargetLogRecord tlr = null;

            switch (target.MonitorType)
            {
                case MonitorType.eHTTPGet:
                    tlr = GetFromTarget(target);
                    break;

                case MonitorType.eHTTPPost:
                    tlr = PostToTarget(target);
                    break;

                case MonitorType.ePing:
                    tlr = PingTarget(target);
                    break;

                // Default case: Throw an exception?
            }

            if (tlr != null)
            {
                MySqlConnection con = DatabaseConnectionProvider.GetMySqlConnection();

                tlr.Insert(con);
                target.UpdateLastTargetLogID(tlr.LogID, con);

                if (tlr.Status == TargetLogRecordStatus.Fail)
                {
                    // Send an e-mail to each of the target's account's contacts.
                    SendFailureNotificationEmails(target, tlr);
                }
            }

            //target.LastMonitoredAt = DateTime.UtcNow;
            target.UpdateLastMonitoredAt(DateTime.UtcNow, DatabaseConnectionProvider.GetMySqlConnection());
            target.DateTimeOfNextMonitor = target.LastMonitoredAt.Value + target.MonitorIntervalAsTimeSpan;   // or += target.MonitorIntervalAsTimeSpan;
            // Update the target's LastMonitoredAt in the database
            m_pqWaitingTargets.Enqueue(target);

            LogToConsole(string.Format(@"The target {0} has been dequeued, bumped, and re-enqueued.", target.Name));

            LogToConsole(@"**** Thread ending ****");
        }

        public void Run()
        {
            LogToConsole(@"Flare main loop: start");

            lock (this)
            {
                m_eServiceStatus = FlareServiceStatus.Started;
            }

            try
            {
                m_pqWaitingTargets.Clear();
                m_qTargetsReadyForTesting.Clear();

                ReadTargetsFromDatabase();

                MySqlConnection con = DatabaseConnectionProvider.GetMySqlConnection();

                m_lstAllContacts = Contact.GetAllContacts(con);
                m_dictSystemConfigurationEntries = SystemConfigurationEntry.GetDictionary(con);

                if (m_pqWaitingTargets.IsEmpty())
                {
                    LogToConsole(@"Aborting: The priority queue is empty; there are no servers to monitor.");
                    return;
                }

                // The main loop begins here.
                TimeSpan tsOneSecond = new TimeSpan(0, 0, 1);
                TimeSpan tsLoopPeriod = new TimeSpan(0, 0, 1);
                bool bLimitNumberOfMainLoopIterations = (NumberOfMainLoopIterations > 0);

                LogToConsole(@"Starting the main loop...");

                for (int nMainLoopIterationNumber = 0;
                    !bLimitNumberOfMainLoopIterations || nMainLoopIterationNumber < NumberOfMainLoopIterations;
                    ++nMainLoopIterationNumber)
                {
                    DateTime dtNow = DateTime.UtcNow;

                    LogToConsole(string.Format(@"It is now {0}.", dtNow.ToLongTimeString()));

                    if (m_pqWaitingTargets.IsEmpty())
                    {
                        LogToConsole(@"The priority queue is empty; sleeping for one second.");
                        System.Threading.Thread.Sleep(tsOneSecond);
                        continue;
                    }

                    Target targetHead = m_pqWaitingTargets.Peek();

                    LogToConsole(string.Format(@"The target at the head of the queue ('{0}') is due to be tested at {1}.",
                        targetHead.Name, targetHead.DateTimeOfNextMonitor.ToLongTimeString()));

                    if (dtNow < targetHead.DateTimeOfNextMonitor)
                    {
                        TimeSpan tsDifference = targetHead.DateTimeOfNextMonitor - dtNow;
                        TimeSpan tsTimeSpanToSleep = (tsDifference < tsLoopPeriod) ? tsDifference : tsLoopPeriod;

                        LogToConsole(string.Format(@"It is not yet time to test the target at the head of the queue.  Sleeping for {0} seconds and {1} milliseconds...",
                            tsTimeSpanToSleep.Seconds, tsTimeSpanToSleep.Milliseconds));

                        System.Threading.Thread.Sleep(tsTimeSpanToSleep);
                        continue;
                    }

                    lock (m_qTargetsReadyForTesting)
                    {
                        m_qTargetsReadyForTesting.Enqueue(m_pqWaitingTargets.Dequeue());
                    }

#if MULTITHREAD_TARGET_TESTING
                    // Note: There is also a delegate named ParameterizedThreadStart; perhaps it may be useful
                    // in allowing us to pass a TargetInfo object as a parameter, thus avoiding the m_qTargetsReadyForTesting queue.
                    Thread thread = new Thread(new ThreadStart(ThreadMain_TestTarget));

                    thread.Start();
#else
                    ThreadMain_TestTarget();
#endif

                    lock (this)
                    {

                        if (m_eServiceStatus == FlareServiceStatus.StopRequested)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToConsole(string.Format(@"{0} caught: {1}", ex.GetType().FullName, ex.Message));
            }

            lock (this)
            {
                m_eServiceStatus = FlareServiceStatus.Stopped;
            }

            LogToConsole(@"Flare main loop: end");
        }

        public void StartService()
        {

            lock (this)
            {

                if (m_eServiceStatus != FlareServiceStatus.Stopped)
                {
                    return;
                }
            }

            Thread thread = new Thread(new ThreadStart(Run));

            thread.Start();
        }

        public void StopService()
        {

            lock (this)
            {
                m_eServiceStatus = FlareServiceStatus.StopRequested;
            }
        }
    } // class FlareMain
} // namespace FlareLib

// **** End of File ****
