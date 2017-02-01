// Solar Flare General Test Application - By Tom Weatherhead - June 26, 2009

//#define USE_MYSQL_DATABASE
//#define SELECT_ALL_FLARE_ACCOUNTS
//#define SEND_DATA_DRIVEN_EMAIL

//#define TEST_STRING_FORMAT_LEADING_ZEROES
//#define TEST_WRITE_FORMATTED_DATETIME
//#define TEST_PARSE_STRING_AS_DATETIME
//#define TEST_PING

using FlareLib;
using SolarFlareCommon;
using MySql.Data.MySqlClient;
//using MySql.Data.MySqlClient.Properties;
//using MySql.Data.Types;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace GeneralTestApp
{
    public static class Program
    {
#if USE_MYSQL_DATABASE
#if SELECT_ALL_FLARE_ACCOUNTS
        private static void FlareSelectAllAccounts(MySqlConnection con)
        {
            string strCommandText = Account.SelectAllCommand;

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {
                //connection.Open();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        //Console.WriteLine(String.Format("{0}, {1}", reader[0], reader[1]));

                        Account acct = new Account(reader, false);

                        Console.WriteLine(string.Format(@"Account ID={0}, Enabled={1}, Name='{2}'.", acct.AccountID, acct.Enabled, acct.Name));
                    }
                }
            }
        }
#endif

#if SEND_DATA_DRIVEN_EMAIL
        private static void SendDataDrivenEmail(MySqlConnection con)
        {
            Dictionary<string, string> dictFlareSystemConfigurationEntries = SystemConfigurationEntry.GetDictionary(con);

            if (!dictFlareSystemConfigurationEntries.ContainsKey(SystemConfigurationEntryKeys.SMTPServerAddress))
            {
                throw new Exception(@"Flare system configuration error: No SMTP server address is specified");
            }

            if (!dictFlareSystemConfigurationEntries.ContainsKey(SystemConfigurationEntryKeys.SMTPServerUserName))
            {
                throw new Exception(@"Flare system configuration error: No SMTP server user name is specified");
            }

            if (!dictFlareSystemConfigurationEntries.ContainsKey(SystemConfigurationEntryKeys.SMTPServerPassword))
            {
                throw new Exception(@"Flare system configuration error: No SMTP server password is specified");
            }

            string strSMTPServerAddress = dictFlareSystemConfigurationEntries[SystemConfigurationEntryKeys.SMTPServerAddress];
            string strSMTPServerUserName = dictFlareSystemConfigurationEntries[SystemConfigurationEntryKeys.SMTPServerUserName];
            string strSMTPServerPassword = dictFlareSystemConfigurationEntries[SystemConfigurationEntryKeys.SMTPServerPassword];
            const string strFrom = @"from.address@domain.ca";
            const string strTo = @"to.address@domain.com";
            const string strSubject = @"Data-driven e-mail test";
            string strBody = string.Format(@"Hello Tom; It is now {0} Universal Time.",
                MySqlUtils.DateTimeToString(DateTime.UtcNow));

            Console.WriteLine(@"About to send data-driven e-mail to {0}...", strTo);
            MailUtils.SendMail(strSMTPServerAddress, strSMTPServerUserName, strSMTPServerPassword,
                strFrom, strTo, null, null, strSubject, strBody);
            Console.WriteLine(@"E-mail sent.");
        }
#endif

        private static void ConnectToMySqlDatabase()
        {
            IMySqlConnectionProvider cp = new FlareUncachedMySqlConnectionProvider();   // Uncached or simply cached?

            using (MySqlConnection con = cp.GetMySqlConnection())
            {
#if SELECT_ALL_FLARE_ACCOUNTS
                FlareSelectAllAccounts(con);
#endif

#if SEND_DATA_DRIVEN_EMAIL
                SendDataDrivenEmail(con);
#endif
            }
        }
#endif

#if TEST_PING
        // args[0] can be an IPaddress or host name.

        public static void Ping(string[] args)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;  // Milliseconds
            PingReply reply = pingSender.Send(args[0], timeout, buffer, options);

            Console.WriteLine(@"Reply status: {0}", reply.Status);

            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("Address: {0}", reply.Address.ToString());
                Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
            }
        }
#endif

        public static void Main(string[] args)
        {
            Console.WriteLine(@"GeneralTestApp: Begin");

            try
            {
                /*
                // Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;
                const string strConnectionString = @"Server=localhost;Database=flare;Uid=flare;Pwd=tomtom7";

                Console.WriteLine(@"Attempting to connect to MySQL database server...");

                using (MySqlConnection con = new MySqlConnection(strConnectionString))
                {
                    con.Open();
                    Console.WriteLine(string.Format(@"Connected to MySQL database server version {0}", con.ServerVersion));
                    con.Close();
                }
                 */

#if USE_MYSQL_DATABASE
                ConnectToMySqlDatabase();
#endif

#if TEST_STRING_FORMAT_LEADING_ZEROES
                Console.WriteLine(string.Format(@"Leading zero test: '{0:0000}'", 13));
#endif

#if TEST_WRITE_FORMATTED_DATETIME
                DateTime dt = DateTime.Now;

                Console.WriteLine(string.Format(@"It is now '{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}'",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
#endif

#if TEST_PARSE_STRING_AS_DATETIME
                DateTime dt = DateTime.Now;

                Console.WriteLine(dt.ToString());

                string strDateTimeAsDatabaseString = MySqlUtils.DateTimeToDatabaseString(dt);

                Console.WriteLine(strDateTimeAsDatabaseString);

                string strDateTimeAsString = strDateTimeAsDatabaseString.Substring(1, strDateTimeAsDatabaseString.Length - 2);

                Console.WriteLine(strDateTimeAsString);

                DateTime dt2 = DateTime.Parse(strDateTimeAsString);

                Console.WriteLine(dt2.ToString());
#endif

#if TEST_PING
                Ping(args);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(@"{0} caught: {1}", ex.GetType().FullName, ex.Message));
            }

            Console.WriteLine(@"GeneralTestApp: End");
        }
    } // class Program
} // namespace GeneralTestApp

/*
private static void ReadOrderData(string connectionString)
{
      string commandText = "SELECT OrderID, CustomerID FROM dbo.Orders;";
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                  connection.Open();
                  using (SqlDataReader reader = command.ExecuteReader())
                  {
                        while (reader.Read())
                        {
                              Console.WriteLine(String.Format("{0}, {1}", 
                                reader[0], reader[1]));
                        }
                  }
            }
      }
}
 */

// **** End of File ****
