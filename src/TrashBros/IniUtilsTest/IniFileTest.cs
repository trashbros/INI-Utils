using Xunit;
using TrashBros.IniUtils;
using System.IO;

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
            Assert.False(File.Exists(fileName));

            // Create a new IniFile with the temp file name
            var iniFile = new IniFile(fileName);

            // Set a value
            iniFile.SetValue("global", "color", "purple");

            // Verify that the file was created
            Assert.True(File.Exists(fileName));

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
            Assert.Equal("purple", color);

            // Clean up after ourselves
            File.Delete(fileName);
        }

    }
}
