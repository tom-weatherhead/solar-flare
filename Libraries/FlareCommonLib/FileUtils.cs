// FileUtils.cs - Tom Weatherhead - September 11, 2009

using SolarFlareCommon.CodeProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;                  // For StringBuilder

namespace SolarFlareCommon
{
    // ThAW TODO : Create a class SolarFlareCommonCopyExceptionBase with a protected constructor that takes a "string strMessage"
    // parameter as well as paths and checksums.

    public class SolarFlareCommonCopyExceptionBase : Exception
    {
        private readonly string m_strSourcePath = null;
        private readonly string m_strDestPath = null;
        private readonly string m_strSourceChecksum = null;
        private readonly string m_strDestChecksum = null;

        protected SolarFlareCommonCopyExceptionBase(string strMessage, string strSourcePath, string strDestPath, string strSourceChecksum, string strDestChecksum)
            : base(strMessage)
        {
            m_strSourcePath = strSourcePath;
            m_strDestPath = strDestPath;
            m_strSourceChecksum = strSourceChecksum;
            m_strDestChecksum = strDestChecksum;
        }

        public string SourcePath
        {
            get
            {
                return m_strSourcePath;
            }
        }

        public string DestPath
        {
            get
            {
                return m_strDestPath;
            }
        }

        public string SourceChecksum
        {
            get
            {
                return m_strSourceChecksum;
            }
        }

        public string DestChecksum
        {
            get
            {
                return m_strDestChecksum;
            }
        }
    }

    public sealed class SolarFlareCommonCopyOverwriteForbiddenException : SolarFlareCommonCopyExceptionBase
    {
        // Here, strSourceChecksum is actually computed by SolarFlareCommonCopy; it is not the client-supplied version.
        public SolarFlareCommonCopyOverwriteForbiddenException(string strSourcePath, string strDestPath, string strSourceChecksum, string strDestChecksum)
            : base(
                string.Format(
                    @"Source file '{0}' differs from destination file '{1}', but overwriting is forbidden by the caller",
                    strSourcePath, strDestPath),
                strSourcePath, strDestPath, strSourceChecksum, strDestChecksum)
        {
        }
    }

    public static class FileUtils
    {
        private static readonly List<char> m_lcFileSystemSafeCharacters = null;

        static FileUtils()
        {
            const string strFileSystemSafeCharacters = @" 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_-.";

            m_lcFileSystemSafeCharacters = new List<char>();

            for (int i = 0; i < strFileSystemSafeCharacters.Length; ++i)
            {
                m_lcFileSystemSafeCharacters.Add(strFileSystemSafeCharacters[i]);
            }

            m_lcFileSystemSafeCharacters.Sort();
        }

        // ThAW : If strClientSuppliedSourceChecksum is not null, then we are trusting that the user has computed it correctly.

        public static string SolarFlareCommonCopy(string strSourcePath, string strDestPath,
            bool bOverwrite, bool bDeleteSourceFile, string strClientSuppliedSourceChecksum)
        {
            const string strThisFunctionName = @"SolarFlareCommon.FileUtils.SolarFlareCommonCopy()";
            string strSourceChecksum = strClientSuppliedSourceChecksum;

            // 1) Ensure that the source file exists.

            if (!File.Exists(strSourcePath))
            {
                //throw new FileNotFoundException(strThisFunctionName + @" : Source file not found", strSourcePath);
                // ThAW 2009/01/12 : Indicate which file was not found.
                throw new FileNotFoundException(
                    string.Format(@"{0} : Source file not found : {1}", strThisFunctionName, strSourcePath),
                    strSourcePath);
            }

            /*
            string strSourceChecksum = MD5.ComputeChecksum(strSourcePath);

            if (strSourceChecksum == null)
            {
                throw new Exception(strThisFunctionName + @" : The checksum of " + strSourcePath + @" is null");
            }
            */

            // 2) Ensure that the destination directory exists
            string strDestDir = Path.GetDirectoryName(strDestPath);

            // From the online help: "The Exists method does not perform network authentication.
            //   If you query an existing network share without being pre-authenticated, the Exists method will return false."

            if (!Directory.Exists(strDestDir))
            {
                Directory.CreateDirectory(strDestDir);
            }
            // 3) Do not bother copying if the destination file already exists and it has the same checksum as the source file.
            else if (bOverwrite || !File.Exists(strDestPath))
            {
                // Do nothing here; proceed with the attempt to copy the file.
            }
            else
            {
                // The destination file exists, but we must not overwrite it.  Thus, either:
                // 1) The source file and destination file are already the same, in which case we have succeeded; or
                // 2) They differ, in which case we have a fatal error.
                bool bSourceChecksumComputedLocally = false;

                if (strSourceChecksum == null)
                {
                    strSourceChecksum = MD5.ComputeChecksum(strSourcePath);
                    bSourceChecksumComputedLocally = true;
                }

                string strDestChecksum = MD5.ComputeChecksum(strDestPath);

                if (strSourceChecksum == strDestChecksum)
                {
                    // This will return inappropriately if there is an MD5 collision,
                    // but the probability of that should be extremely small.
                    return strSourceChecksum;
                }
                else
                {
                    // Assert(!bOverwrite && File.Exists(strDestPath) && !bChecksumsAreEqual);
                    // Both files exist, and they differ, but we are not allowed to overwrite.  Throw an exception.

                    if (!bSourceChecksumComputedLocally)
                    {
                        strSourceChecksum = MD5.ComputeChecksum(strSourcePath);
                    }

                    throw new SolarFlareCommonCopyOverwriteForbiddenException(strSourcePath, strDestPath, strSourceChecksum, strDestChecksum);
                }
            }

            try
            {
                // 4) Copy the file; throw an exception upon failure
                File.Copy(strSourcePath, strDestPath, bOverwrite);

                // 5) Compute the MD5 checksums of the source and dest files; throw an exception if they're different

                if (strSourceChecksum == null)
                {
                    strSourceChecksum = MD5.ComputeChecksum(strSourcePath);
                }

                string strDestChecksum = MD5.ComputeChecksum(strDestPath);

                if (strSourceChecksum != strDestChecksum)
                {
                    //throw new SolarFlareCommonCopyUnequalChecksumsException(strSourcePath, strDestPath, strSourceChecksum, strDestChecksum);
                    throw new Exception(strThisFunctionName + @" : The checksum of " + strDestPath + @" does not match the checksum of " + strSourcePath);
                }
            }
            catch (Exception ex)
            {

                try
                {
                    // The copy operation has failed; clean up.

                    if (File.Exists(strDestPath))
                    {
                        File.Delete(strDestPath);
                    }
                }
                catch
                {
                    // Eat any exceptions; we don't want to throw a second exception while we're handling the first.
                }

                throw ex;   // Re-throw the exception that was caught at the beginning of this block.
            }

            // 6) If bDeleteSourceFile then delete source file (and throw an exception upon failure)

            if (bDeleteSourceFile)
            {
                File.Delete(strSourcePath);
            }

            // 7) Return the MD5 checksum
            return strSourceChecksum;
        }

        public static string SolarFlareCommonCopy(string strSourcePath, string strDestPath, bool bOverwrite, bool bDeleteSourceFile)
        {
            return SolarFlareCommonCopy(strSourcePath, strDestPath, bOverwrite, bDeleteSourceFile, null);
        }

        // Mapping and disconnecting network drives

        public static NetworkDrive MapNetworkDrive(     // Overload #1
            string strLocalDrive,   // The first character of strLocalDrive is used as the logical drive letter.
            string strShareName, string strUserName, string strPassword)
        {
            const string strForbiddenDriveLetters = @"abcdef";  // These letters must all be lower case.
            char cDriveLetter = strLocalDrive[0];

            if (strForbiddenDriveLetters.IndexOf(cDriveLetter) >= 0 ||
                strForbiddenDriveLetters.ToUpper().IndexOf(cDriveLetter) >= 0)
            {
                throw new Exception(@"'" + cDriveLetter.ToString() + @"' is not an acceptable mapped drive letter in the DAM system.");
            }

            NetworkDrive drive = new NetworkDrive();

            drive.LocalDrive = strLocalDrive;   // e.g. @"z:"
            drive.ShareName = strShareName;     // e.g. @"\\ComputerName\ShareName"
            drive.MapDrive(strUserName, strPassword);
            return drive;
        }

        public static NetworkDrive MapNetworkDrive(string strLocalDrive, string strShareName)   // Overload #2
        {
            return MapNetworkDrive(strLocalDrive, strShareName, null, null);
        }

        public static void DisconnectNetworkDrive(NetworkDrive drive)       // Overload #1
        {
            drive.UnMapDrive();
        }

        public static void DisconnectNetworkDrive(string strLocalDrive)     // Overload #2
        {
            NetworkDrive drive = new NetworkDrive();

            drive.LocalDrive = strLocalDrive;   // e.g. @"z:"
            DisconnectNetworkDrive(drive);
        }

        private static bool IsAFileSystemSafeCharacter(char c)
        {
            return (m_lcFileSystemSafeCharacters.BinarySearch(c) >= 0);
        }

        public static string SimpleGetFileSystemSafeString(string strIn)
        {

            if (strIn == null)
            {
                throw new ArgumentNullException(
                    @"strIn",
                    @"SolarFlareCommon.FileUtils.SimpleGetFileSystemSafeString() was passed a null string reference");
            }

            // Simply omit unacceptable characters.
            // A faster solution would be to insert the acceptable characters into a height-balanced search tree
            // so that a search in a set of n characters takes O(n) time.  This search tree could be
            // a static member, and it could be initialized by a static constructor.
            //const string strAcceptableCharacters = @" 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_-";
            StringBuilder sbOut = new StringBuilder();

            for (int i = 0; i < strIn.Length; ++i)
            {
                char c = strIn[i];

                //if (strAcceptableCharacters.IndexOf(c) >= 0)
                //if (m_lcFileSystemSafeCharacters.Contains(c))
                //if (m_lcFileSystemSafeCharacters.BinarySearch(c) >= 0)
                if (IsAFileSystemSafeCharacter(c))
                {
                    sbOut.Append(c);
                }
            }

            return sbOut.ToString();
        }

        //public static string InvertibleGetFileSystemSafeString(string strIn)
        //public static string UndoInvertibleGetFileSystemSafeString(string strIn)
    }
}
