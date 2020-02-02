using System;
using System.Collections.Generic;

namespace TrashBros.IniUtils
{
    /// <summary>A name/value pair</summary>
    /// <seealso cref="System.IEquatable{TrashBros.IniUtils.Setting}" />
    public struct Setting : IEquatable<Setting>
    {
        #region Private Fields

        /// <summary>The key value pair used to hold the setting.</summary>
        private readonly KeyValuePair<string, string> keyValuePair;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>Initializes a new instance of the <see cref="Setting"/> struct.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public Setting(string name, string value)
        {
            keyValuePair = new KeyValuePair<string, string>(name, value);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>Gets the name.</summary>
        /// <value>The name.</value>
        public string Name { get => keyValuePair.Key; }
        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        public string Value { get => keyValuePair.Value; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>Implements the operator !=.</summary>
        /// <param name="left">The left setting.</param>
        /// <param name="right">The right setting.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Setting left, Setting right)
        {
            return !(left == right);
        }

        /// <summary>Implements the operator ==.</summary>
        /// <param name="left">The left setting.</param>
        /// <param name="right">The right setting.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Setting left, Setting right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether the specified <see cref="System.Object"/>, is equal to this instance.</summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
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

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Setting other)
        {
            return keyValuePair.Equals(other.keyValuePair);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return keyValuePair.GetHashCode();
        }

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return keyValuePair.ToString();
        }

        #endregion Public Methods
    }
}
