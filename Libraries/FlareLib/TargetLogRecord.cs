// FlareLib\TargetLogRecord.cs - By Tom Weatherhead - August 24, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public enum TargetLogRecordStatus
    {
        Fail,
        Pass
    }

    public sealed class TargetLogRecord
    {
        private uint m_unLogID;    // Column: LogID (INT) (Primary key) (Auto increment)
        private readonly uint m_unTargetID; // Column: TargetID (INT)
        private readonly DateTime m_dtTimeStamp;    // Column: TimeStamp (DATETIME)
        private readonly TargetLogRecordStatus m_eStatus;   // Column: Status (monitor pass, monitor fail) (CHAR(4) : "PASS" or "FAIL")
        private readonly string m_strMessage;   // Column: Message (verbose message containing extra info or http return strings) (TEXT; equivalent to an Oracle CLOB)
        private readonly uint m_unErrorCode;    // Column: ErrorCode (HTTP error code) (INT)
        private readonly uint m_unLocationID;   // Column: LocationID (location target was monitored from) (Locations stored in the SystemConfiguration table as "Location1", etc.) (INT)
        private readonly uint m_unResponseTime; // Column: ResponseTime (time it took to receive a response for this monitor activity) (seconds or milliseconds?) (INT)

        public TargetLogRecord(uint unTargetID, DateTime dtTimeStamp, TargetLogRecordStatus eStatus,
            string strMessage, uint unErrorCode, uint unLocationID, uint unResponseTime)
        {
            m_unLogID = 0;  // This will be set to a nonzero value upon insert into the database
            m_unTargetID = unTargetID;
            m_dtTimeStamp = dtTimeStamp;
            m_eStatus = eStatus;
            m_strMessage = strMessage;
            m_unErrorCode = unErrorCode;
            m_unLocationID = unLocationID;
            m_unResponseTime = unResponseTime;
        }

        private TargetLogRecord(MySqlDataReader reader)
        {
            m_unLogID = reader.GetUInt32(0);
            m_unTargetID = reader.GetUInt32(1);
            m_dtTimeStamp = reader.GetDateTime(2);
            m_eStatus = StatusStringToEnum(reader.GetString(3));
            m_strMessage = reader.GetString(4);
            m_unErrorCode = reader.GetUInt32(5);
            m_unLocationID = reader.GetUInt32(6);
            m_unResponseTime = reader.GetUInt32(7);
        }

        private static TargetLogRecordStatus StatusStringToEnum(string str)
        {

            if (str == @"PASS")
            {
                return TargetLogRecordStatus.Pass;
            }
            else if (str == @"FAIL")
            {
                return TargetLogRecordStatus.Fail;
            }
            else
            {
                throw new Exception(string.Format(@"FlareLib.TargetLogRecord.StatusStringToEnum() : Unrecognized parameter '{0}'", str));
            }
        }

        private static string StatusEnumToString(TargetLogRecordStatus eStatus)
        {

            switch (eStatus)
            {
                case TargetLogRecordStatus.Pass:
                    return @"PASS";

                case TargetLogRecordStatus.Fail:
                    return @"FAIL";

                default:
                    throw new Exception(string.Format(@"FlareLib.TargetLogRecord.StatusEnumToString() : Unrecognized parameter '{0}'", eStatus));
            }
        }

        public uint LogID
        {
            get
            {
                return m_unLogID;
            }
        }

        public uint TargetID
        {
            get
            {
                return m_unTargetID;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return m_dtTimeStamp;
            }
        }

        public TargetLogRecordStatus Status
        {
            get
            {
                return m_eStatus;
            }
        }

        public string Message
        {
            get
            {
                return m_strMessage;
            }
        }

        public uint ErrorCode
        {
            get
            {
                return m_unErrorCode;
            }
        }

        public uint LocationID
        {
            get
            {
                return m_unLocationID;
            }
        }

        public uint ResponseTime
        {
            get
            {
                return m_unResponseTime;
            }
        }

        public static string GetSelectCommand(uint unTargetID, uint unAccountID, bool bFailedOnly, DateTime? ndtStartTimeStamp, DateTime? ndtEndTimeStamp)
        {
            StringBuilder sb = new StringBuilder();
            //string strSelectCommand = @"SELECT LogID, TargetID, TimeStamp, Status, Message, ErrorCode, LocationID, ResponseTime FROM TargetLog";
            string strSeparatorWord = @" WHERE ";
            string strAnd = @" AND ";

            sb.Append(@"SELECT tl.LogID, tl.TargetID, tl.TimeStamp, tl.Status, tl.Message, tl.ErrorCode, tl.LocationID, tl.ResponseTime FROM TargetLog tl");

            if (unTargetID > 0)
            {
                sb.Append(strSeparatorWord);
                sb.Append(string.Format(@"tl.TargetID={0}", unTargetID));
                strSeparatorWord = strAnd;
            }
            else if (unAccountID > 0)
            {
                sb.Append(@", Targets t");
                sb.Append(strSeparatorWord);
                sb.Append(string.Format(@"tl.TargetID=t.TargetID AND t.AccountID={0}", unAccountID));
                strSeparatorWord = strAnd;
            }

            if (bFailedOnly)
            {
                sb.Append(strSeparatorWord);
                sb.Append(@"tl.Status LIKE 'FAIL'");
                strSeparatorWord = strAnd;
            }

            if (ndtStartTimeStamp.HasValue)
            {
                sb.Append(strSeparatorWord);
                sb.Append(string.Format(@"tl.TimeStamp >= {0}", MySqlUtils.DateTimeToDatabaseString(ndtStartTimeStamp.Value)));
                strSeparatorWord = strAnd;
            }

            if (ndtEndTimeStamp.HasValue)
            {
                sb.Append(strSeparatorWord);
                sb.Append(string.Format(@"tl.TimeStamp <= {0}", MySqlUtils.DateTimeToDatabaseString(ndtEndTimeStamp.Value)));
                strSeparatorWord = strAnd;
            }

            sb.Append(@" ORDER BY tl.TimeStamp;");
            return sb.ToString();
        }

        public static List<TargetLogRecord> GetRecords(MySqlConnection con, string strCommandText)
        {

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    List<TargetLogRecord> lstRecords = new List<TargetLogRecord>();

                    while (reader.Read())
                    {
                        lstRecords.Add(new TargetLogRecord(reader));
                    }

                    return lstRecords;
                }
            }
        }

        private string InsertCommand
        {
            get
            {
                return string.Format(@"INSERT INTO TargetLog VALUES (NULL, {0}, {1}, {2}, {3}, {4}, {5}, {6}); SELECT LAST_INSERT_ID() FROM TargetLog;",
                    TargetID,
                    MySqlUtils.DateTimeToDatabaseString(TimeStamp),
                    MySqlUtils.RawStringToDatabaseString(StatusEnumToString(Status)),
                    MySqlUtils.RawStringToDatabaseString(Message),  // This will wrap the string in single quotes, and double any single quotes within.
                    ErrorCode,
                    LocationID,
                    ResponseTime);
            }
        }

        public void Insert(MySqlConnection con)
        {

            if (LogID > 0)
            {
                return;
            }

            lock (con)  // To prevent different threads from trying to create and use readers associated with the same connection at the same time.
            {
                string strCommandText = InsertCommand;

                using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
                {

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {

                        if (!reader.Read())
                        {
                            throw new Exception(@"Insert: 'SELECT LAST_INSERT_ID()' seems to have failed");
                        }

                        m_unLogID = reader.GetUInt32(0);
                    }
                }
            }
        }
    } // class TargetLogRecord
} // namespace FlareLib

// **** End of File ****
