using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TrashBros.IniUtils
{
    /// <summary>
    /// Represents an INI file that settings can be written and read from.
    /// </summary>
    public class IniFile
    {
        #region Private Fields

        /// <summary>
        /// The file name of the INI file.
        /// </summary>
        private readonly string _fileName;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Checks for null and throws exceptions accordingly.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <exception cref="ArgumentNullException">If arg is null.</exception>
        private static void CheckForNull(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        #endregion Private Methods

        #region Internal Classes

        /// <summary>
        /// Class containing all P/Invokes
        /// </summary>
        internal static class NativeMethods
        {
            #region Internal Methods

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileSection(string lpAppName, char[] lpReturnedString, int nSize, string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

            #endregion Internal Methods
        }

        #endregion Internal Classes

        #region Public Fields

        /// <summary>
        /// The maximum value size in characters that can be used.
        /// </summary>
        public const int MaxValueSize = 32767;

        #endregion Public Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFile"/> class using the specified INI
        /// file name.
        /// </summary>
        /// <param name="fileName">Name of the INI file.</param>
        public IniFile(string fileName)
        {
            _fileName = fileName;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Gets the key/value pairs in the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>The key value pairs.</returns>
        public List<KeyValuePair<string, string>> GetKeyValuePairs(string section)
        {
            // Initialize list of key/value pairs
            var keyValuePairs = new List<KeyValuePair<string, string>>();

            // Read the key/value pairs from the INI file
            char[] lpReturnedString = new char[MaxValueSize];
            int num = NativeMethods.GetPrivateProfileSection(section, lpReturnedString, MaxValueSize, _fileName);

            // Make sure something was actually found
            if (num < 3) return keyValuePairs;

            // Create an array of strings from the returned characters
            string[] pairStrings = new string(lpReturnedString.Take(num - 1).ToArray()).Split('\0');

            // Parse each pair string into a KeyValuePair and add it to the list
            foreach (string pair in pairStrings)
            {
                // Init key and values to empty strings
                string key = "";
                string value = "";

                // Split the key/value pair
                string[] keyValue = pair.Split('=');

                // Set the key
                if (keyValue.Length > 0)
                {
                    key = keyValue[0];
                }

                // Set the value
                if (keyValue.Length > 1)
                {
                    value = string.Join("=", keyValue.Skip(1).ToArray());

                    if (value.Length > 1 && value[0] == '\'' && value[value.Length - 1] == '\'')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (value.Length > 1 && value[0] == '\"' && value[value.Length - 1] == '\"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                }

                // Add the key/value pair to the list
                keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
            }

            // Return the list of key/value pairs
            return keyValuePairs;
        }

        /// <summary>
        /// Gets the value of the specified key in the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">If key is null.</exception>
        public string GetValue(string section, string key, string defaultValue = "")
        {
            CheckForNull(key, nameof(key));

            StringBuilder sb = new StringBuilder(MaxValueSize);
            _ = NativeMethods.GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, _fileName);

            return sb.ToString();
        }

        /// <summary>
        /// Sets the value of the specified key in the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">If key or value is null.</exception>
        public void SetValue(string section, string key, string value)
        {
            CheckForNull(key, nameof(key));
            CheckForNull(value, nameof(value));

            _ = NativeMethods.WritePrivateProfileString(section, key, value, _fileName);
        }

        #endregion Public Methods
    }
}
