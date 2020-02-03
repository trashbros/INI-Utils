using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TrashBros.IniUtils
{
    /// <summary>
    /// Read and write settigns to an INI file.
    /// </summary>
    public class IniFile
    {
        #region Private Fields

        /// <summary>
        /// The maximum size in bytes that can be read or written.
        /// </summary>
        private const int MaxSize = 65536;

        /// <summary>
        /// INI file name.
        /// </summary>
        private readonly string _fileName;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Thrown an exception of <paramref name="arg"/> is null.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="arg"/> is null.</exception>
        private static void ThrowExceptionIfNull(object arg, string argName)
        {
            _ = arg ?? throw new ArgumentNullException(argName);
        }

        /// <summary>
        /// Converts the file to Unicode encoding if it isn't already.
        /// </summary>
        private void ConvertFileToUnicodeEncoding()
        {
            if (File.Exists(_fileName))
            {
                string tempFile = null;
                try
                {
                    using (var reader = new StreamReader(_fileName, true))
                    {
                        reader.Peek();
                        var encoding = reader.CurrentEncoding;
                        if (encoding != Encoding.Unicode)
                        {
                            tempFile = Path.GetTempFileName();

                            using (var writer = new StreamWriter(tempFile, false, Encoding.Unicode))
                            {
                                int charsRead;
                                char[] buffer = new char[1024];
                                while ((charsRead = reader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                                {
                                    writer.Write(buffer, 0, charsRead);
                                }
                            }
                        }
                    }
                    if (tempFile != null)
                    {
                        File.Delete(_fileName);
                        File.Move(tempFile, _fileName);
                    }
                }
                finally
                {
                    if (tempFile != null)
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            else
            {
                File.WriteAllBytes(_fileName, Encoding.Unicode.GetPreamble());
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
            internal static extern bool WritePrivateProfileSection(
                string lpAppName,
                string lpString,
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
        /// Initializes a new instance of the <see cref="IniFile"/> class using the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the INI file.</param>
        public IniFile(string fileName)
        {
            _fileName = fileName;

            ConvertFileToUnicodeEncoding();
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Delete a setting from the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="name">The setting name.</param>
        /// <exception cref="ArgumentNullException">If section or name is null.</exception>
        public void DeleteSetting(string section, string name)
        {
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(name, nameof(name));

            _ = NativeMethods.WritePrivateProfileString(section, name, null, _fileName);
        }

        /// <summary>
        /// Read the setting with the specified name in the specified section. If the setting does
        /// not exist, <paramref name="defaultValue"/> will be returned.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// A setting with the value read from the file, or <paramref name="defaultValue"/> if the
        /// setting is not found in the file.
        /// </returns>
        /// <exception cref="ArgumentNullException">If section or name is null.</exception>
        public Setting ReadSetting(string section, string name, string defaultValue = "")
        {
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(name, nameof(name));

            byte[] lpReturnedString = new byte[MaxSize];
            _ = NativeMethods.GetPrivateProfileString(section, name, defaultValue, lpReturnedString, MaxSize, _fileName);

            string value = Encoding.Unicode.GetString(lpReturnedString).TrimEnd('\0');

            return new Setting(name, value);
        }

        /// <summary>
        /// Read all the settings from the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>A list of settings read from <paramref name="section"/>.</returns>
        public List<Setting> ReadSettings(string section)
        {
            // Initialize list of settings
            var settings = new List<Setting>();

            // Read all the settings from the section into a byte array
            byte[] lpReturnedString = new byte[MaxSize];
            int num = NativeMethods.GetPrivateProfileSection(section, lpReturnedString, MaxSize, _fileName);

            string settingsString = Encoding.Unicode.GetString(lpReturnedString);

            // Make sure something was actually found
            if (num < 3) return settings;

            // Create an array of strings from the returned characters
            string[] pairStrings = new string(settingsString.Take(num - 1).ToArray()).Split('\0');

            // Parse each name/value pair string into a setting and add it to the list
            foreach (string pair in pairStrings)
            {
                // Init name and values to empty strings
                string name = "";
                string value = "";

                // Split the name/value pair
                string[] nameValue = pair.Split('=');

                // Set the name
                if (nameValue.Length > 0)
                {
                    name = nameValue[0];
                }

                // Set the value
                if (nameValue.Length > 1)
                {
                    value = string.Join("=", nameValue.Skip(1).ToArray());

                    if (value.Length > 1 && value[0] == '\'' && value[value.Length - 1] == '\'')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (value.Length > 1 && value[0] == '\"' && value[value.Length - 1] == '\"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                }

                // Add the name/value pair to the list
                settings.Add(new Setting(name, value));
            }

            // Return the list of name/value pairs
            return settings;
        }

        /// <summary>
        /// Write a setting to the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <exception cref="ArgumentNullException">
        /// If section, seting, setting.Name, or setting.Value is null.
        /// </exception>
        public void WriteSetting(string section, Setting setting)
        {
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(setting.Name, nameof(setting.Name));
            ThrowExceptionIfNull(setting.Value, nameof(setting.Value));

            _ = NativeMethods.WritePrivateProfileString(section, setting.Name, setting.Value, _fileName);
        }

        /// <summary>
        /// Writes a list of settings to the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="settings">The settings.</param>
        public void WriteSettings(string section, List<Setting> settings)
        {
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(settings, nameof(settings));

            foreach (var setting in settings)
            {
                WriteSetting(section, setting);
            }
        }

        #endregion Public Methods
    }
}
