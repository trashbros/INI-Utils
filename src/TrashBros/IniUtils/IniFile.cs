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

        #region Internal Classes

        /// <summary>
        /// Class containing all P/Invokes
        /// </summary>
        internal static class NativeMethods
        {
            #region Internal Methods

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

            #endregion Internal Methods
        }

        #endregion Internal Classes

        #region Public Fields

        /// <summary>
        /// The maximum value size in characters that can be read.
        /// </summary>
        public const int MaxValueSize = 1024;

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
        /// Gets the value of the specified key in the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        public string GetValue(string section, string key, string defaultValue = "")
        {
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
        public void SetValue(string section, string key, string value)
        {
            NativeMethods.WritePrivateProfileString(section, key, value, _fileName);
        }

        #endregion Public Methods
    }
}
