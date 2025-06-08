using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downcast
{
    internal class DowncastEnums
    {
        public static void RegisterAll()
        {
            PlayerBodyModeIndex.RegisterValues();
        }

        public static void UnregisterAll()
        {
            PlayerBodyModeIndex.UnregisterValues();
        }

        public class PlayerBodyModeIndex
        {
            public static Player.BodyModeIndex Gliding;

            public static void RegisterValues()
            {
                Gliding = new Player.BodyModeIndex("Gliding", true);
            }

            public static void UnregisterValues()
            {
                if (Gliding != null) { Gliding.Unregister(); Gliding = null; }
            }
        }
    }
}
