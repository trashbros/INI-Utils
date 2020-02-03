/*
IniFileTest.cs

Unit tests for IniFile class.

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

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrashBros.IniUtils;
using Xunit;

namespace IniUtilsTest
{
    public class IniFileTest
    {
        #region Public Methods

        [Theory]
        [InlineData("purple")]
        [InlineData("   purple")]
        [InlineData("purple   ")]
        [InlineData("a")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("This is a value with spaces!")]
        [InlineData("This is a value with [square brackets]")]
        [InlineData("This is a value with ; semi-colors ;")]
        [InlineData("This is a value with = equal signs =")]
        [InlineData("\"Double quotes\"")]
        [InlineData("\'Single quotes\'")]
        [InlineData("\"  Double quotes  \"")]
        [InlineData("\'  Single quotes  \'")]
        [InlineData("\"Unmatched double")]
        [InlineData("\'Unmatched single")]
        [InlineData("Unmatched double\"")]
        [InlineData("Unmatched single\'")]
        [InlineData("\"\"")]
        [InlineData("\'\'")]
        [InlineData("\"")]
        [InlineData("\'")]
        [InlineData("\"Mixed quotes\'")]
        [InlineData("\'Mixed quotes\"")]
        [InlineData("αβγδε")]
        public void CanDeleteASetting(string value)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", $"color={value}", "name=sam" };
            File.WriteAllLines(fileName, lines, Encoding.Unicode);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Delete one of the settings
            iniFile.DeleteSetting("global", "color");

            // Get all settings from the global section
            var settings = iniFile.ReadSettings("global");

            // Verify that all the settings were returned
            settings.Should()
                .BeEquivalentTo(new List<Setting>()
                {
                    new Setting("name", "sam")
                });

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Theory]
        [InlineData("purple", null)]
        [InlineData("   purple", "purple")]
        [InlineData("purple   ", "purple")]
        [InlineData("a", null)]
        [InlineData("", null)]
        [InlineData("  ", "")]
        [InlineData("This is a value with spaces!", null)]
        [InlineData("This is a value with [square brackets]", null)]
        [InlineData("This is a value with ; semi-colors ;", null)]
        [InlineData("This is a value with = equal signs =", null)]
        [InlineData("\"Double quotes\"", "Double quotes")]
        [InlineData("\'Single quotes\'", "Single quotes")]
        [InlineData("\"  Double quotes  \"", "  Double quotes  ")]
        [InlineData("\'  Single quotes  \'", "  Single quotes  ")]
        [InlineData("\"Unmatched double", null)]
        [InlineData("\'Unmatched single", null)]
        [InlineData("Unmatched double\"", null)]
        [InlineData("Unmatched single\'", null)]
        [InlineData("\"\"", "")]
        [InlineData("\'\'", "")]
        [InlineData("\"", null)]
        [InlineData("\'", null)]
        [InlineData("\"Mixed quotes\'", null)]
        [InlineData("\'Mixed quotes\"", null)]
        [InlineData("αβγδε", null)]
        public void CanReadASetting(string value, string expected)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", $"color={value}" };
            File.WriteAllLines(fileName, lines, Encoding.Unicode);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the setting
            var setting = iniFile.ReadSetting("global", "color");

            // Verify that the setting was read correctly
            setting.Name.Should().Be("color");
            setting.Value.Should().Be(expected ?? value);

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Theory]
        [InlineData("purple", null)]
        [InlineData("   purple", "purple")]
        [InlineData("purple   ", "purple")]
        [InlineData("a", null)]
        [InlineData("", null)]
        [InlineData("  ", "")]
        [InlineData("This is a value with spaces!", null)]
        [InlineData("This is a value with [square brackets]", null)]
        [InlineData("This is a value with ; semi-colors ;", null)]
        [InlineData("This is a value with = equal signs =", null)]
        [InlineData("\"Double quotes\"", "Double quotes")]
        [InlineData("\'Single quotes\'", "Single quotes")]
        [InlineData("\"  Double quotes  \"", "  Double quotes  ")]
        [InlineData("\'  Single quotes  \'", "  Single quotes  ")]
        [InlineData("\"Unmatched double", null)]
        [InlineData("\'Unmatched single", null)]
        [InlineData("Unmatched double\"", null)]
        [InlineData("Unmatched single\'", null)]
        [InlineData("\"\"", "")]
        [InlineData("\'\'", "")]
        [InlineData("\"", null)]
        [InlineData("\'", null)]
        [InlineData("\"Mixed quotes\'", null)]
        [InlineData("\'Mixed quotes\"", null)]
        [InlineData("αβγδε", null)]
        public void CanReadSettingsFromSection(string value, string expected)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", $"color={value}", "name=sam" };
            File.WriteAllLines(fileName, lines, Encoding.Unicode);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get all settings from the global section
            var settings = iniFile.ReadSettings("global");

            // Verify that all the settings were returned
            settings.Should()
                .BeEquivalentTo(new List<Setting>()
                {
                    new Setting("color", expected ?? value),
                    new Setting("name", "sam")
                });

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Theory]
        [InlineData("purple")]
        [InlineData("   purple")]
        [InlineData("purple   ")]
        [InlineData("a")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("This is a value with spaces!")]
        [InlineData("This is a value with [square brackets]")]
        [InlineData("This is a value with ; semi-colors ;")]
        [InlineData("This is a value with = equal signs =")]
        [InlineData("\"Double quotes\"")]
        [InlineData("\'Single quotes\'")]
        [InlineData("\"  Double quotes  \"")]
        [InlineData("\'  Single quotes  \'")]
        [InlineData("\"Unmatched double")]
        [InlineData("\'Unmatched single")]
        [InlineData("Unmatched double\"")]
        [InlineData("Unmatched single\'")]
        [InlineData("\"\"")]
        [InlineData("\'\'")]
        [InlineData("\"")]
        [InlineData("\'")]
        [InlineData("\"Mixed quotes\'")]
        [InlineData("\'Mixed quotes\"")]
        [InlineData("αβγδε")]
        public void CanWriteASetting(string value)
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();
            File.Delete(fileName);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Write the setting
            iniFile.WriteSetting("global", new Setting("color", value));

            // Verify that the file contains the setting we just wrote
            File.ReadAllLines(fileName).Should().Contain(new string[] { "[global]", $"color={value}" });

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Theory]
        [InlineData("purple")]
        [InlineData("   purple")]
        [InlineData("purple   ")]
        [InlineData("a")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("This is a value with spaces!")]
        [InlineData("This is a value with [square brackets]")]
        [InlineData("This is a value with ; semi-colors ;")]
        [InlineData("This is a value with = equal signs =")]
        [InlineData("\"Double quotes\"")]
        [InlineData("\'Single quotes\'")]
        [InlineData("\"  Double quotes  \"")]
        [InlineData("\'  Single quotes  \'")]
        [InlineData("\"Unmatched double")]
        [InlineData("\'Unmatched single")]
        [InlineData("Unmatched double\"")]
        [InlineData("Unmatched single\'")]
        [InlineData("\"\"")]
        [InlineData("\'\'")]
        [InlineData("\"")]
        [InlineData("\'")]
        [InlineData("\"Mixed quotes\'")]
        [InlineData("\'Mixed quotes\"")]
        [InlineData("αβγδε")]
        public void CanWriteSettingsToASection(string value)
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();
            File.Delete(fileName);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Write a setting to the section
            iniFile.WriteSetting("global", new Setting("audio", "off"));

            // Create a list of settings
            var settings = new List<Setting>
            {
                new Setting("color", value),
                new Setting("name", "sam")
            };

            // Write the settings to the global section
            iniFile.WriteSettings("global", settings);

            // Verify that the file contains the settings
            File.ReadAllLines(fileName).Should().Contain(new string[] { "[global]", $"color={value}", "name=sam", "audio=off" });

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Theory]
        [InlineData("purple")]
        [InlineData("   purple")]
        [InlineData("purple   ")]
        [InlineData("a")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("This is a value with spaces!")]
        [InlineData("This is a value with [square brackets]")]
        [InlineData("This is a value with ; semi-colors ;")]
        [InlineData("This is a value with = equal signs =")]
        [InlineData("\"Double quotes\"")]
        [InlineData("\'Single quotes\'")]
        [InlineData("\"  Double quotes  \"")]
        [InlineData("\'  Single quotes  \'")]
        [InlineData("\"Unmatched double")]
        [InlineData("\'Unmatched single")]
        [InlineData("Unmatched double\"")]
        [InlineData("Unmatched single\'")]
        [InlineData("\"\"")]
        [InlineData("\'\'")]
        [InlineData("\"")]
        [InlineData("\'")]
        [InlineData("\"Mixed quotes\'")]
        [InlineData("\'Mixed quotes\"")]
        [InlineData("αβγδε")]
        public void DefaultIsReturnedIfNotFound(string value)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the setting, specifying a default
            var setting = iniFile.ReadSetting("global", "color", $"{value}");

            // Verify that the value is correct
            setting.Value.Should().Be(value.TrimEnd());

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void ReadWithNullNameThrowsException()
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", "color=purple" };
            File.WriteAllLines(fileName, lines, Encoding.Unicode);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Create an action that reads a setting with a null name
            Action action = () => { var setting = iniFile.ReadSetting("global", null); };

            // Verify that the exception is thrown
            action.Should().ThrowExactly<ArgumentNullException>();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void WriteWithNullNameThrowsException()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Create an action that writes a setting with a null name
            Action action = () => { iniFile.WriteSetting("global", new Setting(null, "purple")); };

            // Verify that exception is thrown
            action.Should().ThrowExactly<ArgumentNullException>();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void WriteWithNullValueThrowsException()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Create an action that writes a setting with a null value
            Action action = () => { iniFile.WriteSetting("global", new Setting("color", null)); };

            // Verify that exception is thrown
            action.Should().ThrowExactly<ArgumentNullException>();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void WritingASettingCreatesANewFile()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();
            File.Delete(fileName);

            // Verify that the file isn't there
            File.Exists(fileName).Should().BeFalse();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Write a setting
            iniFile.WriteSetting("global", new Setting("color", "purple"));

            // Verify that the file was created
            File.Exists(fileName).Should().BeTrue();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        #endregion Public Methods
    }
}
