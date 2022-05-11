using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using TestScript;
using VRage.Game.ModAPI;
using VRage.Utils;
using IMyFunctionalBlock = Sandbox.ModAPI.IMyFunctionalBlock;
using IMyLightingBlock = Sandbox.ModAPI.IMyLightingBlock;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;

namespace UnFoundBug.LightLink
{
    //here you can use any objectbuiler e.g. MyObjectBuilder_Door, MyObjectBuilder_Decoy
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_InteriorLight), false)]
    public class InteriorLightHooks : MyGameLogicComponent
    {
        private static bool controlsInit = false;
        private BaseLightHooks blh = new BaseLightHooks();
        
        //if you subscribed to events, please always unsubscribe them in close method 
        public override void Close()
        {
        }

        public InteriorLightHooks()
        {
            Logging.Instance.WriteLine("MyObjectBuilder_InteriorLightConstructed!");
            BaseLightHooks.AttachControls();

        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Logging.Instance.WriteLine("MyObjectBuilder Init started");
            base.Init(objectBuilder);
        }
    }
}