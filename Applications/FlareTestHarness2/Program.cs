// Program.cs - By Tom Weatherhead - June 3, 2009
// The main program for the Flare test harness console application "Flare01"

#define MULTITHREAD_TARGET_TESTING

using SolarFlareCommon;
using SolarFlareCommon.Collections.Generic;
using MySql.Data.MySqlClient;   // Necessary?
using MySql.Data.Types;         // Necessary?
using System;
using System.Collections.Generic;
using System.Collections.Specialized;   // For NameValueCollection
//using System.Linq;
//using System.Text;
using System.Threading;

namespace Flare
{
    public sealed class TargetInfo : IComparable<TargetInfo>
    {
        // Members
        private readonly string m_strTargetName;
        private readonly string m_strAccountID;
        private readonly string m_strTargetURL;
        private readonly string m_strUserNameFieldName;
        private readonly string m_strUserName;
        private readonly string m_strPasswordFieldName;
        private readonly string m_strPassword;
        private readonly int m_nTimeIntervalInSeconds;
        private readonly TimeSpan m_tsTimeInterval;
        private readonly NameValueCollection m_nvcAdditionalFormFields;
        private DateTime m_dtNextLoginTest;

        // Constructor

        public TargetInfo(IniFile iniTargetInfo, string strSectionName)
        {
            m_strTargetName = iniTargetInfo.GetString(strSectionName, @"TargetName", string.Empty);
            m_strAccountID = iniTargetInfo.GetString(strSectionName, @"AccountID", string.Empty);
            m_strTargetURL = iniTargetInfo.GetString(strSectionName, @"TargetURL"); // Mandatory
            m_strUserNameFieldName = iniTargetInfo.GetString(strSectionName, @"UserNameFieldName"); // Mandatory
            m_strUserName = iniTargetInfo.GetString(strSectionName, @"UserName"); // Mandatory
            m_strPasswordFieldName = iniTargetInfo.GetString(strSectionName, @"PasswordFieldName"); // Mandatory
            m_strPassword = iniTargetInfo.GetString(strSectionName, @"Password"); // Mandatory
            m_nTimeIntervalInSeconds = Convert.ToInt32(iniTargetInfo.GetString(strSectionName, @"TimeIntervalInSeconds"));  // Mandatory

            m_tsTimeInterval = new TimeSpan(0, 0, m_nTimeIntervalInSeconds);

            m_nvcAdditionalFormFields = new NameValueCollection();

            foreach (string strKeyName in iniTargetInfo.GetKeyNames(strSectionName))
            {
                const string strNameKeyStartsWith = @"AdditionalFormFieldName";

                if (!strKeyName.StartsWith(strNameKeyStartsWith))
                {
                    continue;
                }

                string strKeyEndsWith = strKeyName.Substring(strNameKeyStartsWith.Length);
                string strValueKey = @"AdditionalFormFieldValue" + strKeyEndsWith;

                if (!iniTargetInfo.KeyExists(strSectionName, strValueKey))
                {
                    continue;
                }

                string strName = iniTargetInfo.GetString(strSectionName, strKeyName);
                string strValue = iniTargetInfo.GetString(strSectionName, strValueKey);

                /*
                if (m_nvcAdditionalFormFields.Keys.Contains(strName))
                {
                    throw new Exception(@"...");
                }
                 */

                m_nvcAdditionalFormFields.Add(strName, strValue);
            }

            m_dtNextLoginTest = DateTime.Now;
        }

        // Properties

        public string TargetName
        {
            get
            {
                return m_strTargetName;
            }
        }

        public string AccountID
        {
            get
            {
                return m_strAccountID;
            }
        }

        public string TargetURL
        {
            get
            {
                return m_strTargetURL;
            }
        }

        public string UserNameFieldName
        {
            get
            {
                return m_strUserNameFieldName;
            }
        }

        public string UserName
        {
            get
            {
                return m_strUserName;
            }
        }

        public string PasswordFieldName
        {
            get
            {
                return m_strPasswordFieldName;
            }
        }

        public string Password
        {
            get
            {
                return m_strPassword;
            }
        }

        //public readonly int m_nTimeIntervalInSeconds;

        public TimeSpan TimeInterval
        {
            get
            {
                return m_tsTimeInterval;
            }
        }

        public NameValueCollection AdditionalFormFields
        {
            get
            {
                return m_nvcAdditionalFormFields;
            }
        }

        public DateTime DateTimeOfNextLoginTest
        {
            get
            {
                return m_dtNextLoginTest;
            }
            set
            {
                m_dtNextLoginTest = value;
            }
        }

        // Implementation of IComparable<TargetInfo>

        public int CompareTo(TargetInfo other)
        {
            // In the priority queue, the TargetInfo object with the smallest DateTimeOfNextLoginTest should be at the head.

            if (DateTimeOfNextLoginTest > other.DateTimeOfNextLoginTest)
            {
                return -1;
            }

            if (DateTimeOfNextLoginTest < other.DateTimeOfNextLoginTest)
            {
                return 1;
            }

            return 0;
        }
    } // class TargetInfo

    public sealed class MainUnit
    {
        private readonly PriorityQueue<TargetInfo> m_pqWaitingTargets = new PriorityQueue<TargetInfo>();
        private readonly Queue<TargetInfo> m_qTargetsReadyForTesting = new Queue<TargetInfo>();

        public MainUnit()
        {
        }

        private void ReadTargetInfoIni()
        {
            const string strTargetInfoIniFilePath = @"..\..\TargetInfo2.ini";
            IniFile iniTargetInfo = new IniFile(strTargetInfoIniFilePath);

            foreach (string strSectionName in iniTargetInfo.GetSectionNames())
            {
                // For each target (server to be monitored), there is the following info in the corresponding INI file section:
                // - Target name
                // - Account ID (for contacting (sending alerts) and billing)
                // - Target URL
                // - User name
                // - Password
                // - Time interval
                TargetInfo ti = new TargetInfo(iniTargetInfo, strSectionName);

                Console.WriteLine(@"Read INI info for target named '{0}'", ti.TargetName);
                m_pqWaitingTargets.Enqueue(ti);
            }
        }

        private void PostToTarget(TargetInfo ti)
        {

            try
            {
                PostSubmitter post = new PostSubmitter();

                post.Url = ti.TargetURL;
                /*
                <input type="hidden" name="SCREEN" value="homebase/signin">
                <input type="hidden" name="ACTION" value="SIGNIN">
                <input type="hidden" name="return" value="/homebase/go_hipserv">
                <input id="inSubdomain" name="inSubdomain" type="text" maxlength="32" size="19" value="">
                <input id="inUsername" name="inUsername" type="text" maxlength="32" size="19" value="">
                <input id="inPassword" name="inPassword" type="password" size="19" value="">
                <input type="checkbox" id="rememberMe" name="rememberMe"  value="1">
                 */
                post.PostItems.Add(ti.UserNameFieldName, ti.UserName);
                post.PostItems.Add(ti.PasswordFieldName, ti.Password);

                for (int i = 0; i < ti.AdditionalFormFields.Count; ++i)
                {
                    post.PostItems.Add(ti.AdditionalFormFields.GetKey(i), ti.AdditionalFormFields[i]);
                }

                post.Type = PostSubmitter.PostTypeEnum.Post;

                string strResult = post.Post();

                Console.WriteLine(@"Result of post to target: '{0}'", strResult);
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine(@"Post to target: Caught {0} : {1}", ex.GetType().FullName, ex.Message);
            }
        }

        private void ThreadMain_TestTarget()
        {
            Console.WriteLine(@"**** Thread starting ****");

            TargetInfo ti = null;

            lock (m_qTargetsReadyForTesting)
            {

                if (m_qTargetsReadyForTesting.Count <= 0)
                {
                    // Throw an exception when we attempt to Dequeue() ?
                    return;
                }

                ti = m_qTargetsReadyForTesting.Dequeue();
            }

            Console.WriteLine(@"Testing the target {0} (at {1}).", ti.TargetName, ti.TargetURL);

            // In a separate thread:
            // - HTTP Post to the target
            // - Handle any errors (by logging to a database and sending e-mail(s))
            // - Re-enqueue the TargetInfo object while the priority queue is locked.

            PostToTarget(ti);

            ti.DateTimeOfNextLoginTest = DateTime.Now + ti.TimeInterval;   // or += ti.TimeInterval;
            m_pqWaitingTargets.Enqueue(ti);

            Console.WriteLine(@"The target {0} has been dequeued, bumped, and re-enqueued.", ti.TargetName);

            Console.WriteLine(@"**** Thread ending ****");
        }

        public void Run()
        {
            Console.WriteLine(@"Flare01: start");

            try
            {
                ReadTargetInfoIni();

                if (m_pqWaitingTargets.IsEmpty())
                {
                    Console.WriteLine(@"Aborting: The priority queue is empty; there are no servers to monitor.");
                    return;
                }

                // The main loop begins here.
                TimeSpan tsTenSeconds = new TimeSpan(0, 0, 10);
                TimeSpan tsLoopPeriod = new TimeSpan(0, 0, 10);

                Console.WriteLine(@"Starting the main loop...");

                for (int nMainLoopIterationNumber = 0; nMainLoopIterationNumber < 3; ++nMainLoopIterationNumber)
                {
                    DateTime dtNow = DateTime.Now;

                    Console.WriteLine(@"It is now {0}.", dtNow.ToLongTimeString());

                    if (m_pqWaitingTargets.IsEmpty())
                    {
                        Console.WriteLine(@"The priority queue is empty; sleeping for ten seconds.");
                        System.Threading.Thread.Sleep(tsTenSeconds);
                        continue;
                    }

                    TargetInfo tiHead = m_pqWaitingTargets.Peek();

                    Console.WriteLine(@"The target at the head of the queue ('{0}') is due to be tested at {1}.",
                        tiHead.TargetName, tiHead.DateTimeOfNextLoginTest.ToLongTimeString());

                    if (dtNow < tiHead.DateTimeOfNextLoginTest)
                    {
                        TimeSpan tsDifference = tiHead.DateTimeOfNextLoginTest - dtNow;
                        TimeSpan tsTimeSpanToSleep = (tsDifference < tsLoopPeriod) ? tsDifference : tsLoopPeriod;

                        Console.WriteLine(@"It is not yet time to test the target at the head of the queue.  Sleeping for {0} seconds and {1} milliseconds...",
                            tsTimeSpanToSleep.Seconds, tsTimeSpanToSleep.Milliseconds);

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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(@"{0} caught: {1}", ex.GetType().FullName, ex.Message));
            }

            Console.WriteLine(@"Flare01: end");
        }
    } // class MainUnit

    public sealed class Program
    {
        public static void Main(string[] args)
        {
            MainUnit mu = new MainUnit();

            mu.Run();
        }
    } // class Program
} // namespace Flare

// **** End of File ****
