using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace VRCAntiCrash
{
    public class IniFile
    {
        private string Path;

        private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default,
            StringBuilder RetVal, int Size, string FilePath);

        internal IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? (EXE + ".ini")).FullName;
        }

        internal T Read<T>(string Section, string Key, T DefaultValue = default(T), int Length = 255)
        {
            if (string.IsNullOrEmpty(Key))
            {
                return default(T);
            }

            var stringBuilder = new StringBuilder(Length);

            GetPrivateProfileString(Section ?? EXE, Key, "", stringBuilder, Length, Path);

            var Result = stringBuilder.ToString();

            var Converted = false;

            var ConvertedResult = default(T);

            try
            {
                if (!string.IsNullOrEmpty(Result))
                {
                    ConvertedResult = (T)Convert.ChangeType(Result, typeof(T));

                    Converted = true;
                }
            }
            catch
            {

            }

            return Converted ? ConvertedResult : DefaultValue;
        }

        internal bool Write<T>(string Section, string Key, T Value)
        {
            string ConvertedType = null;

            try
            {
                ConvertedType = (string)Convert.ChangeType(Value, typeof(string));
            }
            catch (Exception ex)
            {
                //("Config", "An Error Occurred While Writing To Config:\n" + ex);
            }

            if (string.IsNullOrEmpty(ConvertedType))
            {
                return false;
            }

            WritePrivateProfileString(Section ?? EXE, Key, ConvertedType, Path);

            return true;
        }

        internal bool DeleteKey(string Section, string Key)
        {
            if (string.IsNullOrEmpty(Key))
            {
                return false;
            }

            return Write(Key, null, Section ?? EXE);
        }

        internal bool DeleteSection(string Section = null)
        {
            return Write(null, null, Section ?? EXE);
        }

        internal bool KeyExists(string Section, string Key)
        {
            return !string.IsNullOrEmpty(Read<string>(Section, Key));
        }
    }
}
