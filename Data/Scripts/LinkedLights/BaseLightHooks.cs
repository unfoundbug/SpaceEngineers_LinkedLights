using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.ModAPI;
using VRage.Utils;

namespace TestScript
{
    public static class BaseLightHooks
    {
        private static bool controlsInit = false;
        private static Guid storageGuid => new Guid("{F4D66A79-0469-47A3-903C-7964C8F65A25}");
        private static IMyTerminalControlSeparator separator;
        private static IMyTerminalControlListbox listControl;

        public static long GetTargetId(IMyLightingBlock source)
        {
            long targetBlockId = 0;
            if (source?.Storage?.ContainsKey(storageGuid) ?? false)
            {
                targetBlockId = long.Parse(source.Storage.GetValue(storageGuid));
            }

            return targetBlockId;
        }

        public static void SetTargetId(IMyLightingBlock target, long value)
        {
            if (target.Storage == null)
            {
                target.Storage = new MyModStorageComponent();
            }
            target.Storage.SetValue(storageGuid, value.ToString());
        }
        public static  void AttachControls()
        {
            if (!controlsInit)
            {
                controlsInit = true;
            }
            else
            {
                return;
            }

            //Logging.Instance.WriteLine("Light hook controls for " + typeof(IMyLightingBlock).Name + " started.");
            separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyLightingBlock>("lightlink_seperator");
            separator.Enabled = (MyObjectBuilder_LightingBlock) => { return true; };
            separator.Visible = (MyObjectBuilder_LightingBlock) => { return true; };

            listControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyLightingBlock>("lightlink_block");
            listControl.Title = MyStringId.GetOrCompute("Linked Block");
            listControl.Tooltip = MyStringId.GetOrCompute("If a block is selected here, the lights enable/disable will be bound to the selected block");
            listControl.SupportsMultipleBlocks = false;
            listControl.Visible = block => true;
            listControl.VisibleRowsCount = 8;
            listControl.ItemSelected = (block, selected) =>
            {
                //Logging.Instance.WriteLine("Light item selected: " + block.DisplayNameText + "has selected:  " + selected.Count.ToString());
                var localLight = (IMyLightingBlock)block;
                if (localLight != null && selected.Count != 0)
                {
                    string selectedTarget = ((long) selected.FirstOrDefault().UserData).ToString();
                    //Logging.Instance.WriteLine("Light: " + localLight.DisplayNameText + "has new value set: " + selectedTarget);
                    SetTargetId(localLight, (long)(selected.FirstOrDefault()?.UserData ?? 0));
                    //Logging.Instance.WriteLine("Value set");
                    localLight.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
            };
            listControl.ListContent = (block, items, selected) =>
            {
                //Logging.Instance.WriteLine("List content building!");
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();
                //Logging.Instance.WriteLine("Source block as lightsource: " + localLight == null ? "NULL" : "Not Null");
                items.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("None"),
                    MyStringId.GetOrCompute("None"), 0));
                //Logging.Instance.WriteLine("Added None Entry");
                List<IMyFunctionalBlock> foundBlockList = new List<IMyFunctionalBlock>();
                var funcBlocks = block.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
                //Logging.Instance.WriteLine("Found " + funcBlocks.ToList().Count + " functional block sources");
                long targetId = BaseLightHooks.GetTargetId(block as IMyLightingBlock);
                foreach (var funcBlock in funcBlocks)
                {
                    var newItem = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(funcBlock.DisplayNameText),
                        MyStringId.GetOrCompute(funcBlock.Name), funcBlock.EntityId);
                    if (funcBlock.EntityId == targetId)
                    {
                        //Logging.Instance.WriteLine("Setting " + funcBlock.DisplayNameText + "to selected");
                        selected.Add(newItem);
                    }

                    items.Add(newItem);
                }

                if (selected.Count == 0)
                {
                    selected.Add(items[0]);

                }

                listControl.RedrawControl();
                //Logging.Instance.WriteLine("Redrawn");

            };

            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(separator);
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(listControl);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(separator);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(listControl);

        }
    }
}
