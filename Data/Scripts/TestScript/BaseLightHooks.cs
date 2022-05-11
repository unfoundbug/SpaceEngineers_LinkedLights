using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.ModAPI;
using VRage.Utils;

namespace TestScript
{
    public class BaseLightHooks
    {
        private static bool controlsInit = false;
        private static Guid storageGuid = new Guid("{F4D66A79-0469-47A3-903C-7964C8F65A25}");
        private static IMyTerminalControlSeparator separator;
        private static IMyTerminalControlListbox listControl;

        public BaseLightHooks()
        {
            
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

            Logging.Instance.WriteLine("Light hook controls for " + typeof(IMyLightingBlock).Name + " started.");
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
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();
                if (localLight != null)
                {
                    localLight.Storage.SetValue(storageGuid, selected.FirstOrDefault()?.UserData.ToString() ?? "0");
                    localLight.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
            };
            listControl.ListContent = (block, items, selected) =>
            {
                Logging.Instance.WriteLine("List content building!");
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();
                Logging.Instance.WriteLine("Source block as lightsource: " + localLight == null ? "NULL" : "Not Null");
                items.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("None"),
                    MyStringId.GetOrCompute("None"), 0));
                Logging.Instance.WriteLine("Added None Entry");
                List<IMyFunctionalBlock> foundBlockList = new List<IMyFunctionalBlock>();
                var funcBlocks = block.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
                Logging.Instance.WriteLine("Found " + funcBlocks.ToList().Count + " functional block sources");
                var target = foundBlockList.FirstOrDefault(found => found.EntityId == long.Parse(block.Storage.GetValue(storageGuid)));
                foreach (var funcBlock in funcBlocks)
                {
                    Logging.Instance.WriteLine("Adding " + funcBlock.Name + "to list");
                    var newItem = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(funcBlock.DisplayNameText),
                        MyStringId.GetOrCompute(funcBlock.Name), funcBlock.EntityId);
                    if (funcBlock == target)
                    {
                        Logging.Instance.WriteLine("Setting " + funcBlock.DisplayNameText + "to selected");
                        selected.Add(newItem);
                    }

                    items.Add(newItem);
                }

                if (selected.Count == 0)
                {
                    selected.Add(items[0]);

                }

                listControl.RedrawControl();
                Logging.Instance.WriteLine("Redrawn");

            };

            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(separator);
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(listControl);
        }
    }
}
