// MailUtils.cs - By Tom Weatherhead - June 9, 2009

using System;
using System.Collections.Generic;
using System.Net;                   // For CredentialCache
using System.Net.Mail;
using System.Text;

namespace SolarFlareCommon
{
    static public class MailUtils
    {
        private static void AddMailRecipients(MailAddressCollection mac, string strRecipients)
        {

            foreach (string strRecipient in strRecipients.Split(',', ';'))
            {
                string strRecipientTrimmed = strRecipient.Trim();

                if (strRecipientTrimmed == string.Empty)
                {
                    continue;
                }

                MailAddress ma = new MailAddress(strRecipientTrimmed);

                mac.Add(ma);
            }
        }

        public static void SendMail(string strServer, string strFrom, string strTo, string strCC, string strBCC,
            string strSubject, string strBody)
        {
            // Create a message and set up the recipients.
            // See the online help for System.Net.Mail.MailMessage for an example of how to add an attachment.
            MailMessage message = new MailMessage(strFrom, strTo, strSubject, strBody);

            // Add more recipients?
            //message.To.Add(MailAddress item);
            //message.CC.Add(MailAddress item);
            AddMailRecipients(message.CC, strCC);
            //message.Bcc.Add(MailAddress item);
            AddMailRecipients(message.Bcc, strBCC);

            // Send the message.
            SmtpClient client = new SmtpClient(strServer);

            // Add credentials if the SMTP server requires them.
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            client.Send(message);
        }
    } // static public class MailUtils
} // namespace SolarFlareCommon

// **** End of File ****
