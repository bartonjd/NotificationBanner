using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace LogonAcceptanceWindow
{
    public class Reg
    {
        static public Int32? GetInt(String Path, String Property)
        {
            Path = FormatPath(Path);
            Int32? value = (Int32?)Registry.GetValue(Path, Property, -1);
            if (value == -1)
            {
                value = null;
            }
            return value;
        }

        static public bool GetBoolFromInt(String Path, String Property)
        {
            Path = FormatPath(Path);
            Int32? value = (Int32?)Registry.GetValue(Path, Property, -1);
            if (value <= 0)
            {
                return false;
            }
            else if (value > 0)
            {
                return true;
            }
            else {
                return false;
            }
        }

        static public Double GetDouble(String Path, String Property)
        {
            Path = FormatPath(Path);
            var value = Registry.GetValue(Path, Property, -1);
            if (null == value)
            {
                value = -1;
            }
            return Double.Parse((String)value);
        }
        static public String[]? GetMultiString(String Path, String Property)
        {
            Path = FormatPath(Path);
            String[]? value = (string[]?)Registry.GetValue(Path, Property, "");
            return value;
        }
        static public String? GetString(String Path, String Property)
        {
            Path = FormatPath(Path);
            var value = Registry.GetValue(Path, Property, "");
            if (value is not null)
            {
                return (string)value;
            }
            else {
                return "";
            }
        }

        static public dynamic? GetValue(String Path, String Property)
        {
            Path = FormatPath(Path);
            var value = Registry.GetValue(Path, Property, "");
            return value;
        }
        static public Boolean PropertyExists(String Path, String Property)
        {
            Path = FormatPath(Path);
            var value = Registry.GetValue(Path, Property, null);

            if (null == value)
            {
                return false;
            }
            return true;
        }
        static public Dictionary<string,dynamic> GetProperties(String Path, String[] properties ) { 
            Dictionary<string,dynamic> results = new ();

            foreach (string property in properties) { 
                var prop = GetValue(Path, property);
                results.Add(property, prop ?? "");
            }

            return results;
        }

        static private String GetHiveName(String Path)
        {
            Path = Path.Replace(":", "");
            //Get the hivename (string before the first \)
            Regex whichHiveRegEx = new(@"(\w+)(?=\\)");
            Match match = whichHiveRegEx.Match(Path);
            String hiveName = "HKEY_LOCAL_MACHINE";
            //TODO: Implement a result class like: https://www.youtube.com/watch?v=a1ye9eGTB98 rather than using a default

            if (match.Success)
            {
                hiveName = match.Groups[1].Value;
                hiveName = hiveName.ToUpper();

                //Take the string and find which registry hive it refers to
                if ((hiveName == "HKEY_LOCAL_MACHINE") || (hiveName == "HKLM"))
                {
                    hiveName = "HKEY_LOCAL_MACHINE";
                }
                if ((hiveName == "HKEY_CURRENT_USER") || (hiveName == "HKCU"))
                {
                    hiveName = "HKEY_CURRENT_USER";
                }
                if ((hiveName == "HKEY_CLASSES_ROOT") || (hiveName == "HKCR"))
                {
                    hiveName = "HKEY_CLASSES_ROOT";
                }
                if ((hiveName == "HKEY_USERS") || (hiveName == "HKU"))
                {
                    hiveName = "HKEY_USERS";
                }
                if ((hiveName == "HKEY_CURRENT_CONFIG") || (hiveName == "HKCC"))
                {
                    hiveName = "HKEY_CURRENT_CONFIG";
                }
                if ((hiveName == "HKEY_PERFORMANCE_DATA") || (hiveName == "HKPD"))
                {
                    hiveName = "HKEY_PERFORMANCE_DATA";
                }
            }
            return hiveName;
        }

        static private RegistryKey GetHive(String Path)
        {
            return GetHiveName(Path) switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                "HKEY_PERFORMANCE_DATA" => Registry.PerformanceData,
                _ => Registry.LocalMachine, //If string doesn't match assume HKLM was intended
            };
        }
        static public Boolean KeyExists(String Path)
        {
            String relativePath = RelativizePath(FormatPath(Path));
            Boolean keyExists = (null != GetHive(Path).OpenSubKey(relativePath));
            return keyExists;
        }
        static private String RelativizePath(String Path)
        {
            Path = Path.Replace(GetHiveName(Path) + @"\", "");
            return Path;
        }
        static private String FormatPath(String Path)
        {
            //Cleanup path if user used colon, e.g. HKLM:\SOFTWARE becomes HKLM\SOFTWARE
            Path = Path.Replace(":", "");
            //Ensure there are no double \ after concatenation or @ escaping
            Path = Path.Replace(@"\\", @"\");
            String hiveName;

            //Get the hivename (string before the first \)
            Regex whichHiveRegEx = new(@"(\w+)(?=\\)");
            Match match = whichHiveRegEx.Match(Path);

            if (match.Success)
            {
                hiveName = match.Groups[1].Value;
                Path = Path.Replace(hiveName, GetHiveName(Path));
            }
            return Path;
        }
        static public void NewKey(String Path, String KeyName)
        {
            string fullPath = ($"{Path}\\{KeyName}").Replace(@"\\", @"\");
            var hive = GetHive(Path);
            if (!KeyExists(fullPath))
            {
                if (KeyName != "")
                {
                    //Ensure keyname is at the end of the path if it was provided
                    Path = RelativizePath(FormatPath(Path));
                }
                var key = hive.OpenSubKey(Path,true);
                if (key is not null)
                {
                    key.CreateSubKey(KeyName);
                }
                else {
                    throw new Exception("No such key");
                }
            }
        }
        /*
        static public void SetProperty(String Path, String Property, dynamic Value)
        {
            //Not implemented yet
        }
   
        static public void DeleteKey(String Path)
        {
            //Not implemented yet
        }
        static public void DeleteProperty()
        {
            //Not implemented yet
        }
        */
    }
}