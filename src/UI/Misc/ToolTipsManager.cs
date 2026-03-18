using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using DataGrid = System.Windows.Controls.DataGrid;
using ListBox = System.Windows.Controls.ListBox;
using ListView = System.Windows.Controls.ListView;
using NumericUpDown = HandyControl.Controls.NumericUpDown;
using RadioButton = System.Windows.Controls.RadioButton;
using UserControl = System.Windows.Controls.UserControl;

namespace eft_dma_radar.UI.Misc
{
    public static class TooltipManager
    {

        public static void AssignLootTooltips(UserControl context)
        {
            if (context.FindName("chkShowLoot") is CheckBox chkShowLoot)
                chkShowLoot.ToolTip = "Toggle loot display on or off.";

            if (context.FindName("chkShowLootWishlist") is CheckBox chkShowLootWishlist)
                chkShowLootWishlist.ToolTip = "Tracks loot on your account's Loot Wishlist (Manual Adds Only, does not work for Automatically Added Items).";

            if (context.FindName("sldrPriceRange") is HandyControl.Controls.TextBox txtRegularValue)
                txtRegularValue.ToolTip = "Set the Minimum & Valuable ruble value for items to show";

            if (context.FindName("chkPricePerSlot") is CheckBox chkPricePerSlot)
                chkPricePerSlot.ToolTip = "Use price per slot instead of total item value.";

            if (context.FindName("rdbFleaPrices") is RadioButton rdbFleaPrices)
                rdbFleaPrices.ToolTip = "Loot prices use the optimal flea market price for the item based on ~realtime market value for displayed loot items.";

            if (context.FindName("rdbTraderPrices") is RadioButton rdbTraderPrices)
                rdbTraderPrices.ToolTip = "Loot prices use the highest trader price for displayed loot items.";

            if (context.FindName("chkHideCorpses") is CheckBox chkHideCorpses)
                chkHideCorpses.ToolTip = "Hide dead player & AI loot bodies.";

            if (context.FindName("chkShowMeds") is CheckBox chkShowMeds)
                chkShowMeds.ToolTip = "Only show medical items.";

            if (context.FindName("chkShowFood") is CheckBox chkShowFood)
                chkShowFood.ToolTip = "Only show food & drink items.";

            if (context.FindName("chkShowBackpacks") is CheckBox chkShowBackpacks)
                chkShowBackpacks.ToolTip = "Only show backpack loot.";

            if (context.FindName("txtLootToSearch") is HandyControl.Controls.TextBox txtLootToSearch)
                txtLootToSearch.ToolTip = "Comma-separated item names to search (e.g. 'GPU,keycard').";

            if (context.FindName("chkStaticContainers") is CheckBox chkStaticContainers)
                chkStaticContainers.ToolTip = "Shows static containers on the map. Due to recent Tarkov Anti-Cheat Measures, you cannot see what the contents are however.";

            if (context.FindName("sldrContainerDistance") is Slider sldrContainerDistance)
                sldrContainerDistance.ToolTip = "Maximum distance at which loot containers are rendered.";

            if (context.FindName("chkContainersSelectAll") is CheckBox chkContainersSelectAll)
                chkContainersSelectAll.ToolTip = "Toggle all containers on or off.";

            if (context.FindName("chkContainersHideSearched") is CheckBox chkContainersHideSearched)
                chkContainersHideSearched.ToolTip = "Hides containers that have already been searched by a networked entity (usually ONLY yourself).";
        }

        public static void AssignLootFilterTooltips(UserControl context)
        {
            if (context.FindName("cboLootFilters") is HandyControl.Controls.ComboBox cboLootFilters)
                cboLootFilters.ToolTip = "The loot filter to modify.";

            if (context.FindName("txtNewGroupName") is HandyControl.Controls.TextBox txtNewGroupName)
                txtNewGroupName.ToolTip = "The name of the new loot filter.";

            if (context.FindName("btnRemoveGroup") is Button btnRemoveGroup)
                btnRemoveGroup.ToolTip = "Remove the selected loot filter.";

            if (context.FindName("btnAddGroup") is Button btnAddGroup)
                btnAddGroup.ToolTip = "Add a new loot filter.";

            if (context.FindName("chkEnabled") is CheckBox chkEnabled)
                chkEnabled.ToolTip = "Toggle the loot filter on/off.";

            if (context.FindName("chkStatic") is CheckBox chkStatic)
                chkStatic.ToolTip = "Removes the loot filter if unticked after restarting/closing radar.";

            if (context.FindName("chkNotify") is CheckBox chkNotify)
                chkNotify.ToolTip = "Notifies you if a selected item in the loot filter is present.";

            if (context.FindName("nudNotifyTime") is HandyControl.Controls.NumericUpDown nudNotifyTime)
                nudNotifyTime.ToolTip = "The repetition delay for loot item notifications (0 to disable).";

            if (context.FindName("nudGroupIndex") is HandyControl.Controls.NumericUpDown nudGroupIndex)
                nudGroupIndex.ToolTip = "The priority of displaying items (lower = displayed first)";

            if (context.FindName("txtGroupName") is HandyControl.Controls.TextBox txtGroupName)
                txtGroupName.ToolTip = "The name of the selected loot filter.";

            if (context.FindName("txtItemSearch") is HandyControl.Controls.TextBox txtItemSearch)
                txtItemSearch.ToolTip = "Type to search & filter available items.";

            if (context.FindName("btnAddItem") is Button btnAddItem)
                btnAddItem.ToolTip = "Add the selected item to your loot filter.";

            if (context.FindName("cboItems") is HandyControl.Controls.ComboBox cboItems)
                cboItems.ToolTip = "Available loot items to add.";

            if (context.FindName("itemsListView") is ListView itemsListView)
                itemsListView.ToolTip = "Your current filtered loot entries. Enable/disable or edit colors & notification status.";

            if (context.FindName("btnRemoveItem") is Button btnRemoveItem)
                btnRemoveItem.ToolTip = "Remove the selected item from your loot filter.";
        }

        public static void AssignESPTips(UserControl context)
        {
            if (context.FindName("chkEnableFuser") is CheckBox chkEnableFuser)
                chkEnableFuser.ToolTip = "Starts the ESP Window. This will render ESP over a black background. Move this window to the screen that is being fused.";

            if (context.FindName("chkAutoFullscreen") is CheckBox chkAutoFullscreen)
                chkAutoFullscreen.ToolTip = "Sets 'Auto Fullscreen' for the ESP Window.\nWhen set this will automatically go into full screen mode on the selected screen when the application starts.";

            if (context.FindName("cboHighAlert") is HandyControl.Controls.ComboBox cboHighAlert)
                cboHighAlert.ToolTip = "Enables the 'High Alert' ESP Feature. This will activate when you are being aimed at for longer than 0.5 seconds.\n" +
                    "Targets in your FOV (in front of you) will draw an aimline towards your character.\nTargets outside your FOV will draw the border of your screen red.\n" +
                    "None = Feature Disabled\nAllPlayers = Enabled for both players and bots (AI)\nHumansOnly = Enabled only for human-controlled players.";

            if (context.FindName("nudFPSCap") is NumericUpDown nudFPSCap)
                nudFPSCap.ToolTip = "Sets an FPS Cap for the ESP Window. Generally this can be the refresh rate of your Game PC Monitor. This also helps reduce resource usage on your Radar PC.";

            if (context.FindName("sldrFuserFontScale") is Slider sldrFuserFontScale)
                sldrFuserFontScale.ToolTip = "Sets the font scaling factor for the ESP Window.\nIf you are rendering at a really high resolution, you may want to increase this.";

            if (context.FindName("sldrFuserLineScale") is Slider sldrFuserLineScale)
                sldrFuserLineScale.ToolTip = "Sets the lines scaling factor for the ESP Window.\nIf you are rendering at a really high resolution, you may want to increase this.";

            if (context.FindName("chkCrosshairEnabled") is CheckBox chkCrosshairEnabled)
                chkCrosshairEnabled.ToolTip = "Toggles rendering a Crosshair on the ESP.";

            if (context.FindName("cboCrosshairType") is HandyControl.Controls.ComboBox cboCrosshairType)
                cboCrosshairType.ToolTip = "The type of Crosshair to display.";

            if (context.FindName("sldrFuserCrosshairScale") is Slider sldrFuserCrosshairScale)
                sldrFuserCrosshairScale.ToolTip = "Adjust the crosshair scale.";

            if (context.FindName("cboPlayerRenderMode") is HandyControl.Controls.ComboBox cboPlayerRenderMode)
                cboPlayerRenderMode.ToolTip = "Choose how players are displayed (e.g., Skeleton, Box or Head Dot).";

            if (context.FindName("chkFuserPlayerLabels") is CheckBox chkFuserPlayerLabels)
                chkFuserPlayerLabels.ToolTip = "Display entity label/name.";

            if (context.FindName("chkFuserPlayerWeapons") is CheckBox chkFuserPlayerWeapons)
                chkFuserPlayerWeapons.ToolTip = "Display entity's held weapon/ammo.";

            if (context.FindName("chkFuserPlayerDistance") is CheckBox chkFuserPlayerDistance)
                chkFuserPlayerDistance.ToolTip = "Display entity distance from LocalPlayer.";

            if (context.FindName("cboAIRenderMode") is HandyControl.Controls.ComboBox cboAIRenderMode)
                cboAIRenderMode.ToolTip = "Choose how AI are displayed (e.g., Skeleton, Box or Head Dot).";

            if (context.FindName("chkFuserAILabels") is CheckBox chkFuserAILabels)
                chkFuserAILabels.ToolTip = "Display entity label/name.";

            if (context.FindName("chkFuserAIWeapons") is CheckBox chkFuserAIWeapons)
                chkFuserAIWeapons.ToolTip = "Display entity's held weapon/ammo.";

            if (context.FindName("chkFuserAIDistance") is CheckBox chkFuserAIDistance)
                chkFuserAIDistance.ToolTip = "Display entity distance from LocalPlayer.";

            if (context.FindName("chkFuserLoot") is CheckBox chkFuserLoot)
                chkFuserLoot.ToolTip = "Enables the rendering of loot items in the ESP Window.";

            if (context.FindName("chkFuserExfils") is CheckBox chkFuserExfils)
                chkFuserExfils.ToolTip = "Enables the rendering of Exfil Points in the ESP Window.";

            if (context.FindName("chkFuserExplosives") is CheckBox chkFuserExplosives)
                chkFuserExplosives.ToolTip = "Enables the rendering of Grenades in the ESP Window.";

            if (context.FindName("chkFuserMagazine") is CheckBox chkFuserMagazine)
                chkFuserMagazine.ToolTip = "Shows your currently loaded Magazine Ammo Count/Type.";

            if (context.FindName("chkFuserDistances") is CheckBox chkFuserDistances)
                chkFuserDistances.ToolTip = "Enables the rendering of 'Distance' below ESP Entities. This is the In-Game distance from yourself and the entity.";

            if (context.FindName("chkFuserMines") is CheckBox chkFuserMines)
                chkFuserMines.ToolTip = "Display landmines.";

            if (context.FindName("chkFuserFireportAim") is CheckBox chkFuserFireportAim)
                chkFuserFireportAim.ToolTip = "Shows the base fireport trajectory on screen so you can see where bullets will go. Disappears when ADS.";

            if (context.FindName("chkFuserAimbotFOV") is CheckBox chkFuserAimbotFOV)
                chkFuserAimbotFOV.ToolTip = "Enables the rendering of an 'Aim FOV Circle' in the center of your ESP Window. This is used for Aimbot Targeting.";

            if (context.FindName("chkFuserRaidStats") is CheckBox chkFuserRaidStats)
                chkFuserRaidStats.ToolTip = "Displays Raid Stats (Player counts, etc.) in top right corner of ESP window.";

            if (context.FindName("chkFuserAimbotLock") is CheckBox chkFuserAimbotLock)
                chkFuserAimbotLock.ToolTip = "Enables the rendering of a line between your Fireport and your currently locked Aimbot Target.";

            if (context.FindName("chkFuserStatusText") is CheckBox chkFuserStatusText)
                chkFuserStatusText.ToolTip = "Displays status text in the top center of the screen (Aimbot Status, Wide Lean, etc.)";

            if (context.FindName("chkFuserFPS") is CheckBox chkFuserFPS)
                chkFuserFPS.ToolTip = "Enables the display of the ESP Rendering Rate (FPS) in the Top Left Corner of your ESP Window.";

            if (context.FindName("sldrFuserLootDistance") is Slider sldrFuserLootDistance)
                sldrFuserLootDistance.ToolTip = "Sets the Maximum Distance from LocalPlayer for regular loot to be rendered.";

            if (context.FindName("sldrFuserImportantLootDistance") is Slider sldrFuserImportantLootDistance)
                sldrFuserImportantLootDistance.ToolTip = "Sets the Maximum Distance from LocalPlayer for important loot to be rendered.";

            if (context.FindName("sldrFuserContainerDistance") is Slider sldrFuserContainerDistance)
                sldrFuserContainerDistance.ToolTip = "Sets the Maximum Distance from LocalPlayer for containers to be rendered.";

            if (context.FindName("sldrFuserQuestDistance") is Slider sldrFuserQuestDistance)
                sldrFuserQuestDistance.ToolTip = "Sets the Maximum Distance from LocalPlayer for Static Quest Items/Locations to be rendered. Quest Helper must be on.";

            if (context.FindName("sldrFuserExplosivesDistance") is Slider sldrFuserExplosivesDistance)
                sldrFuserExplosivesDistance.ToolTip = "Sets the Maximum Distance from LocalPlayer for explosives to be rendered.";
        }
        public static void AssignGeneralSettingsTooltips(UserControl context)
        {
            if (context.FindName("chkMapSetup") is CheckBox chkMapSetup)
                chkMapSetup.ToolTip = "Toggles the 'Map Setup Helper' to assist with getting Map Bounds/Scaling";

            if (context.FindName("chkESPWidget") is CheckBox chkESPWidget)
                chkESPWidget.ToolTip = "Toggles the ESP 'Widget' that gives you a Mini ESP in the radar window. Can be moved.";

            if (context.FindName("chkPlayerInfoWidget") is CheckBox chkPlayerInfoWidget)
                chkPlayerInfoWidget.ToolTip = "Toggles the Player Info 'Widget' that gives you information about the players/bosses in your raid. Can be moved.";

            if (context.FindName("chkConnectGroups") is CheckBox chkConnectGroups)
                chkConnectGroups.ToolTip = "Connects players that are grouped up via semi-transparent green lines. Does not apply to your own party.";

            if (context.FindName("chkHideNames") is CheckBox chkHideNames)
                chkHideNames.ToolTip = chkHideNames.ToolTip = "Hides all player names from ESP overlays.";

            if (context.FindName("chkMines") is CheckBox chkMines)
                chkMines.ToolTip = "Shows proximity mines on the map and ESP.";

            if (context.FindName("chkTeammateAimlines") is CheckBox chkTeammateAimlines)
                chkTeammateAimlines.ToolTip = "When enabled makes teammate aimlines the same length as the main player";

            if (context.FindName("chkAIAimlines") is CheckBox chkAIAimlines)
                chkAIAimlines.ToolTip = "Enables dynamic aimlines for AI Players. When you are being aimed at the aimlines will extend.";

            if (context.FindName("chkDebugWidget") is CheckBox chkDebugWidget)
                chkDebugWidget.ToolTip = "Toggles the Debug 'Widget' (only draws radar fps). Can be moved.";

            if (context.FindName("chkLootInfoWidget") is CheckBox chkLootInfoWidget)
                chkLootInfoWidget.ToolTip = "Toggles the Loot 'Widget' that shows top items in the match as well as their quantity. Can be moved.";

            if (context.FindName("nudFPSLimit") is NumericUpDown nudFPSLimit)
                nudFPSLimit.ToolTip = "Sets an FPS Limit for the Radar. This also helps reduce resource usage on your Radar PC.";

            if (context.FindName("sldrUIScale") is Slider sldrUIScale)
                sldrUIScale.ToolTip = "Sets the scaling factor for the Radar/User Interface. For high resolution monitors you may want to increase this.";

            if (context.FindName("sldrMaxDistance") is Slider sldrMaxDistance)
                sldrMaxDistance.ToolTip = "Sets the 'Maximum Distance' for the Radar and many of it's features. This will affect Hostile Aimlines, Aimview, ESP, and Aimbot.\nIn most cases you don't need to set this over 500.";

            if (context.FindName("sldrAimlineLength") is Slider sldrAimlineLength)
                sldrAimlineLength.ToolTip = "Sets the Aimline Length for Local Player/Teammates";

            if (context.FindName("sldrContainerDistance") is Slider sldrContainerDistance)
                sldrContainerDistance.ToolTip = "Distance at which containers are displayed on the ESP.";

            if (context.FindName("cboMonitor") is HandyControl.Controls.ComboBox cboMonitor)
                cboMonitor.ToolTip = "Select which monitor to render ESP on.";

            if (context.FindName("btnRefreshMonitors") is Button btnRefreshMonitors)
                btnRefreshMonitors.ToolTip = "Automatically detects the resolution of your Game PC Monitor that Tarkov runs on, and sets the Width/Height fields. Game must be running.";

            if (context.FindName("txtGameWidth") is HandyControl.Controls.TextBox txtFuserWidth)
                txtFuserWidth.ToolTip = "The resolution Width of your Game PC Monitor that Tarkov runs on. This must be correctly set for Aimview/Aimbot/ESP to function properly.";

            if (context.FindName("txtGameHeight") is HandyControl.Controls.TextBox txtFuserHeight)
                txtFuserHeight.ToolTip = "The resolution Height of your Game PC Monitor that Tarkov runs on. This must be correctly set for Aimview/Aimbot/ESP to function properly.";

            if (context.FindName("chkQuestHelper") is CheckBox chkQuestHelper)
                chkQuestHelper.ToolTip = "Toggles the Quest Helper feature. This will display Items and Zones that you need to pickup/visit for quests that you currently have active.";

            if (context.FindName("listQuests") is ListBox listQuests)
                listQuests.ToolTip = "Active Quest List (populates once you are in raid). Uncheck a quest to untrack it.";

            if (context.FindName("cboTheme") is HandyControl.Controls.ComboBox cboTheme)
                cboTheme.ToolTip = "Choose between Dark and Light themes.";

            if (context.FindName("hotkeyListView") is ListView hotkeyListView)
                hotkeyListView.ToolTip = "Displays all assigned hotkeys.";

            if (context.FindName("btnAddHotkey") is Button btnAddHotkey)
                btnAddHotkey.ToolTip = "Add a new hotkey binding.";

            if (context.FindName("btnRemoveHotkey") is Button btnRemoveHotkey)
                btnRemoveHotkey.ToolTip = "Remove the selected hotkey.";

            if (context.FindName("cboAction") is HandyControl.Controls.ComboBox cboAction)
                cboAction.ToolTip = "Select an action to assign a hotkey for.";

            if (context.FindName("cboKey") is HandyControl.Controls.ComboBox cboKey)
                cboKey.ToolTip = "Select the key that will trigger the selected action.";

            if (context.FindName("rdbOnKey") is RadioButton rdbOnKey)
                rdbOnKey.ToolTip = "Trigger the action while holding the key down.";

            if (context.FindName("rdbToggle") is RadioButton rdbToggle)
                rdbToggle.ToolTip = "Toggle the action on and off with the key.";
        }
    }
}
