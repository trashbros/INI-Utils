/*
IniFile.cs

Provies a class to read and write settigns to INI files.

Copyright (C) 2020 Trash Bros (BlinkTheThings, Reakain)

This file is part of TransBros.IniUtils.

TransBros.IniUtils is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

TransBros.IniUtils is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with TransBros.IniUtils.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TrashBros.IniUtils
{
    /// <summary>
    /// Read and write settigns to an INI file.
    /// </summary>
    public class IniFile
    {
        #region Private Fields

        /// <summary>
        /// A regex that should match a any setting
        /// </summary>
        private static readonly Regex anyNameValueRegex = new Regex($@"^\s*(.*?\S)\s*=\s*(.*)$");

        /// <summary>
        /// A regex that should match any section
        /// </summary>
        private static readonly Regex anySectionRegex = new Regex(@"^\s*\[\s*(.*)\s*\].*$");

        /// <summary>
        /// INI file name.
        /// </summary>
        private readonly string _fileName;

        #endregion Private Fields

        #region Private Enums

        private enum DeleteSettingState
        {
            LookingForSection,
            LookingForSetting,
            DoneLooking
        }

        private enum ReadSettingState
        {
            LookingForSection,
            LookingForSetting,
            DoneLooking,
        }

        private enum WriteSettingState
        {
            LookingForSection,
            LookingForSetting,
            DoneLooking
        }

        #endregion Private Enums

        #region Private Methods

        /// <summary>
        /// Thrown an exception if <paramref name="arg"/> is null.
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
            // Make sure the section and name aren't null
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(name, nameof(name));

            // A regex that will match the specified section
            Regex specificSectionRegex = new Regex($@"^\s*\[\s*({section})\s*\].*$");

            // A regex that will match a setting with a specific name
            Regex specificNameValueRegex = new Regex($@"^\s*({name})\s*=.*$");

            // First we look for the section
            DeleteSettingState deleteSettingState = DeleteSettingState.LookingForSection;

            // Create a memory stream to store the new file
            var ms = new MemoryStream();

            using (var reader = new StreamReader(_fileName, Encoding.Unicode))
            using (var writer = new StreamWriter(ms, Encoding.Unicode))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    switch (deleteSettingState)
                    {
                        // We are looking for the specific section
                        case DeleteSettingState.LookingForSection:

                            // Is this the right section?
                            if (specificSectionRegex.IsMatch(line))
                            {
                                // Now look for the setting in this section
                                deleteSettingState = DeleteSettingState.LookingForSetting;
                            }
                            writer.WriteLine(line);
                            break;

                        // We are looking for the specific setting to delete
                        case DeleteSettingState.LookingForSetting:
                            // Is this the right setting?
                            if (specificNameValueRegex.IsMatch(line))
                            {
                                // We are now done looking, also we won't write out this line
                                deleteSettingState = DeleteSettingState.DoneLooking;
                            }
                            else
                            {
                                // Not the right setting, keep on going
                                writer.WriteLine(line);
                            }
                            break;

                        // We done looking for the setting, just write out the rest of the lines
                        case DeleteSettingState.DoneLooking:
                            writer.WriteLine(line);
                            break;
                    }
                }
            }

            // Replace the file
            File.WriteAllBytes(_fileName, ms.ToArray());
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
            // Make sure the seciton and name aren't null
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(name, nameof(name));

            // Initialize the value to the default in case we don't find it
            string value = defaultValue?.TrimEnd() ?? "";

            // A regex that will match the specified section
            Regex specificSectionRegex = new Regex($@"^\s*\[\s*({section})\s*\].*$");

            // A regex that will match a setting with a specific name
            Regex specificNameValueRegex = new Regex($@"^\s*({name})\s*=\s*(.*\S)\s*$");

            // First we look for the section
            ReadSettingState readSettingState = ReadSettingState.LookingForSection;

            // Read all the file lines
            string[] lines = File.ReadAllLines(_fileName, Encoding.Unicode);

            // Check the lines one at a time and try and find the setting in specified section
            foreach (string line in lines)
            {
                switch (readSettingState)
                {
                    // We are looking for the specific section
                    case ReadSettingState.LookingForSection:

                        // Is this the right section?
                        if (specificSectionRegex.IsMatch(line))
                        {
                            // Now look for the setting in this section
                            readSettingState = ReadSettingState.LookingForSetting;
                        }
                        break;

                    // We are looking for the specific setting
                    case ReadSettingState.LookingForSetting:

                        // Is this the start of a new section?
                        if (anySectionRegex.IsMatch(line))
                        {
                            // We have encountered a new section without finding the setting We can
                            // stop looking now
                            readSettingState = ReadSettingState.DoneLooking;
                        }
                        // Is this the setting we are looking for?
                        else if (specificNameValueRegex.IsMatch(line))
                        {
                            // Grab the value using the regex
                            var matches = specificNameValueRegex.Matches(line);
                            value = matches[0].Groups[2].Value.TrimEnd();

                            // Check for outer single quotes
                            if (value.Length > 1 && value[0] == '\'' && value[value.Length - 1] == '\'')
                            {
                                // Remove outer single quotes
                                value = value.Substring(1, value.Length - 2);
                            }
                            // Check for outer double quotes
                            else if (value.Length > 1 && value[0] == '\"' && value[value.Length - 1] == '\"')
                            {
                                // Remove outer double quotes
                                value = value.Substring(1, value.Length - 2);
                            }

                            // We found the setting, we can stop looking now
                            readSettingState = ReadSettingState.DoneLooking;
                        }
                        break;
                }

                // We can stop looping through the lines if we are done.
                if (readSettingState == ReadSettingState.DoneLooking)
                {
                    break;
                }
            }

            // Return the setting
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

            // A regex that will match the specified section
            Regex specificSectionRegex = new Regex($@"^\s*\[\s*({section})\s*\].*$");

            // First we look for the section
            ReadSettingState readSettingState = ReadSettingState.LookingForSection;

            // Read all the file lines
            string[] lines = File.ReadAllLines(_fileName, Encoding.Unicode);

            // Check the lines one at a time and try and find the setting in specified section
            foreach (string line in lines)
            {
                switch (readSettingState)
                {
                    // We are looking for the specific section
                    case ReadSettingState.LookingForSection:

                        // Is this the right section?
                        if (specificSectionRegex.IsMatch(line))
                        {
                            // Now look for the settings in this section
                            readSettingState = ReadSettingState.LookingForSetting;
                        }
                        break;

                    // We are looking for settings in this seciton now
                    case ReadSettingState.LookingForSetting:

                        // Is this the start of a new section?
                        if (anySectionRegex.IsMatch(line))
                        {
                            // We have encountered a new section without finding the setting We can
                            // stop looking now
                            readSettingState = ReadSettingState.DoneLooking;
                        }
                        // Is this a setting?
                        else if (anyNameValueRegex.IsMatch(line))
                        {
                            // Grab the name and value using the regex
                            var matches = anyNameValueRegex.Matches(line);
                            string name = matches[0].Groups[1].Value.TrimEnd();
                            string value = matches[0].Groups[2].Value.TrimEnd();

                            // Check for outer single quotes
                            if (value.Length > 1 && value[0] == '\'' && value[value.Length - 1] == '\'')
                            {
                                // Remove outer single quotes
                                value = value.Substring(1, value.Length - 2);
                            }
                            // Check for outer double quotes
                            else if (value.Length > 1 && value[0] == '\"' && value[value.Length - 1] == '\"')
                            {
                                // Remove outer double quotes
                                value = value.Substring(1, value.Length - 2);
                            }

                            // Add the setting to the list
                            settings.Add(new Setting(name, value));
                        }
                        break;
                }

                // We can stop looping through the lines if we are done.
                if (readSettingState == ReadSettingState.DoneLooking)
                {
                    break;
                }
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
            // Make sure section and settting isn't null
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(setting.Name, nameof(setting.Name));
            ThrowExceptionIfNull(setting.Value, nameof(setting.Value));

            // A regex that will match the specified section
            Regex specificSectionRegex = new Regex($@"^\s*\[\s*({section})\s*\].*$");

            // A regex that will match a setting with a specific name
            Regex specificNameValueRegex = new Regex($@"^\s*({setting.Name})\s*=.*$");

            // First we look for the section
            WriteSettingState writeSettingState = WriteSettingState.LookingForSection;

            // Create a memory stream to store the new file
            var ms = new MemoryStream();

            using (var reader = new StreamReader(_fileName, Encoding.Unicode))
            using (var writer = new StreamWriter(ms, Encoding.Unicode))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    switch (writeSettingState)
                    {
                        // We are looking for the specific section
                        case WriteSettingState.LookingForSection:

                            // Is this the right section?
                            if (specificSectionRegex.IsMatch(line))
                            {
                                // Now look for the setting in this section
                                writeSettingState = WriteSettingState.LookingForSetting;
                            }
                            writer.WriteLine(line);
                            break;

                        // We are looking for the specific setting to update it
                        case WriteSettingState.LookingForSetting:
                            // Is this the start of a new section?
                            if (anySectionRegex.IsMatch(line))
                            {
                                // We have encountered a new section without finding the setting We
                                // can stop looking now

                                // Write the setting followed by a new line
                                writer.WriteLine($"{setting.Name}={setting.Value}");
                                writer.WriteLine("");

                                writer.WriteLine(line);
                                writeSettingState = WriteSettingState.DoneLooking;
                            }
                            // Is this the right setting?
                            else if (specificNameValueRegex.IsMatch(line))
                            {
                                // Write the new setting
                                writer.WriteLine($"{setting.Name}={setting.Value}");

                                // We are now done looking
                                writeSettingState = WriteSettingState.DoneLooking;
                            }
                            else
                            {
                                // Not the right setting, keep on going
                                writer.WriteLine(line);
                            }
                            break;

                        // We done looking for the setting, just write out the rest of the lines
                        case WriteSettingState.DoneLooking:
                            writer.WriteLine(line);
                            break;
                    }
                }

                // If we didn't find the section
                if (writeSettingState == WriteSettingState.LookingForSection)
                {
                    // Write the seciton
                    writer.WriteLine("");
                    writer.WriteLine($"[{section.Trim()}]");

                    // Write the setting followed by a new line
                    writer.WriteLine($"{setting.Name.Trim()}={setting.Value}");
                    writer.WriteLine("");
                }
            }

            // Replace the file
            File.WriteAllBytes(_fileName, ms.ToArray());
        }

        /// <summary>
        /// Writes a list of settings to the specified section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="settings">The settings.</param>
        public void WriteSettings(string section, List<Setting> settings)
        {
            // Make sure section and setttings isn't null
            ThrowExceptionIfNull(section, nameof(section));
            ThrowExceptionIfNull(settings, nameof(settings));

            // A regex that will match the specified section
            Regex specificSectionRegex = new Regex($@"^\s*\[\s*({section})\s*\].*$");

            // First we look for the section
            WriteSettingState writeSettingState = WriteSettingState.LookingForSection;

            // Create a copy of the settings
            var remainingSettings = new List<Setting>(settings.ToArray());

            // Create memory stream to store the new file
            var ms = new MemoryStream();

            using (var reader = new StreamReader(_fileName, Encoding.Unicode))
            using (var writer = new StreamWriter(ms, Encoding.Unicode))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    switch (writeSettingState)
                    {
                        // We are looking for the specific section
                        case WriteSettingState.LookingForSection:

                            // Is this the right section?
                            if (specificSectionRegex.IsMatch(line))
                            {
                                // Now look for the setting in this section
                                writeSettingState = WriteSettingState.LookingForSetting;
                            }
                            writer.WriteLine(line);
                            break;

                        // We are looking for the specific setting to update it
                        case WriteSettingState.LookingForSetting:
                            // Is this the start of a new section?
                            if (anySectionRegex.IsMatch(line))
                            {
                                // We have encountered a new section without finding the setting We
                                // can stop looking now

                                // Write the remaining settings followed by a new line
                                foreach (var setting in remainingSettings)
                                {
                                    writer.WriteLine($"{setting.Name}={setting.Value}");
                                }
                                writer.WriteLine("");

                                writer.WriteLine(line);
                                writeSettingState = WriteSettingState.DoneLooking;
                            }
                            // Is this a setting?
                            else if (anyNameValueRegex.IsMatch(line))
                            {
                                // Grab the name and value using the regex
                                var matches = anyNameValueRegex.Matches(line);
                                string name = matches[0].Groups[1].Value.TrimEnd();

                                // Do any of the remaining settings match?
                                if (remainingSettings.Any(s => s.Name == name))
                                {
                                    // Update the setting
                                    Setting setting = remainingSettings.Last(s => s.Name == name);
                                    writer.WriteLine($"{setting.Name}={setting.Value}");

                                    // Remove it from the list
                                    remainingSettings.RemoveAll(s => s.Name == name);
                                }
                                // No match, so just keep the setting and keep going
                                else
                                {
                                    writer.WriteLine(line);
                                }
                            }
                            else
                            {
                                // Not a setting, keep on going
                                writer.WriteLine(line);
                            }
                            break;

                        // We done looking for the setting, just write out the rest of the lines
                        case WriteSettingState.DoneLooking:
                            writer.WriteLine(line);
                            break;
                    }
                }

                // If we didn't find the section
                if (writeSettingState == WriteSettingState.LookingForSection)
                {
                    // Write the seciton
                    writer.WriteLine("");
                    writer.WriteLine($"[{section.Trim()}]");

                    // Write the remaining settings followed by a new line
                    foreach (var setting in remainingSettings)
                    {
                        writer.WriteLine($"{setting.Name}={setting.Value}");
                    }
                    writer.WriteLine("");
                }
                // Else if we found the section
                else if (writeSettingState == WriteSettingState.LookingForSetting)
                {
                    // Write the remaining settings followed by a new line
                    foreach (var setting in remainingSettings)
                    {
                        writer.WriteLine($"{setting.Name}={setting.Value}");
                    }
                    writer.WriteLine("");
                }
            }

            // Replace the file
            File.WriteAllBytes(_fileName, ms.ToArray());
        }

        #endregion Public Methods
    }
}
