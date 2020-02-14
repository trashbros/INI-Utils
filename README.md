# INI Utils
![Build](https://github.com/trashbros/INI-Utils/workflows/Build/badge.svg)

Library for reading and writing INI files.

## Usage
INI files are broken up into discrete sections of settings with values. Each section and setting name are **case sensitive** but accept spaces.

### Initialization
Begin by defining a new INI file
```
IniFile iniFile = new IniFile(filePath);
```

### Read Settings
To read settings you provide the name of the section in the file, and the name of the setting in the section.
```
var setting = iniFile.ReadSetting("global", "color");
```

Or read all settings in a section at once with
```
var settings = iniFile.ReadSettings("global");
```

### Write Settings
To write settings you provide the name of the section, and an instance of the Setting class, which has an identifying name and a setting value.
```
iniFile.WriteSetting("global", new Setting("color", value));
```

You can also write an entire section of settings at once as shown below.
```
// Create a list of settings
var settings = new List<Setting>
{
    new Setting("color", value),
    new Setting("name", "sam")
};

// Write the settings to the global section
iniFile.WriteSettings("global", settings);
```