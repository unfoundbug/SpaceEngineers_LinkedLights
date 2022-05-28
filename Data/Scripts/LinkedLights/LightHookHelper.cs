// <copyright file="LightHookHelper.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Game.Gui;
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
            // Logging.Instance.WriteLine("Light hook controls for " + typeof(IMyLightingBlock).Name + " started.");
            separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyLightingBlock>("lightlink_seperator");
            separator.Enabled = (lb) => true;
            separator.Visible = (lb) => true;

            scanSubgridCb = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyLightingBlock>("lightlink_scanSubGrid");
            scanSubgridCb.Title = MyStringId.GetOrCompute("Scan Subgrids");
            scanSubgridCb.OnText = MyStringId.GetOrCompute("Enabled");
            scanSubgridCb.OffText = MyStringId.GetOrCompute("Disabled");
            scanSubgridCb.Tooltip = MyStringId.GetOrCompute("WARNING: Can cause alot of server load!");
            scanSubgridCb.Getter = block =>
            {
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                return logic.EnableSubGrid;
            };
            scanSubgridCb.Setter = (block, value) =>
            {
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                logic.EnableSubGrid = value;
                listControl.UpdateVisual();
            };
            scanSubgridCb.Visible = block => true;

            filterListCB = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyLightingBlock>("lightlink_scanSubGrid");
            filterListCB.Title = MyStringId.GetOrCompute("Filter Available blocks");
            filterListCB.Tooltip = MyStringId.GetOrCompute("Filters less interesting blocks from appearing in the list, re-enter the menu for this to take effect.");
            filterListCB.OnText = MyStringId.GetOrCompute("Enabled");
            filterListCB.OffText = MyStringId.GetOrCompute("Disabled");
            filterListCB.Getter = block =>
            {
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                return logic.EnableFiltering;
            };
            filterListCB.Setter = (block, value) =>
            {
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                logic.EnableFiltering = value;
                listControl.UpdateVisual();
                block.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            };
            filterListCB.Visible = block => true;

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
                    var logic = block.GameLogic.GetAs<BaseLightHooks>();
                    logic.TargetEntity = (long)(selected.FirstOrDefault()?.UserData ?? 0);

                    localLight.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

                    flagControl.UpdateVisual();
                }
            };
            listControl.ListContent = (block, items, selected) =>
            {
                // Logging.Instance.WriteLine("List content building!");
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                long targetId = logic.TargetEntity;

                // Logging.Instance.WriteLine("Source block as lightsource: " + localLight == null ? "NULL" : "Not Null");
                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("None"),
                    MyStringId.GetOrCompute("None"),
                    0L));

                // Logging.Instance.WriteLine("Added None Entry");
                List<IMyFunctionalBlock> foundBlockList = new List<IMyFunctionalBlock>();
                List<IMyCubeGrid> activeGrids = new List<IMyCubeGrid>();
                List<long> addedBlocks = new List<long>();
                if (!logic.EnableSubGrid)
                {
                    activeGrids.Add(block.CubeGrid);
                }
                else
                {
                    block.CubeGrid.GetGridGroup(GridLinkTypeEnum.Physical).GetGrids(activeGrids);
                }

                List<MyTerminalControlListBoxItem> resultsList = new List<MyTerminalControlListBoxItem>();

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

                        addedBlocks.Add(funcBlock.EntityId);

                        if (logic.EnableFiltering)
                        {
                            if (funcBlock is IMyLightingBlock)
                            {
                                continue;
                            }

                            if (funcBlock is IMyDoor)
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

                            if (funcBlock is IMyThrust)
                            {
                                continue;
                            }
                        }

                        var newItem = new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute(funcBlock.DisplayNameText),
                            MyStringId.GetOrCompute(funcBlock.Name),
                            funcBlock.EntityId);
                        if (funcBlock.EntityId == targetId)
                        {
                            // Logging.Instance.WriteLine("Setting " + funcBlock.DisplayNameText + "to selected");
                            selected.Add(newItem);
                        }

                        resultsList.Add(newItem);
                    }
                }

                if (selected.Count == 0)
                {
                    selected.Add(resultsList[0]);
                }

                resultsList.Sort(new CLBIComparer());

                resultsList.ForEach(entry => items.Add(entry));

                // Logging.Instance.WriteLine("Redrawn");
            };

            flagControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyLightingBlock>("lightlink_flags");
            flagControl.Title = MyStringId.GetOrCompute("Enable source");
            flagControl.Tooltip = MyStringId.GetOrCompute("Each selected option is ORd together, to get the light's state");
            flagControl.Multiselect = false;
            flagControl.Visible = block => true;
            flagControl.VisibleRowsCount = 8;
            flagControl.ItemSelected = (block, selected) =>
            {
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                LightEnableOptions resultant = (LightEnableOptions)selected.First().UserData;
                logic.LightEnableOption = resultant;
            };
            flagControl.ListContent = (block, items, selected) =>
            {
                var logic = block.GameLogic.GetAs<BaseLightHooks>();
                // Logging.Instance.WriteLine("List content building!");
                var localLight = block.GameLogic.GetAs<IMyLightingBlock>();

                IMyEntity targetBlock = null;
                if (logic.TargetEntity != 0)
                {
                    targetBlock = MyAPIGateway.Entities.GetEntityById(logic.TargetEntity);
                }

                if (targetBlock != null)
                {
                    items.Add(new MyTerminalControlListBoxItem(
                        MyStringId.GetOrCompute("Enable"),
                        MyStringId.GetOrCompute("Is the block Enabled"),
                        LightEnableOptions.Generic_Enable));
                    if (logic.LightEnableOption == LightEnableOptions.Generic_Enable)
                    {
                        selected.Add(items.Last());
                    }

                    items.Add(new MyTerminalControlListBoxItem(
                        MyStringId.GetOrCompute("Functional"),
                        MyStringId.GetOrCompute("Is the block in a runnable state"),
                        LightEnableOptions.Generic_IsFunctional));
                    if (logic.LightEnableOption == LightEnableOptions.Generic_IsFunctional)
                    {
                        selected.Add(items.Last());
                    }

                    if (targetBlock is IMyShipToolBase || targetBlock is IMyProductionBlock)
                    {
                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Active"),
                            MyStringId.GetOrCompute("Tools only: Is the tool running"),
                            LightEnableOptions.Tool_IsActive));
                        if (logic.LightEnableOption == LightEnableOptions.Tool_IsActive)
                        {
                            selected.Add(items.Last());
                        }
                    }

                    if (targetBlock is IMyBatteryBlock)
                    {
                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Charging"),
                            MyStringId.GetOrCompute("Batteries only: Is the battery charging"),
                            LightEnableOptions.Battery_Charging));
                        if (logic.LightEnableOption == LightEnableOptions.Battery_Charging)
                        {
                            selected.Add(items.Last());
                        }

                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Charged"),
                            MyStringId.GetOrCompute(
                                "Batteries only: Is the battery full? (99% or above to prevent flickering)"),
                            LightEnableOptions.Battery_Charged));
                        if (logic.LightEnableOption == LightEnableOptions.Battery_Charged)
                        {
                            selected.Add(items.Last());
                        }

                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Recharge Mode"),
                            MyStringId.GetOrCompute("Batteries only: is the battery set to charge mode"),
                            LightEnableOptions.Battery_ChargeMode));
                        if (logic.LightEnableOption == LightEnableOptions.Battery_ChargeMode)
                        {
                            selected.Add(items.Last());
                        }
                    }

                    if (targetBlock is IMyGasTank)
                    {
                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Full"),
                            MyStringId.GetOrCompute("The tank is full"),
                            LightEnableOptions.Tank_Full));
                        if (logic.LightEnableOption == LightEnableOptions.Tank_Full)
                        {
                            selected.Add(items.Last());
                        }

                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Stockpile"),
                            MyStringId.GetOrCompute("The tank is stockpiling"),
                            LightEnableOptions.Tank_Stockpile));
                        if (logic.LightEnableOption == LightEnableOptions.Tank_Stockpile)
                        {
                            selected.Add(items.Last());
                        }
                    }

                    if (targetBlock is IMyThrust)
                    {
                        items.Add(new MyTerminalControlListBoxItem(
                            MyStringId.GetOrCompute("Thrust"),
                            MyStringId.GetOrCompute("EXPERIMENTAL: Brightness of light = thrust, colour is R/G/B in Custom data."),
                            LightEnableOptions.Thrust_Power));

                        if (logic.LightEnableOption == LightEnableOptions.Thrust_Power)
                        {
                            selected.Add(items.Last());
                        }
                    }
                }
            };
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
        }
    }
}
