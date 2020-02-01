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
        public void CanGetKeyValuePairs(string value, string expected)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", $"color={value}", "name=sam" };
            File.WriteAllLines(fileName, lines);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get all the key/value pairs from the global section
            var keyValuePairs = iniFile.GetKeyValuePairs("global");

            // Verify that all the pairs were returned
            keyValuePairs.Should()
                .BeEquivalentTo(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("color", expected ?? value),
                    new KeyValuePair<string, string>("name", "sam")
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
        public void CanGetValues(string value, string expected)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();
            string[] lines = { "[global]", $"color={value}" };
            File.WriteAllLines(fileName, lines);

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the value
            string color = iniFile.GetValue("global", "color");

            // Verify that the file was created
            color.Should().Be(expected ?? value);

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
        public void DefaultIsReturnedIfNotFound(string value)
        {
            // Create a simple ini file with one value
            string fileName = Path.GetTempFileName();

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Get the value, specifying a default
            string color = iniFile.GetValue("global", "color", $"{value}");

            // Verify that the value is correct
            color.Should().Be(value.TrimEnd());

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
