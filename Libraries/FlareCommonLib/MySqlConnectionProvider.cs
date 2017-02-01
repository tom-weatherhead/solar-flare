// MySqlConnectionProvider.cs - By Tom Weatherhead - June 29, 2009

#define TRY_TO_REOPEN_CONNECTIONS

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Linq;
using System.Text;
using System.Web.SessionState;

namespace SolarFlareCommon
{
    public interface IMySqlConnectionProvider
    {
        MySqlConnection GetMySqlConnection();
    }

    public static class MySqlConnectionHelper
    {
        public static string FlareConnectionString
        {
            get
            {
                return @"Server=localhost;Database=flare;Uid=flare;Pwd=password;";
            }
        }

        public static MySqlConnection GetConnection(string strConnectionString)
        {
            MySqlConnection con = new MySqlConnection(strConnectionString);

            con.Open();
            return con;
        }

        public static MySqlConnection GetFlareConnection()
        {
            return GetConnection(FlareConnectionString);
        }
    } // class MySqlConnectionHelper

    public abstract class CachedMySqlConnectionProvider : IMySqlConnectionProvider
    {
        private readonly string m_strConnectionString;

        public CachedMySqlConnectionProvider(string strConnectionString)
        {
            m_strConnectionString = strConnectionString;
        }

        public abstract MySqlConnection MySqlConnectionCache { get; set; }

        public MySqlConnection GetMySqlConnection()
        {
            MySqlConnection con = MySqlConnectionCache;

            if (con == null)
            {
                con = MySqlConnectionHelper.GetConnection(m_strConnectionString);
                MySqlConnectionCache = con;
                return con;
            }

            if (con.State != ConnectionState.Open)
            {
#if TRY_TO_REOPEN_CONNECTIONS
                bool bGetNewConnection = false;

                try
                {
                    con.Close();
                    con.Open();

                    if (con.State != ConnectionState.Open)
                    {
                        bGetNewConnection = true;
                    }
                }
                catch
                {
                    bGetNewConnection = true;
                }

                if (bGetNewConnection)
                {
                    MySqlConnectionCache = null;
                    con.Dispose();
                    con = MySqlConnectionHelper.GetConnection(m_strConnectionString);
                    MySqlConnectionCache = con;
                }
#else
                MySqlConnectionCache = null;
                con.Dispose();
                con = MySqlConnectionHelper.GetConnection(m_strConnectionString);
                MySqlConnectionCache = con;
#endif
            }

            return con;
        }
    } // class CachedMySqlConnectionProvider

    public class FlareUncachedMySqlConnectionProvider : IMySqlConnectionProvider
    {
        public MySqlConnection GetMySqlConnection()
        {
            return MySqlConnectionHelper.GetFlareConnection();
        }
    }

    public class FlareSimplyCachedMySqlConnectionProvider : CachedMySqlConnectionProvider
    {
        private MySqlConnection m_con = null;

        public FlareSimplyCachedMySqlConnectionProvider()
            : base(MySqlConnectionHelper.FlareConnectionString)
        {
        }

        public override MySqlConnection MySqlConnectionCache
        {
            get
            {
                return m_con;
            }
            set
            {
                m_con = value;
            }
        }
    }

    public class FlareWebSessionCachedMySqlConnectionProvider : CachedMySqlConnectionProvider
    {
        private readonly HttpSessionState m_session;

        public FlareWebSessionCachedMySqlConnectionProvider(HttpSessionState session)
            : base(MySqlConnectionHelper.FlareConnectionString)
        {
            m_session = session;
        }

        private static string ConnectionKeyInWebSession
        {
            get
            {
                return @"SolarFlare_MySqlConnection";
            }
        }

        public override MySqlConnection MySqlConnectionCache
        {
            get
            {

                if (m_session == null)
                {
                    return null;
                }

                return m_session[ConnectionKeyInWebSession] as MySqlConnection;
            }
            set
            {

                if (m_session != null)
                {
                    m_session[ConnectionKeyInWebSession] = value;
                }
            }
        }
    }
} // namespace SolarFlareCommon

// **** End of File ****
