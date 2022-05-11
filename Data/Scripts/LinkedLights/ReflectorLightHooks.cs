using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParallelTasks;
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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ReflectorLight), true)]
    public class ReflectorLightHooks : MyGameLogicComponent
    {
        private IMyLightingBlock BaseLight => (IMyLightingBlock)this.Entity;

        private IMyFunctionalBlock targetBlock = null;
        
        //if you subscribed to events, please always unsubscribe them in close method 
        public override void Close()
        {
        }

        public ReflectorLightHooks()
        {
            //Logging.Instance.WriteLine("MyObjectBuilder_InteriorLightConstructed!");
            BaseLightHooks.AttachControls();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            //Logging.Instance.WriteLine("MyObjectBuilder Init started");
            base.Init(objectBuilder);
            Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            //Logging.Instance.WriteLine("Light update for " + this.BaseLight.DisplayNameText + " started.");

            long targetBlockId = BaseLightHooks.GetTargetId(this.BaseLight);
            //Logging.Instance.WriteLine("Found target block: " + targetBlockId.ToString());
            if (targetBlockId != 0)
            {
                var funcBlocks = this.BaseLight.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
                var target = funcBlocks.FirstOrDefault(found => found.EntityId == targetBlockId);
                AttachTarget(target);
            }
            else
            {
                DetachFromTarget();
            }

        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if(targetBlock!=null)
                if (targetBlock is IMyShipToolBase)
                {
                    var asTool = targetBlock as IMyShipToolBase;
                    var newEn = asTool.Enabled | asTool.IsActivated;
                    if (this.BaseLight.Enabled != newEn)
                    {
                        this.BaseLight.Enabled = newEn;
                    }
                }
                else
                {
                    //Logging.Instance.WriteLine(this.BaseLight.DisplayNameText + "is changing to " + this.targetBlock.Enabled.ToString());
                    this.BaseLight.Enabled = this.targetBlock.Enabled;
                }
        }

        private void DetachFromTarget()
        {
            if (targetBlock != null)
            {
                //Logging.Instance.WriteLine(this.BaseLight.DisplayNameText + "is now detached from " + this.targetBlock.DisplayNameText);
                targetBlock.OnMarkForClose -= TargetBlock_OnMarkForClose;
                targetBlock = null;
            }
        }

        private void AttachTarget(IMyFunctionalBlock newTarget)
        {
            //DetachFromTarget();
            if (newTarget != null)
            {
                this.targetBlock = newTarget;
                var targetAsTool = targetBlock as IMyShipToolBase;
                this.targetBlock.OnMarkForClose += TargetBlock_OnMarkForClose;
            }
        }

        private void TargetBlock_OnMarkForClose(IMyEntity obj)
        {
            DetachFromTarget();
        }
    }
}