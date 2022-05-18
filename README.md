Space Engineers Workshop;
Unfoundbug Light Linker 
https://steamcommunity.com/sharedfiles/filedetails/?id=2806143877

======================

Developed by UnFoundBug.

======================

	Linked Lights adds an option to lights which allows them to be turned off/on along with other blocks. 
	
	For example, warning lights which turn on when a welder or grinder is inactive.
	Or mood lighting when reactors are turned on. 


======================
V0.0.8
	Fixed potential crash when selecting None
	Add Tank light enable options
	Filter available enable options based on selected target

V0.0.7
	Fix Auto-Refresh issue.
	Enable name sorting of available blocks.

V0.0.6
	Better filtering of 'less interesting' blocks
	Filtering enabled by default
	New Charged enable source.
		When a battery is selected, Charged represents the battery above 99% charge. 99% is used instead of 100% to prevent flickering.

V0.0.5
	Enable selection of enable sources. Enabled, Active, Charging and Recharge Mode can be selected
		Enabled: Whether the block is enabled to receive power or not
		Active: Whether a ship tool is forced on or controlled by a mouse click
		Charging: If a battery is currently charging from the grid.
		Recharge Mode: If a battery is set to recharge mode.
	Enable searching of sub-grids
	Increased storage complexity to allow storage of not only linked blocks, but also enable source per light
	KNOWN ISSUE: Currently the List box does not update when subgrid searching is enabled/disabled, requires the block to be left and re-entered
V0.0.4
	Code reorganise and optimisations.
V0.0.3
	Fix new world crash.
V0.0.2
	New supported lights: Rotating Lights, Spot Lights
	Fix for quick save/load bug

V0.0.1: ModAPI Implementation of Linked Lights
	New Supported Lights: Interior Light, Light Panel
