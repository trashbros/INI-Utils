using System;
using System.Collections.Generic;

namespace TrashBros.IniUtils
{
    public struct Setting : IEquatable<Setting>
    {
        #region Private Fields

        private readonly KeyValuePair<string, string> keyValuePair;

        #endregion Private Fields

        #region Public Constructors

        public Setting(string key, string value)
        {
            keyValuePair = new KeyValuePair<string, string>(key, value);
        }

        #endregion Public Constructors

        #region Public Properties

        public string Key { get => keyValuePair.Key; }
        public string Value { get => keyValuePair.Value; }

        #endregion Public Properties

        #region Public Methods

        public static bool operator !=(Setting left, Setting right)
        {
            return !(left == right);
        }

        public static bool operator ==(Setting left, Setting right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            Setting setting = (Setting)obj;

            if (setting == null)
            {
                return false;
            }
            else
            {
                return Equals(setting);
            }
        }

        public bool Equals(Setting other)
        {
            return keyValuePair.Equals(other.keyValuePair);
        }

        public override int GetHashCode()
        {
            return keyValuePair.GetHashCode();
        }

        public override string ToString()
        {
            return keyValuePair.ToString();
        }

        #endregion Public Methods
    }
}
