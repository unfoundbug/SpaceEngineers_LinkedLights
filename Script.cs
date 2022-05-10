
        /*
         * LinkedLights V0.2
         * Links lights to other blocks so lights turn on when other blocks are enabled.
         * 
         * Version History:
         * V0.2
         *      Same as V0.1, but now with comments!
         * V0.1
         *      Initial Upload, basic linking of lights to blocks
        */

        // Block cache to reduce server load
        IMyLightingBlock[] activeLights = null;
        IMyFunctionalBlock[] activeTargets = null;

        // Every 100 successful light updates, the lights are re-enumerated to check for changes
        int periodRescan = 100;

        // Enumeration is tied to update 100 to prevent the script from repeatedly scanning the grid.
        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string strIn, UpdateType trig)
        {
            if(trig == UpdateType.Update100 
                ||
                trig == UpdateType.Terminal)
            {
                Enumerate();
            }
            else if(trig == UpdateType.Update10)
            {
                LightUpdate();
            }
        }

        // Check all cached lights, and make sure the lights enable matches the target block
        private void LightUpdate()
        {
            int lightCount = activeLights.Count();
            for(int i = 0; i < activeLights.Count(); ++i)
            {
                var light = activeLights[i];
                var target = activeTargets[i];
                bool errorDuringProcess = false;
                try
                {
                    bool targetMode = target.Enabled;
                    if (light.Enabled != target.Enabled)
                    {
                        Echo("Light "+ light.Name + " changing to " + target.Enabled);
                        light.Enabled = target.Enabled;
                    }
                }
                catch
                {
                    errorDuringProcess = true;
                }
                if (errorDuringProcess)
                {
                    Echo("Light unable to refresh, enumerating at next cycle");
                    Runtime.UpdateFrequency = UpdateFrequency.Update100;
                }
            }

            --periodRescan;
            if(periodRescan == 0)
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            }
        }

        private void Enumerate()
        {
            List<IMyLightingBlock> processedLights = new List<IMyLightingBlock>();
            List<IMyFunctionalBlock> processedTargets = new List<IMyFunctionalBlock>();

            List<IMyLightingBlock> sourceLights = new List<IMyLightingBlock>();
            Echo("Enumeration Starting.");
            GridTerminalSystem.GetBlocksOfType(sourceLights, block => block.CustomData.Contains('['));
            Echo("Enumeration Found " + sourceLights.Count + " potentially linked lights");
            System.Text.RegularExpressions.Regex blockRegex = new System.Text.RegularExpressions.Regex("\\[(.+)\\]");
            char[] trimChars = new char[] { '[', ']' };
            foreach (var light in sourceLights)
            {
                var blockMatch = blockRegex.Match(light.CustomData);
                if(blockMatch.Success){
                    var targetBlockName = blockMatch.Value.Trim(trimChars);
                    var targetBlock = GridTerminalSystem.GetBlockWithName(targetBlockName);
                    if(targetBlock != null)
                    { 
                        IMyFunctionalBlock funcBlock = (IMyFunctionalBlock)targetBlock;
                        if (targetBlock != null)
                        {
                            processedLights.Add(light);
                            processedTargets.Add(funcBlock);
                        }
                    }
                }
            }
            
            Echo("Enumeration Complete. Found " + processedLights.Count + "lights were usable");
            activeLights = processedLights.ToArray();
            activeTargets = processedTargets.ToArray();
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            periodRescan = 100;
        }
