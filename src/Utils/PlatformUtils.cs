using Godot;
using System;

namespace CrazySnooker.Utils
{

    public class PlatformUtils
    {
        public static bool IsOnMobile(string osName)
        {
            if (osName == "Android" || osName == "iOS") {
                return true;
            }

            return false;
        }

        public static bool IsOnDesktop(string osName)
        {
            if (osName == "Windows" || osName == "OSX" || osName == "Linux") {
                return true;
            }

            return false;
        }
    }
}
