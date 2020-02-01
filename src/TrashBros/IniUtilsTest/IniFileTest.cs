using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using TrashBros.IniUtils;
using Xunit;

namespace IniUtilsTest
{
    public class IniFileTest
    {
        #region Public Methods

        [Fact]
        public void CanGetAValue()
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", "color=purple" };
            File.WriteAllLines(fileName, lines);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the value
            string color = iniFile.GetValue("global", "color");

            // Verify that the file was created
            color.Should().Be("purple");

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void CanGetKeyValuePairs()
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", "color=purple", "name=sam" };
            File.WriteAllLines(fileName, lines);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get all the key/value pairs from the global section
            var keyValuePairs = iniFile.GetKeyValuePairs("global");

            // Verify that all the pairs were returned
            keyValuePairs.Should()
                .BeEquivalentTo(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("color", "purple"),
                    new KeyValuePair<string, string>("name", "sam")
                });

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void CanSetAValue()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();
            File.Delete(fileName);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Set a value
            iniFile.SetValue("global", "color", "purple");

            // Verify that the file matches what is expected
            File.ReadAllLines(fileName).Should().BeEquivalentTo(new string[] { "[global]", "color=purple" });

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void DefaultIsReturnedIfNotFound()
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the value, specifying a default
            string color = iniFile.GetValue("global", "color", "purple");

            // Verify that the value is correct
            color.Should().Be("purple");

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void GetWithNullKeyThrowsException()
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", "color=purple" };
            File.WriteAllLines(fileName, lines);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the value
            Action action = () => { string color = iniFile.GetValue("global", null); };

            // Verify that the exception is thrown
            action.Should().ThrowExactly<ArgumentNullException>();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void SettingAValueCreatesANewFile()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();
            File.Delete(fileName);

            // Verify that the file isn't there
            File.Exists(fileName).Should().BeFalse();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Set a value
            iniFile.SetValue("global", "color", "purple");

            // Verify that the file was created
            File.Exists(fileName).Should().BeTrue();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void SetWithNullKeyThrowsException()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Set a value
            Action action = () => { iniFile.SetValue("global", null, "purple"); };

            // Verify that exception is thrown
            action.Should().ThrowExactly<ArgumentNullException>();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        [Fact]
        public void SetWithNullValueThrowsException()
        {
            // Create a new temporary file to get a temp file name and delete it
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Set a value
            Action action = () => { iniFile.SetValue("global", "color", null); };

            // Verify that exception is thrown
            action.Should().ThrowExactly<ArgumentNullException>();

            // Clean up after ourselves
            File.Delete(fileName);
        }

        #endregion Public Methods
    }
}
