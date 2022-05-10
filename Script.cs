/*
 * LinkedLights V0.2
 * Links lights to other blocks so lights turn on when other blocks are enabled.
 * 
 * Version History:
 * V0.3
 *      Light control is encapsulated 
 * V0.2
 *      Same as V0.1, but now with comments!
 * V0.1
 *      Initial Upload, basic linking of lights to blocks
*/

// The script will periodically look for new lights to manage, 
// The period is 0.5s * periodRescan
static int periodRescan = 100;

LightLinker llControl;

// Enumeration is tied to update 100 to prevent the script from repeatedly scanning the grid.
public Program() {
	Runtime.UpdateFrequency = UpdateFrequency.Update100;
	llControl = new LightLinker(periodRescan, this);
}

public void Main(string strIn, UpdateType trig)
{
	if(trig == UpdateType.Update100 
		||
		trig == UpdateType.Terminal)
	{
		llControl.Enumerate();
	}
	else if(trig == UpdateType.Update10)
	{
		llControl.LightUpdate();
	}
}

class LightLinker
{
	// Block cache to reduce server load
	IMyLightingBlock[] activeLights = null;
	IMyFunctionalBlock[] activeTargets = null;
	
	int periodRescan;
	int beforeRescan;
	
	Program parent;
	
	public LightLinker(int rescanPeriod, Program parent){
		this.periodRescan = rescanPeriod;
		this.beforeRescan = rescanPeriod;
		this.parent = parent;
	}
	
	// Check all cached lights, and make sure the lights enable matches the target block
	public void LightUpdate()
	{
		int lightCount = this.activeLights.Count();
		for(int i = 0; i < this.activeLights.Count(); ++i)
		{
			var light = this.activeLights[i];
			var target = this.activeTargets[i];
			bool errorDuringProcess = false;
			try
			{
				bool targetMode = target.Enabled;
				if (light.Enabled != target.Enabled)
				{
					this.parent.Echo("Light "+ light.Name + " changing to " + target.Enabled);
					light.Enabled = target.Enabled;
				}
			}
			catch
			{
				errorDuringProcess = true;
			}
			if (errorDuringProcess)
			{
				this.parent.Echo("Light unable to refresh, enumerating at next cycle");
				this.parent.Runtime.UpdateFrequency = UpdateFrequency.Update100;
			}
		}

		--beforeRescan;
		if(beforeRescan == 0)
		{
			this.parent.Runtime.UpdateFrequency = UpdateFrequency.Update100;
		}
	}

	public void Enumerate()
	{
		List<IMyLightingBlock> processedLights = new List<IMyLightingBlock>();
		List<IMyFunctionalBlock> processedTargets = new List<IMyFunctionalBlock>();

		List<IMyLightingBlock> sourceLights = new List<IMyLightingBlock>();
		this.parent.Echo("Enumeration Starting.");
		this.parent.GridTerminalSystem.GetBlocksOfType(sourceLights, block => block.CustomData.Contains('['));
		this.parent.Echo("Enumeration Found " + sourceLights.Count + " potentially linked lights");
		System.Text.RegularExpressions.Regex blockRegex = new System.Text.RegularExpressions.Regex("\\[(.+)\\]");
		char[] trimChars = new char[] { '[', ']' };
		foreach (var light in sourceLights)
		{
			var blockMatch = blockRegex.Match(light.CustomData);
			if(blockMatch.Success){
				var targetBlockName = blockMatch.Value.Trim(trimChars);
				var targetBlock = this.parent.GridTerminalSystem.GetBlockWithName(targetBlockName);
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
		
		this.parent.Echo("Enumeration Complete. Found " + processedLights.Count + "lights were usable");
		this.activeLights = processedLights.ToArray();
		this.activeTargets = processedTargets.ToArray();
		this.parent.Runtime.UpdateFrequency = UpdateFrequency.Update10;
		this.beforeRescan = this.periodRescan;
	}

}

