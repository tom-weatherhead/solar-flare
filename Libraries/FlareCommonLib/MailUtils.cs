// MailUtils.cs - Tom Weatherhead - September 11, 2009

using System;
using System.Collections.Generic;
using System.Net;                   // For CredentialCache and NetworkCredential
using System.Net.Mail;
using System.Text;

namespace SolarFlareCommon
{
    public static class MailUtils
    {
        private static void AddMailRecipients(MailAddressCollection mac, string strRecipients)
        {

            if (StringUtils.IsDegenerateString(strRecipients))
            {
                return;
            }

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

        public static void SendMail(string strServer, string strServerUserName, string strServerPassword,
            string strFrom, string strTo, string strCC, string strBCC, string strSubject, string strBody)
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
            SmtpClient client = new SmtpClient(strServer /* , int port */ );

            // Add credentials if the SMTP server requires them.
            // ThAW 2009/09/11 : We will have to find a way to log in to the SMTP server (GMail, Sympatico, or whatever).
            // Flare: The credentials should probably be loaded from the database by the caller, and then passed in here.

            if (!StringUtils.IsDegenerateString(strServerUserName) && !StringUtils.IsDegenerateString(strServerPassword))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(strServerUserName, strServerPassword);   // Credentials will be sent in the clear.
            }
            else
            {
                // The following is apparently the same as setting client.UseDefaultCredentials to true:
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            client.Send(message);
        }
    } // static public class MailUtils
} // namespace Generator2

// **** End of File ****
