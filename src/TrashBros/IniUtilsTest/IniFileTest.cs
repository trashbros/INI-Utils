using Xunit;
using TrashBros.IniUtils;
using System.IO;
using FluentAssertions;

namespace IniUtilsTest
{
    public class IniFileTest
    {
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
    }
}
