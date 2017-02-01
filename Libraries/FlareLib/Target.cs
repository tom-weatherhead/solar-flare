// FlareLib\Target.cs - By Tom Weatherhead - July 9, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using MySql.Data.Types;         // For MySqlConversionException
using System;
using System.Collections.Generic;
using System.Collections.Specialized;   // For NameValueCollection
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public enum MonitorType
    {
        eHTTPGet,
        eHTTPPost,
        ePing
    }

    public sealed class MonitorTypeCollectionMember
    {
        private MonitorType m_eMonitorType;
        private uint m_nEquivalentUInt;
        private string m_strUIString;

        public MonitorTypeCollectionMember(MonitorType eMonitorType, uint nEquivalentUInt, string strUIString)
        {
            m_eMonitorType = eMonitorType;
            m_nEquivalentUInt = nEquivalentUInt;
            m_strUIString = strUIString;
        }

        public MonitorType MonitorType
        {
            get
            {
                return m_eMonitorType;
            }
        }

        public uint EquivalentUInt
        {
            get
            {
                return m_nEquivalentUInt;
            }
        }

        public string UIString
        {
            get
            {
                return m_strUIString;
            }
        }
    } // class MonitorTypeCollectionMember

    public static class MonitorTypeCollection //: IEnumerable<MonitorTypeCollectionMember>  // Static classes cannot implement interfaces
    {
        // Member variable
        private static readonly List<MonitorTypeCollectionMember> m_lst;

        // Static constructor

        static MonitorTypeCollection()
        {
            m_lst = new List<MonitorTypeCollectionMember>();

            m_lst.Add(new MonitorTypeCollectionMember(MonitorType.eHTTPGet, 1, @"HTTP Get"));
            m_lst.Add(new MonitorTypeCollectionMember(MonitorType.eHTTPPost, 2, @"HTTP Post"));
            m_lst.Add(new MonitorTypeCollectionMember(MonitorType.ePing, 3, @"Ping"));
        }

        // Methods

        public static IEnumerable<MonitorTypeCollectionMember> GetEnumerable()
        {
            return m_lst;
        }

        public static MonitorTypeCollectionMember FindUsingMonitorType(MonitorType mt)
        {

            foreach (MonitorTypeCollectionMember item in m_lst)
            {

                if (mt == item.MonitorType)
                {
                    return item;
                }
            }

            return null;
        }

        public static MonitorTypeCollectionMember FindUsingEquivalentUInt(uint nEquivalentUInt)
        {

            foreach (MonitorTypeCollectionMember item in m_lst)
            {

                if (nEquivalentUInt == item.EquivalentUInt)
                {
                    return item;
                }
            }

            return null;
        }

        public static MonitorTypeCollectionMember FindUsingUIString(string strUIString)
        {

            foreach (MonitorTypeCollectionMember item in m_lst)
            {

                if (strUIString == item.UIString)
                {
                    return item;
                }
            }

            return null;
        }
    } // class MonitorTypeCollection

    public sealed class Target : NamedAndNumberedDatabaseObject, IComparable<Target>
    {
        // Column: TargetID (9-digit integer) (INT) (Primary key) (Auto increment)
        private UInt32 m_unAccountID = 0;   // Column: AccountID (9-digit integer) (INT) (Foreign key?)
        private bool m_bEnabled = false;    // Column: Enabled (Boolean; 1-character string: "Y" or "N") (CHAR(1))
        // Column: Name (-character string) (VARCHAR(100)) (Target name)
        private string m_strURL = string.Empty; // Column: URL (-character string) (VARCHAR(255))
        private int m_nMonitorInterval = 0; // Column: MonitorInterval (5-digit integer) (INT) (in seconds; can accomodate an interval of one day)
        private TimeSpan m_tsMonitorInterval = new TimeSpan(0, 0, 0);
        //private bool m_bLastMonitoredAtIsNull = true;
        //private DateTime m_dtLastMonitoredAt = new DateTime(2001, 1, 1);    // Column: LastMonitoredAt (Date and Time) (DATETIME)
        private DateTime? m_ndtLastMonitoredAt = null;
        private DateTime m_dtNextMonitor = DateTime.UtcNow;
        private MonitorType m_eMonitorType = DefaultMonitorType; // Column: MonitorType (INT) (enumerated type for (1) http get, (2) http post, (3) ping, etc....)
        private uint m_unLastTargetLogID = 0; // Column: LastTargetLogID (INT)
        private DateTime m_dtTargetAddedAt = DateTime.UtcNow;  // Column: TargetAddedAt (DATETIME) (date/time the target was added to system to show duration it has been monitored)
        //private DateTime m_dtLastFailedAt = new DateTime(2001, 1, 1);   // Column: LastFailedAt (DATETIME) (date/time of last failure monitor to show customer uptime for this target; only use failures customer was notified of)
        private DateTime? m_ndtLastFailedAt = null; // Nullable DateTime
        private readonly TargetFormFieldCollection m_collTargetFormFields = new TargetFormFieldCollection(); // Populated from the table TargetFormFields

        public Target()
            : base()
        {
        }

        public Target(MySqlDataReader reader, bool bCloseReader)  // Create the Target object from a row of data.
            : base(reader, 0, 3)
        {
            m_unAccountID = reader.GetUInt32(1);
            m_bEnabled = StringUtils.DatabaseStringToBoolean(reader, 2);
            m_strURL = reader.GetString(4);
            m_nMonitorInterval = reader.GetInt32(5);
            m_tsMonitorInterval = new TimeSpan(0, 0, m_nMonitorInterval);
            //m_bLastMonitoredAtIsNull = reader.IsDBNull(6);

            if (!reader.IsDBNull(6))
            {
                m_ndtLastMonitoredAt = reader.GetDateTime(6);
            }

            if (!m_ndtLastMonitoredAt.HasValue)
            {
                m_dtNextMonitor = DateTime.UtcNow;
            }
            else
            {
                m_dtNextMonitor = m_ndtLastMonitoredAt.Value + m_tsMonitorInterval;
            }

            uint unMonitorTypeAsUInt = reader.GetUInt32(7);
            MonitorTypeCollectionMember mtcm = MonitorTypeCollection.FindUsingEquivalentUInt(unMonitorTypeAsUInt);

            if (mtcm != null)
            {
                m_eMonitorType = mtcm.MonitorType;
            }

            m_unLastTargetLogID = reader.GetUInt32(8);

            try
            {

                if (!reader.IsDBNull(9))
                {
                    m_dtTargetAddedAt = reader.GetDateTime(9);
                }
            }
            catch (MySqlConversionException)
            {
                m_dtTargetAddedAt = DateTime.UtcNow;
            }

            if (!reader.IsDBNull(10))
            {
                m_ndtLastFailedAt = reader.GetDateTime(10);
            }

            if (bCloseReader)
            {
                reader.Close(); // We are only permitted one open data reader per connection at a time.
            }
        }

        public Target(MySqlConnection con, UInt32 unID)
            : this(GetSingleRowReaderFromSelectCommand(con, SelectAllCommandBasedOnID(unID)), true)
        {
            FinishConstruction(con);
        }

        // FinishConstruction()
        // Call this method separately after constructing a collection of Target objects from the same MySqlDataReader.
        // This method is necessary as a separate method because MySQL does not allow a connection to have
        // more that one reader open at the same time, and we need to select from more than one table in order to construct a Target object.

        public void FinishConstruction(MySqlConnection con)
        {
            // Read from tables other than the Targets table.
            m_collTargetFormFields.LoadFromDatabase(con, TargetID);

            /*
            // Populate m_nvcTargetFormFields.
            string strCommandText = string.Format(@"SELECT FieldName, FieldValue FROM TargetFormFields WHERE TargetID={0};", TargetID);

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    m_nvcTargetFormFields.Clear();

                    while (reader.Read())
                    {
                        m_nvcTargetFormFields.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
            }
             */
        }

        // **** Implementation of IComparable<Target> ****

        public int CompareTo(Target other)
        {
            // In the priority queue, the Target object with the smallest DateTimeOfNextLoginTest should be at the head.

            if (DateTimeOfNextMonitor > other.DateTimeOfNextMonitor)
            {
                return -1;
            }

            if (DateTimeOfNextMonitor < other.DateTimeOfNextMonitor)
            {
                return 1;
            }

            return 0;
        }

        // **** Member accessor properties ****

        public UInt32 TargetID
        {
            get
            {
                return ID;
            }
            set
            {
                ID = value;
            }
        }

        public UInt32 AccountID
        {
            get
            {
                return m_unAccountID;
            }
            set
            {
                m_unAccountID = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return m_bEnabled;
            }
            set
            {
                m_bEnabled = value;
            }
        }

        public string URL
        {
            get
            {
                return m_strURL;
            }
            set
            {
                m_strURL = value;
            }
        }

        public int MonitorIntervalAsInt
        {
            get
            {
                return m_nMonitorInterval;
            }
            set
            {
                m_nMonitorInterval = value;
            }
        }

        public TimeSpan MonitorIntervalAsTimeSpan
        {
            get
            {
                return m_tsMonitorInterval;
            }
            set
            {
                m_tsMonitorInterval = value;
            }
        }

        public bool LastMonitoredAtIsNull
        {
            get
            {
                return !m_ndtLastMonitoredAt.HasValue;
            }
        }

        public DateTime? LastMonitoredAt     // Use nullable DateTime? ?
        {
            get
            {
                return m_ndtLastMonitoredAt;
            }
            set
            {
                m_ndtLastMonitoredAt = value;
            }
        }

        public string LastMonitoredAtAsUIString     // ... as opposed to a string formatted for a database insert or update.
        {
            get
            {

                if (LastMonitoredAtIsNull)
                {
                    return @"(null)";
                }

                return LastMonitoredAt.ToString();
            }
        }

        public DateTime DateTimeOfNextMonitor
        {
            get
            {
                return m_dtNextMonitor;
            }
            set
            {
                m_dtNextMonitor = value;
            }
        }

        public MonitorType MonitorType
        {
            get
            {
                return m_eMonitorType;
            }
            set
            {
                m_eMonitorType = value;
            }
        }

        public uint LastTargetLogID
        {
            get
            {
                return m_unLastTargetLogID;
            }
            set
            {
                m_unLastTargetLogID = value;
            }
        }

        public DateTime TargetAddedAt
        {
            get
            {
                return m_dtTargetAddedAt;
            }
        }

        public bool LastFailedAtIsNull
        {
            get
            {
                return !m_ndtLastFailedAt.HasValue;
            }
        }

        public DateTime? LastFailedAt
        {
            get
            {
                return m_ndtLastFailedAt;
            }
        }

        public string LastFailedAtAsUIString     // ... as opposed to a string formatted for a database insert or update.
        {
            get
            {

                if (LastFailedAtIsNull)
                {
                    return @"(null)";
                }

                return LastFailedAt.ToString();
            }
        }

        public TargetFormFieldCollection FormFields
        {
            get
            {
                return m_collTargetFormFields;
            }
        }

        // **** SQL command string properties and methods ****

        public static string SelectAllCommand
        {
            get
            {
                return @"SELECT TargetID, AccountID, Enabled, Name, URL, MonitorInterval, LastMonitoredAt, MonitorType, LastTargetLogID, TargetAddedAt, LastFailedAt FROM Targets;";
            }
        }

        public static string SelectIDAndNameCommand
        {
            get
            {
                return @"SELECT TargetID, Name FROM Targets ORDER BY Name;";
            }
        }

        public static string SelectAllCommandBasedOnID(UInt32 unID)
        {
            return string.Format(@"SELECT TargetID, AccountID, Enabled, Name, URL, MonitorInterval, LastMonitoredAt, MonitorType, LastTargetLogID, TargetAddedAt, LastFailedAt FROM Targets WHERE TargetID={0};", unID);
        }

        private string LastMonitoredAtValueAsDatabaseString
        {
            get
            {
                return MySqlUtils.NullableDateTimeToDatabaseString(m_ndtLastMonitoredAt);
            }
        }

        public static MonitorType DefaultMonitorType
        {
            get
            {
                return MonitorType.eHTTPPost;
            }
        }

        public static uint DefaultMonitorTypeAsUInt
        {
            get
            {
                MonitorTypeCollectionMember mtcm = MonitorTypeCollection.FindUsingMonitorType(DefaultMonitorType);

                return mtcm.EquivalentUInt;
            }
        }

        private uint MonitorTypeAsUInt
        {
            get
            {
                MonitorTypeCollectionMember mtcm = MonitorTypeCollection.FindUsingMonitorType(MonitorType);

                if (mtcm == null)
                {
                    return DefaultMonitorTypeAsUInt;
                }

                return mtcm.EquivalentUInt;
            }
        }

        private string LastFailedAtValueAsDatabaseString
        {
            get
            {
                return MySqlUtils.NullableDateTimeToDatabaseString(m_ndtLastFailedAt);
            }
        }

        public override string InsertCommand
        {
            get
            {
                return string.Format(@"INSERT INTO Targets VALUES (NULL, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}); SELECT LAST_INSERT_ID() FROM Targets;",
                    AccountID,
                    MySqlUtils.BooleanToDatabaseString(Enabled),
                    MySqlUtils.RawStringToDatabaseString(Name),
                    MySqlUtils.RawStringToDatabaseString(URL),
                    MonitorIntervalAsInt,     // ? Do we need to single-quote this in the format string above?  No.
                    LastMonitoredAtValueAsDatabaseString,
                    MonitorTypeAsUInt,
                    LastTargetLogID,
                    MySqlUtils.DateTimeToDatabaseString(TargetAddedAt),
                    LastFailedAtValueAsDatabaseString);
            }
        }

        public override string UpdateCommand
        {
            get
            {
                return string.Format(@"UPDATE Targets SET AccountID={0}, Enabled={1}, Name={2}, URL={3}, MonitorInterval={4}, LastMonitoredAt={5}, MonitorType={6}, LastTargetLogID={7}, TargetAddedAt={8}, LastFailedAt={9} WHERE TargetID={10};",
                    AccountID,
                    MySqlUtils.BooleanToDatabaseString(Enabled),
                    MySqlUtils.RawStringToDatabaseString(Name),
                    MySqlUtils.RawStringToDatabaseString(URL),
                    MonitorIntervalAsInt,
                    LastMonitoredAtValueAsDatabaseString,
                    MonitorTypeAsUInt,
                    LastTargetLogID,
                    MySqlUtils.DateTimeToDatabaseString(TargetAddedAt),
                    LastFailedAtValueAsDatabaseString,
                    TargetID);
            }
        }

        public string UpdateLastMonitoredAtCommand
        {
            get
            {
                return string.Format(@"UPDATE Targets SET LastMonitoredAt={0} WHERE TargetID={1};",
                    LastMonitoredAtValueAsDatabaseString,
                    TargetID);
            }
        }

        public string UpdateLastTargetLogIDCommand
        {
            get
            {
                return string.Format(@"UPDATE Targets SET LastTargetLogID={0} WHERE TargetID={1};",
                    LastTargetLogID,
                    TargetID);
            }
        }

        public override string DeleteCommand
        {
            get
            {
                return string.Format(@"DELETE FROM Targets WHERE TargetID={0};", TargetID);
            }
        }

        // **** Methods ****

        public void UpdateLastMonitoredAt(DateTime dt, MySqlConnection con)
        {
            LastMonitoredAt = dt;

            MySqlUtils.ExecuteNonQuery(con, UpdateLastMonitoredAtCommand);
        }

        public void UpdateLastTargetLogID(uint unLogID, MySqlConnection con)
        {
            LastTargetLogID = unLogID;

            MySqlUtils.ExecuteNonQuery(con, UpdateLastTargetLogIDCommand);
        }

        /*
        private void InsertTargetFormFields(MySqlConnection con)
        {

            for (int i = 0; i < m_nvcTargetFormFields.Count; ++i)
            {
                string strCommandText = string.Format(@"INSERT INTO TargetFormFields VALUES ({0}, '{1}', '{2}');",
                    TargetID,
                    m_nvcTargetFormFields.GetKey(i),
                    m_nvcTargetFormFields[i]);

                MySqlUtils.ExecuteNonQuery(con, strCommandText);
            }
        }

        private void DeleteTargetFormFields(MySqlConnection con)
        {
            string strCommandText = string.Format(@"DELETE FROM TargetFormFields WHERE TargetID={0};", TargetID);

            MySqlUtils.ExecuteNonQuery(con, strCommandText);
        }
         */

        protected override void ValidateDataBeforeSave()    // Throw an informative exception if any data is invalid.
        {
            /*
                    MySqlUtils.RawStringToDatabaseString(Name),
                    MySqlUtils.RawStringToDatabaseString(URL),
                    MonitorIntervalAsInt,     // ? Do we need to single-quote this in the format string above?  No.
                    LastMonitoredAtValueAsDatabaseString,
                    MonitorTypeAsUInt,
                    LastTargetLogID,
                    MySqlUtils.DateTimeToDatabaseString(TargetAddedAt),
                    LastFailedAtValueAsDatabaseString);
             */

            if (Name == null)
            {
                throw new Exception(@"Cannot save the target information: The target name is null.");
            }
            else if (Name == string.Empty)
            {
                throw new Exception(@"Cannot save the target information: The target name is empty.");
            }

            if (URL == null)
            {
                throw new Exception(@"Cannot save the target information: The target URL is null.");
            }
            else if (URL == string.Empty)
            {
                throw new Exception(@"Cannot save the target information: The target URL is empty.");
            }

            if (MonitorIntervalAsInt <= 0)
            {
                throw new Exception(@"Cannot save the target information: The monitor interval must be greater than zero seconds.");
            }
        }

        protected override void AdditionalInsertCode(MySqlConnection con)
        {
            m_collTargetFormFields.SaveToDatabase(con);
        }

        protected override void AdditionalUpdateCode(MySqlConnection con)
        {
            m_collTargetFormFields.SaveToDatabase(con);
        }

        protected override void AdditionalDeleteCode(MySqlConnection con)
        {
            m_collTargetFormFields.DeleteFromDatabase(con, TargetID);
        }
    } // class Target
} // namespace FlareLib
