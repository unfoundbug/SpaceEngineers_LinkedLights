// <copyright file="LightHookHelper.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.Utils;

    /// <summary>
    /// Static container of helper functions.
    /// </summary>
    public static class LightHookHelper
    {
        private static bool controlsInit = false;
        private static IMyTerminalControlSeparator separator;
        private static IMyTerminalControlListbox listControl;
        private static IMyTerminalControlOnOffSwitch scanSubgridCb;
        private static IMyTerminalControlOnOffSwitch filterListCB;
        private static IMyTerminalControlListbox flagControl;

        /// <summary>
        /// Attach controls to light terminal menus.
        /// </summary>
        public static void AttachControls()
        {
            if (!controlsInit)
            {
                Logging.Debug("Initialising Controls");
                controlsInit = true;
            }
            else
            {
                return;
            }

            // Logging.Instance.WriteLine("Light hook controls for " + typeof(IMyLightingBlock).Name + " started.");
            separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyLightingBlock>("lightlink_seperator");
            separator.Enabled = (lb) => true;
            separator.Visible = (lb) => true;

            Logging.Debug("Seperator initialised");

            scanSubgridCb = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyLightingBlock>("lightlink_scanSubGrid");
            scanSubgridCb.Title = MyStringId.GetOrCompute("Scan Subgrids");
            scanSubgridCb.OnText = MyStringId.GetOrCompute("Enabled");
            scanSubgridCb.OffText = MyStringId.GetOrCompute("Disabled");
            scanSubgridCb.Tooltip = MyStringId.GetOrCompute("WARNING: Can cause alot of server load!");
            scanSubgridCb.Getter = block =>
            {
                StorageHandler handler = new StorageHandler(block);
                return handler.SubGridScanningEnable;
            };
            scanSubgridCb.Setter = (block, value) =>
            {
                StorageHandler handler = new StorageHandler(block);
                handler.SubGridScanningEnable = value;
                MyAPIGateway.TerminalControls.GetControls<IMyLightingBlock>(out var controls);
                controls.Where(control => control.Id == "lightlink_block").ForEach(list => list.UpdateVisual());
                block.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            };
            scanSubgridCb.Visible = block => true;
            Logging.Debug("SubGridOnOff initialised");

            filterListCB = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyLightingBlock>("lightlink_scanSubGrid");
            filterListCB.Title = MyStringId.GetOrCompute("Filter Available blocks");
            filterListCB.Tooltip = MyStringId.GetOrCompute("Filters less interesting blocks from appearing in the list, re-enter the menu for this to take effect.");
            filterListCB.OnText = MyStringId.GetOrCompute("Enabled");
            filterListCB.OffText = MyStringId.GetOrCompute("Disabled");
            filterListCB.Getter = block =>
            {
                StorageHandler handler = new StorageHandler(block);
                return handler.BlockFiltering;
            };
            filterListCB.Setter = (block, value) =>
            {
                StorageHandler handler = new StorageHandler(block);
                handler.BlockFiltering = value;
                MyAPIGateway.TerminalControls.GetControls<IMyLightingBlock>(out var controls);
                controls.Where(control => control.Id == "lightlink_block").ForEach(list => list.UpdateVisual());
                block.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            };
            filterListCB.Visible = block => true;
            Logging.Debug("FilterListOnOff initialised");

            listControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyLightingBlock>("lightlink_block");
            listControl.Title = MyStringId.GetOrCompute("Linked Block");
            listControl.Tooltip = MyStringId.GetOrCompute("If a block is selected here, the lights enable/disable will be bound to the selected block");
            listControl.SupportsMultipleBlocks = false;
            listControl.Visible = block => true;
            listControl.VisibleRowsCount = 8;
            listControl.ItemSelected = (block, selected) =>
            {
                // Logging.Instance.WriteLine("Light item selected: " + block.DisplayNameText + MyAPIGateway.Gui.ChangeInteractedEntity();"has selected:  " + selected.Count.ToString());
                var localLight = (IMyLightingBlock)block;
                if (localLight != null && selected.Count != 0)
                {
                    StorageHandler storage = new StorageHandler(block);
                    storage.TargetEntity = (long)selected.FirstOrDefault().UserData;

                    // Logging.Instance.WriteLine("Value set");
                    localLight.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

                    MyAPIGateway.TerminalControls.GetControls<IMyLightingBlock>(out var controls);
                    controls.Where(control => control.Id == "lightlink_flags").ForEach(list => list.UpdateVisual());
                }
            };
            listControl.ListContent = (block, items, selected) =>
            {
                // Logging.Instance.WriteLine("List content building!");
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();
                StorageHandler storage = new StorageHandler(block);
                long targetId = storage.TargetEntity;

                // Logging.Instance.WriteLine("Source block as lightsource: " + localLight == null ? "NULL" : "Not Null");
                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("None"),
                    MyStringId.GetOrCompute("None"),
                    0));

                // Logging.Instance.WriteLine("Added None Entry");
                List<IMyFunctionalBlock> foundBlockList = new List<IMyFunctionalBlock>();
                List<IMyCubeGrid> activeGrids = new List<IMyCubeGrid>();
                List<long> addedBlocks = new List<long>();
                if (!storage.SubGridScanningEnable)
                {
                    activeGrids.Add(block.CubeGrid);
                }
                else
                {
                    block.CubeGrid.GetGridGroup(GridLinkTypeEnum.Physical).GetGrids(activeGrids);
                }

                foreach (var activeGrid in activeGrids)
                {
                    var funcBlocks = activeGrid.GetFatBlocks<IMyFunctionalBlock>();

                    // Logging.Instance.WriteLine("Found " + funcBlocks.ToList().Count + " functional block sources");
                    foreach (var funcBlock in funcBlocks)
                    {

                        if (addedBlocks.Contains(funcBlock.EntityId))
                        {
                            continue;
                        }

                        if (storage.BlockFiltering)
                        {
                            if (funcBlock is IMyLightingBlock)
                            {
                                continue;
                            }

                            if (funcBlock is IMyDoor)
                            {
                                continue;
                            }

                            if (funcBlock is IMyGasTank)
                            {
                                continue;
                            }

                            if (funcBlock is IMyRadioAntenna)
                            {
                                continue;
                            }

                            if (funcBlock is IMyUpgradeModule)
                            {
                                continue;
                            }

                            if (funcBlock is IMyTextPanel)
                            {
                                continue;
                            }
                        }

                        addedBlocks.Add(funcBlock.EntityId);

                        var newItem = new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute(funcBlock.DisplayNameText),
                            MyStringId.GetOrCompute(funcBlock.Name),
                            funcBlock.EntityId);
                        if (funcBlock.EntityId == targetId)
                        {
                            // Logging.Instance.WriteLine("Setting " + funcBlock.DisplayNameText + "to selected");
                            selected.Add(newItem);
                        }

                        items.Add(newItem);
                    }
                }

                if (selected.Count == 0)
                {
                    selected.Add(items[0]);
                }

                listControl.RedrawControl();

                // Logging.Instance.WriteLine("Redrawn");
            };
            Logging.Debug("ListControl initialised");

            flagControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyLightingBlock>("lightlink_flags");
            flagControl.Title = MyStringId.GetOrCompute("Enable source");
            flagControl.Tooltip = MyStringId.GetOrCompute("Each selected option is ORd together, to get the light's state");
            flagControl.Multiselect = true;
            flagControl.Visible = block => true;
            flagControl.VisibleRowsCount = 8;
            flagControl.ItemSelected = (block, selected) =>
            {
                LightEnableOptions resultant = 0;
                foreach (var selection in selected)
                {
                    resultant |= (LightEnableOptions)selection.UserData;
                }

                StorageHandler handler = new StorageHandler(block);
                handler.ActiveFlags = resultant;
                block.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            };
            flagControl.ListContent = (block, items, selected) =>
            {
                // Logging.Instance.WriteLine("List content building!");
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();
                StorageHandler storage = new StorageHandler(block);

                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Enable"),
                    MyStringId.GetOrCompute("Is the block Enabled"),
                    LightEnableOptions.Generic_Enable));
                if ((storage.ActiveFlags & LightEnableOptions.Generic_Enable) == LightEnableOptions.Generic_Enable)
                {
                    selected.Add(items.Last());
                }

                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Functional"),
                    MyStringId.GetOrCompute("Is the block in a runnable state"),
                    LightEnableOptions.Generic_IsFunctional));
                if ((storage.ActiveFlags & LightEnableOptions.Generic_IsFunctional) == LightEnableOptions.Generic_IsFunctional)
                {
                    selected.Add(items.Last());
                }

                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Active"),
                    MyStringId.GetOrCompute("Tools only: Is the tool running"),
                    LightEnableOptions.Tool_IsActive));
                if ((storage.ActiveFlags & LightEnableOptions.Tool_IsActive) == LightEnableOptions.Tool_IsActive)
                {
                    selected.Add(items.Last());
                }

                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Charging"),
                    MyStringId.GetOrCompute("Batteries only: Is the battery charging"),
                    LightEnableOptions.Battery_Charging));
                if ((storage.ActiveFlags & LightEnableOptions.Battery_Charging) == LightEnableOptions.Battery_Charging)
                {
                    selected.Add(items.Last());
                }

                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Charged"),
                    MyStringId.GetOrCompute("Batteries only: Is the battery full? (99% or above to prevent flickering)"),
                    LightEnableOptions.Battery_Charged));
                if ((storage.ActiveFlags & LightEnableOptions.Battery_Charged) == LightEnableOptions.Battery_Charged)
                {
                    selected.Add(items.Last());
                }

                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Recharge Mode"),
                    MyStringId.GetOrCompute("Batteries only: is the battery set to charge mode"),
                    LightEnableOptions.Battery_ChargeMode));
                if ((storage.ActiveFlags & LightEnableOptions.Battery_ChargeMode) == LightEnableOptions.Battery_ChargeMode)
                {
                    selected.Add(items.Last());
                }
            };
            Logging.Debug("FlagControl initialised");
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(separator);
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(scanSubgridCb);
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(filterListCB);
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(listControl);
            MyAPIGateway.TerminalControls.AddControl<IMyLightingBlock>(flagControl);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(separator);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(scanSubgridCb);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(filterListCB);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(listControl);
            MyAPIGateway.TerminalControls.AddControl<IMyReflectorLight>(flagControl);
            Logging.Debug("Controls Registered");
        }
    }
}
