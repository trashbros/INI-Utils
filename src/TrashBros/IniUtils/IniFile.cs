using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TrashBros.IniUtils
{
    /// <summary>
    /// Represents an INI file that settings can be written to and read from.
    /// </summary>
    public class IniFile
    {
        #region Private Fields

        /// <summary>
        /// The maximum size in bytes that can be read or written.
        /// </summary>
        private const int MaxSize = 65536;

        /// <summary>
        /// The file name of the INI file.
        /// </summary>
        private readonly string _fileName;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Checks for null and throws exception accordingly.
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
            internal static extern int GetPrivateProfileSection(
                string lpAppName,
                byte[] lpReturnedString,
                int nSize,
                string lpFileName
            );

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(
                string lpAppName,
                string lpKeyName,
                string lpDefault,
                byte[] lpReturnedString,
                int nSize,
                string lpFileName
            );

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool WritePrivateProfileString(
                string lpAppName,
                string lpKeyName,
                string lpString,
                string lpFileName
            );

            #endregion Internal Methods
        }

        #endregion Internal Classes

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
        /// Read the setting with the specified key in the specified section. If the setting does
        /// not exist, <paramref name="defaultValue"/> will be used.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">If section or key is null.</exception>
        public KeyValuePair<string, string> ReadSetting(string section, string key, string defaultValue = "")
        {
            CheckForNull(section, nameof(section));
            CheckForNull(key, nameof(key));

            byte[] lpReturnedString = new byte[MaxSize];
            _ = NativeMethods.GetPrivateProfileString(section, key, defaultValue, lpReturnedString, MaxSize, _fileName);

            string value = Encoding.Unicode.GetString(lpReturnedString).TrimEnd('\0');

            return new KeyValuePair<string, string>(key, value);
        }

        /// <summary>
        /// Read all the settings from the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>The key value pairs.</returns>
        public List<KeyValuePair<string, string>> ReadSettingsFromSection(string section)
        {
            // Initialize list of settings
            var settings = new List<KeyValuePair<string, string>>();

            // Read all the settings from the section into a byte array
            byte[] lpReturnedString = new byte[MaxSize];
            int num = NativeMethods.GetPrivateProfileSection(section, lpReturnedString, MaxSize, _fileName);

            string settingsString = Encoding.Unicode.GetString(lpReturnedString);

            // Make sure something was actually found
            if (num < 3) return settings;

            // Create an array of strings from the returned characters
            string[] pairStrings = new string(settingsString.Take(num - 1).ToArray()).Split('\0');

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
                settings.Add(new KeyValuePair<string, string>(key, value));
            }

            // Return the list of key/value pairs
            return settings;
        }

        /// <summary>
        /// Write the setting to the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <exception cref="ArgumentNullException">If key or value is null.</exception>
        public void WriteSetting(string section, KeyValuePair<string, string> setting)
        {
            CheckForNull(section, nameof(section));
            CheckForNull(setting.Key, nameof(setting.Key));
            CheckForNull(setting.Value, nameof(setting.Value));

            _ = NativeMethods.WritePrivateProfileString(section, setting.Key, setting.Value, _fileName);
        }

        #endregion Public Methods
    }
}
