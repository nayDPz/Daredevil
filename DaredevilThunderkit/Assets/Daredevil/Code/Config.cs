using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using BepInEx.Configuration;
namespace Daredevil
{
    public static class Config
    {
        public static ConfigEntry<float> comboVolume;
        public static void ReadConfig()
        {
            comboVolume = DaredevilMain.Instance.Config.Bind<float>(new ConfigDefinition("Sounds", "Combo Volume"), 
                100f, 
                new ConfigDescription("Volume of the combo system, 0-100"));
        }
    }
}
