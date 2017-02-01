// IniFile.cs - By Tom Weatherhead - June 1, 2009

#define BOOLEAN_FASLE_TYPO_SUPPORT

using System;
using System.IO;
using System.Collections.Generic;
//using System.Text;

namespace SolarFlareCommon
{
    public class IniFileException : Exception
    {
        private readonly string m_strFilePath;
        private readonly string m_strSection;
        private readonly string m_strKey;

        public IniFileException(string strFilePath, string strSection, string strKey, Exception exInner, string strMessage)
            : base(ConstructMessage(strFilePath, strSection, strKey, strMessage), exInner)
        {
            m_strFilePath = strFilePath;
            m_strSection = strSection;
            m_strKey = strKey;
        }

        public IniFileException(string strFilePath, string strSection, string strKey, string strMessage)
            : this(strFilePath, strSection, strKey, null, strMessage)
        {
        }

        static private string ConstructMessage(string strFilePath, string strSection, string strKey, string strMessage)
        {
            return string.Format(@"File '{0}', section '{1}', key '{2}' : {3}", strFilePath, strSection, strKey, strMessage);
        }

        public string FilePath
        {
            get
            {
                return m_strFilePath;
            }
        }

        public string Section
        {
            get
            {
                return m_strSection;
            }
        }

        public string Key
        {
            get
            {
                return m_strKey;
            }
        }
    }

    public class IniFile
    {
        private readonly string m_strFilePath;
        // ThAW 2009/06/01 : Could we use something like a DictionaryList<> so that we can do key-based (i.e. string-based) sorting before saving?
        private readonly Dictionary<string, Dictionary<string, string>> m_dictSections;

        public IniFile(string strFilePath)
        {
            StringUtils.ThrowExceptionIfStringArgumentIsDegenerate(@"SolarFlareCommon.IniFile constructor", @"strFilePath", strFilePath);

            m_strFilePath = strFilePath;
            m_dictSections = new Dictionary<string, Dictionary<string, string>>();

            if (!File.Exists(m_strFilePath))
            {
                return;
            }

            StreamReader sr = null;

            try
            {
                sr = File.OpenText(m_strFilePath);

                Dictionary<string, string> dictSection = null;

                for (; ; )
                {
                    string strLine = sr.ReadLine();

                    if (strLine == null)
                    {
                        break;
                    }

                    strLine = strLine.Trim();

                    if (strLine == string.Empty || strLine.StartsWith(@";"))
                    {
                        // The line is a comment; skip it.
                        continue;
                    }

                    // ThAW 2009/06/01 : Since we implicitly disallow a comment from being on the same line
                    // as a section header or a key/value pair, could we just use strLine.EndsWith(@"]") instead of the IndexOf() below,
                    // and then get the section name via strLine.Substring(1, strLine.Length - 2) ?

                    /*
                    int nClosingSquareBracket = strLine.IndexOf(']', 1);
                    bool bNewSection = (strLine.StartsWith(@"[") && nClosingSquareBracket > 0);

                    if (bNewSection)
                    */
                    if (strLine.StartsWith(@"[") && strLine.EndsWith(@"]"))     // This is the header of a new section.
                    {
                        // Determine the name of the section.
                        //string strNewSectionName = strLine.Substring(1, nClosingSquareBracket - 1).Trim();
                        string strNewSectionName = strLine.Substring(1, strLine.Length - 2).Trim();

                        // ThAW 2009/06/01 : What should we do here if strNewSectionName is empty?
                        // (e.g. if the line is "[ ]".)  We could throw an exception, or we could ignore
                        // all keys and values from here to the next valid section header.  We could
                        // control this either/or choice with a "bStrict" flag passed into the constructor,
                        // which would be overloaded: 1) (path), with a default strictness value that would be
                        // passed to the other overload; 2) (path, bStrict).

                        if (strNewSectionName == string.Empty)
                        {
                            // Ignore all keys and values from here to the next valid section header.
                            dictSection = null;
                            continue;
                        }

                        dictSection = GetSection(strNewSectionName);

                        if (dictSection == null)
                        {
                            // Since this is a new section, add a new section object to the collection.
                            dictSection = new Dictionary<string, string>();
                            m_dictSections.Add(strNewSectionName, dictSection);
                        }
                    }
                    else if (dictSection != null)   // Not a new section and not a comment; interpret this line of the file as a key-value pair
                    {
                        int nEqualsSign = strLine.IndexOf('=');

                        if (nEqualsSign > 0)
                        {
                            string strKey = strLine.Substring(0, nEqualsSign).Trim();
                            string strValue = strLine.Substring(nEqualsSign + 1).Trim();

                            if (strKey == string.Empty)
                            {
                                continue;
                            }

                            string strExistingKeyName = StringUtils.FindFirstCaseInsensitiveString(strKey, dictSection.Keys);

                            if (strExistingKeyName == null)
                            {
                                dictSection.Add(strKey, strValue);
                            }
                            else
                            {
                                dictSection[strExistingKeyName] = strValue;
                            }
                        }
                    }
                }
            }
            finally
            {

                if (sr != null)
                {
                    sr.Close();
                }
            }
        }

        public string FilePath
        {
            get
            {
                return m_strFilePath;
            }
        }

        public void Save()
        {
            StreamWriter sw = null;

            try
            {
                bool bFirstSection = true;

                sw = File.CreateText(m_strFilePath);

                foreach (KeyValuePair<string, Dictionary<string, string>> kvp1 in m_dictSections)
                {

                    if (!bFirstSection)
                    {
                        sw.WriteLine();     // Place blank lines between sections.
                    }

                    sw.WriteLine(string.Format(@"[{0}]", kvp1.Key));

                    foreach (KeyValuePair<string, string> kvp2 in kvp1.Value)
                    {
                        sw.WriteLine(string.Format(@"{0}={1}", kvp2.Key, kvp2.Value));
                    }

                    bFirstSection = false;
                }
            }
            finally
            {

                if (sw != null)
                {
                    sw.Close();
                }
            }
        }

        // Methods for working with sections

        public ICollection<string> GetSectionNames()    // was GetSectionKeys()
        {
            return m_dictSections.Keys;
        }

        public void RemoveAllEmptySections()
        {

            foreach (string strSection in m_dictSections.Keys)
            {

                if (m_dictSections[strSection].Count <= 0)
                {
                    m_dictSections.Remove(strSection);
                }
            }
        }

        public bool SectionExists(string strSection)
        {
            return (StringUtils.FindFirstCaseInsensitiveString(strSection, m_dictSections.Keys) != null);
        }

        public void RemoveSection(string strSection)
        {
            string strExistingSectionName = StringUtils.FindFirstCaseInsensitiveString(strSection, m_dictSections.Keys);

            if (strExistingSectionName != null)
            {
                m_dictSections.Remove(strExistingSectionName);
            }
        }

        // Methods for working with keys

        private Dictionary<string, string> GetSection(string strSection)
        {
            StringUtils.ThrowExceptionIfStringArgumentIsDegenerate(@"SolarFlareCommon.IniFile.GetSection()", @"strSection", strSection);

            // Search the section collection for a section of this name.
            string strExistingSectionName = StringUtils.FindFirstCaseInsensitiveString(strSection, m_dictSections.Keys);

            if (strExistingSectionName == null)
            {
                return null;
            }

            return m_dictSections[strExistingSectionName];
        }

        private bool GetSectionAndKeyName(string strSection, string strKey,
            out Dictionary<string, string> dictSection, out string strExistingKeyName)
        {
            dictSection = GetSection(strSection);

            if (dictSection == null)
            {
                strExistingKeyName = null;
                return false;
            }

            strExistingKeyName = StringUtils.FindFirstCaseInsensitiveString(strKey, dictSection.Keys);
            return (strExistingKeyName != null);
        }

        public ICollection<string> GetKeyNames(string strSection)
        {
            Dictionary<string, string> dictSection = GetSection(strSection);

            if (dictSection == null)
            {
                return null;
            }

            return dictSection.Keys;
        }

        public bool KeyExists(string strSection, string strKey)
        {
            Dictionary<string, string> dictSection;
            string strExistingKeyName;

            return GetSectionAndKeyName(strSection, strKey, out dictSection, out strExistingKeyName);
        }

        public void ThrowExceptionIfKeyDoesNotExist(string strSection, string strKey)
        {

            if (!KeyExists(strSection, strKey))
            {
                throw new IniFileException(FilePath, strSection, strKey, string.Format(@"Key '{0}' does not exist.", strKey));
            }
        }

        public void RemoveKey(string strSection, string strKey)   // Remove the key's section if the section is empty after the key has been removed.
        {
            Dictionary<string, string> dictSection;
            string strExistingKeyName;

            if (!GetSectionAndKeyName(strSection, strKey, out dictSection, out strExistingKeyName))
            {
                return;
            }

            dictSection.Remove(strExistingKeyName);

            if (dictSection.Count <= 0)
            {
                // The section is empty; remove it.
                RemoveSection(strSection);
            }
        }

        // Methods for working with values

        public string GetString(string strSection, string strKey, string strDefault)
        {

            if (StringUtils.IsDegenerateString(strSection) ||
                StringUtils.IsDegenerateString(strKey))
            {
                return strDefault;
            }

            Dictionary<string, string> dictSection;
            string strExistingKeyName;

            if (!GetSectionAndKeyName(strSection, strKey, out dictSection, out strExistingKeyName))
            {
                return strDefault;
            }

            return dictSection[strExistingKeyName];
        }

        public string GetString(string strSection, string strKey)
        {
            StringUtils.ThrowExceptionIfStringArgumentIsDegenerate(@"SolarFlareCommon.IniFile.GetString()", @"strSection", strSection);
            StringUtils.ThrowExceptionIfStringArgumentIsDegenerate(@"SolarFlareCommon.IniFile.GetString()", @"strKey", strKey);

            Dictionary<string, string> dictSection;
            string strExistingKeyName;

            if (!GetSectionAndKeyName(strSection, strKey, out dictSection, out strExistingKeyName))
            {
                throw new IniFileException(FilePath, strSection, strKey, string.Format(@"Key '{0}' does not exist.", strKey));
            }

            return dictSection[strExistingKeyName];
        }

        private static bool StringToBool(string strValue)
        {
            strValue = strValue.Trim().ToLower();
            return (strValue != string.Empty && strValue != @"0" && strValue != @"false" && strValue != @"n"
#if BOOLEAN_FASLE_TYPO_SUPPORT
                && strValue != @"fasle"
#endif
                );
        }

        public bool GetBool(string strSection, string strKey, bool bDefault)
        {
            string strValue = GetString(strSection, strKey, null);

            // ThAW Test: 2009/06/01
            //Console.WriteLine(string.Format(@"IniFile.GetBool(): Section {0}, Key {1}", strSection, strKey));

            if (strValue == null)
            {
                //Console.WriteLine(@"-> Value is null");
                return bDefault;
            }

            //Console.WriteLine(@"-> Value is '{0}'", strValue);
            //strValue = strValue.Trim().ToLower();
            //Console.WriteLine(@"-> Trimmed lowercase value is '{0}'", strValue);
            //return (strValue != string.Empty && strValue != @"0" && strValue != @"false" && strValue != @"n");
            return StringToBool(strValue);
        }

        public bool GetBool(string strSection, string strKey)
        {
            return StringToBool(GetString(strSection, strKey));
        }

        public int GetInt(string strSection, string strKey, int nDefault)
        {
            string strValue = GetString(strSection, strKey, null);

            try
            {

                if (strValue != null)
                {
                    return Convert.ToInt32(strValue);
                }
            }
            catch //(Exception ex)
            {
                /* throw new IniFileException(m_strFilePath, strSection, strKey, ex,
                    string.Format(@"Expected an integer value; encountered '{0}' instead.", strValue)); */
            }

            return nDefault;
        }

        public int GetInt(string strSection, string strKey)
        {
            string strValue = GetString(strSection, strKey);

            try
            {
                //return int.Parse(strValue);
                return Convert.ToInt32(strValue);
            }
            catch (Exception ex)
            {
                throw new IniFileException(m_strFilePath, strSection, strKey, ex,
                    string.Format(@"Expected an integer value; encountered '{0}' instead.", strValue));
            }
        }

        public double GetDouble(string strSection, string strKey, double dDefault)
        {
            string strValue = GetString(strSection, strKey, null);

            try
            {

                if (strValue != null)
                {
                    return Convert.ToDouble(strValue);
                }
            }
            catch // (Exception ex)
            {
                /* throw new IniFileException(m_strFilePath, strSection, strKey, ex,
                    string.Format(@"Expected a double-precision floating-point value; encountered '{0}' instead.", strValue)); */
            }

            return dDefault;
        }

        public double GetDouble(string strSection, string strKey)
        {
            string strValue = GetString(strSection, strKey);

            try
            {
                //return double.Parse(strValue);
                return Convert.ToDouble(strValue);
            }
            catch (Exception ex)
            {
                throw new IniFileException(m_strFilePath, strSection, strKey, ex,
                    string.Format(@"Expected a double-precision floating-point value; encountered '{0}' instead.", strValue));
            }
        }

        public decimal GetDecimal(string strSection, string strKey, decimal decDefault)
        {
            string strValue = GetString(strSection, strKey, null);

            try
            {

                if (strValue != null)
                {
                    return Convert.ToDecimal(strValue);
                }
            }
            catch // (Exception ex)
            {
                /* throw new IniFileException(m_strFilePath, strSection, strKey, ex,
                    string.Format(@"Expected a decimal value; encountered '{0}' instead.", strValue)); */
            }

            return decDefault;
        }

        public decimal GetDecimal(string strSection, string strKey)
        {
            string strValue = GetString(strSection, strKey);

            try
            {
                //return decimal.Parse(strValue);
                return Convert.ToDecimal(strValue);
            }
            catch (Exception ex)
            {
                throw new IniFileException(m_strFilePath, strSection, strKey, ex,
                    string.Format(@"Expected a decimal value; encountered '{0}' instead.", strValue));
            }
        }

        public void SetString(string strSection, string strKey, string strValue)
        {
            StringUtils.ThrowExceptionIfStringArgumentIsDegenerate(@"SolarFlareCommon.IniFile.SetString()", @"strSection", strSection);
            StringUtils.ThrowExceptionIfStringArgumentIsDegenerate(@"SolarFlareCommon.IniFile.SetString()", @"strKey", strKey);

            if (strValue == null)
            {
                throw new ArgumentNullException(@"strValue", @"SolarFlareCommon.IniFile.SetString() : strValue is null");
            }

            Dictionary<string, string> dictSection = GetSection(strSection);

            if (dictSection == null)
            {
                // Since this is a new section, add a new section object to the collection.
                dictSection = new Dictionary<string, string>();
                m_dictSections.Add(strSection, dictSection);
            }

            string strExistingKeyName = StringUtils.FindFirstCaseInsensitiveString(strKey, dictSection.Keys);

            if (strExistingKeyName == null)
            {
                dictSection.Add(strKey, strValue);
            }
            else
            {
                dictSection[strExistingKeyName] = strValue;
            }
        }
    } // public class IniFile
} // namespace SolarFlareCommon

// **** End of File ****
