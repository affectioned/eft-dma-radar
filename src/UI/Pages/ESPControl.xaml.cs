using eft_dma_radar.Common.DMA.Features;
using eft_dma_radar.Common.Misc;
using eft_dma_radar.Common.Unity;
using eft_dma_radar.Tarkov.Features;
using eft_dma_radar.Tarkov.GameWorld;
using eft_dma_radar.Tarkov.Unity.IL2CPP;
using eft_dma_radar.UI.Controls;
using eft_dma_radar.UI.ESP;
using eft_dma_radar.UI.Misc;
using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static eft_dma_radar.Tarkov.EFTPlayer.Player;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = HandyControl.Controls.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using Window = System.Windows.Window;

namespace eft_dma_radar.UI.Pages
{
    /// <summary>
    /// Interaction logic for ESPSettingsControl.xaml
    /// </summary>
    public partial class ESPControl : UserControl
    {
        #region Fields and Properties
        private const int INTERVAL = 100; // 0.1 second
        private Point _dragStartPoint;
        public event EventHandler CloseRequested;
        public event EventHandler BringToFrontRequested;
        public event EventHandler<PanelDragEventArgs> DragRequested;
        public event EventHandler<PanelResizeEventArgs> ResizeRequested;

        private PopupWindow _openColorPicker;

        private static Config Config => Program.Config;

        private bool _isImporting = false;

        private string _currentFuserPlayerType;
        private string _currentFuserEntityType;
        private bool _isLoadingFuserOptionSettings = false;
        private bool _isLoadingFuserPlayerSettings = false;
        private bool _isLoadingFuserEntitySettings = false;

        private readonly string[] _availableWidgets = new string[]
        {
            "Quest Info Widget",
            "Hotkey Info Widget"
        };

        private readonly string[] _availableFuserOptions = new string[]
        {
            "Fireport Aim",
            "Aimbot FOV",
            "Raid Stats",
            "KillFeed",
            "Aimbot Lock",
            "Status Text",
            "FPS",
            "Energy/Hydration Bar",
            "Magazine Info",
            "Closest Player",
            "Top Loot"
        };

        private readonly string[] _availableFuserPlayerInformation = new string[]
        {
            "ADS",
            "Ammo Type",
            "Distance",
            "Health",
            "Name",
            "KD",
            "Night Vision",
            "Thermal",
            "UBGL",
            "Weapon"
        };

        private readonly string[] _availableFuserEntityInformation = new string[]
        {
            "Name",
            "Distance",
            "Value"
        };
        #endregion

        public ESPControl()
        {
            InitializeComponent();
            TooltipManager.AssignESPTips(this);

            this.Loaded += async (s, e) =>
            {
                while (MainWindow.Config == null)
                {
                    await Task.Delay(INTERVAL);
                }

                PanelCoordinator.Instance.SetPanelReady("ESP");
                ExpanderManager.Instance.RegisterExpanders(this, "ESPSettings",
                    expFuserGeneralSettings,
                    expFuserCrosshairSettings,
                    expFuserMiniRadarSettings,
                    expFuserPlayerInformation,
                    expFuserEntityInformation);

                try
                {
                    await PanelCoordinator.Instance.WaitForAllPanelsAsync();

                    InitializeControlEvents();
                    LoadSettings();
                }
                catch (TimeoutException ex)
                {
                    XMLogging.WriteLine($"[PANELS] {ex.Message}");
                }
            };
        }

        #region ESP Panel
        #region Functions/Methods
        private void InitializeControlEvents()
        {
            Dispatcher.InvokeAsync(() =>
            {
                RegisterPanelEvents();
                RegisterFuserEvents();
            });
        }

        private void RegisterPanelEvents()
        {
            // Header close button
            btnCloseHeader.Click += btnCloseHeader_Click;

            // Drag handling
            DragHandle.MouseLeftButtonDown += DragHandle_MouseLeftButtonDown;
        }

        public async void LoadSettings()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                LoadFuserSettings();
            });
        }
        #endregion

        #region Events
        private void btnCloseHeader_Click(object sender, RoutedEventArgs e)
        {
            _openColorPicker?.Close();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BringToFrontRequested?.Invoke(this, EventArgs.Empty);

            DragHandle.CaptureMouse();
            _dragStartPoint = e.GetPosition(this);

            DragHandle.MouseMove += DragHandle_MouseMove;
            DragHandle.MouseLeftButtonUp += DragHandle_MouseLeftButtonUp;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _dragStartPoint;

                DragRequested?.Invoke(this, new PanelDragEventArgs(offset.X, offset.Y));
            }
        }

        private void DragHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DragHandle.ReleaseMouseCapture();
            DragHandle.MouseMove -= DragHandle_MouseMove;
            DragHandle.MouseLeftButtonUp -= DragHandle_MouseLeftButtonUp;
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).CaptureMouse();
            _dragStartPoint = e.GetPosition(this);

            ((UIElement)sender).MouseMove += ResizeHandle_MouseMove;
            ((UIElement)sender).MouseLeftButtonUp += ResizeHandle_MouseLeftButtonUp;
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var sizeDelta = currentPosition - _dragStartPoint;

                ResizeRequested?.Invoke(this, new PanelResizeEventArgs(sizeDelta.X, sizeDelta.Y));
                _dragStartPoint = currentPosition;
            }
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            ((UIElement)sender).MouseMove -= ResizeHandle_MouseMove;
            ((UIElement)sender).MouseLeftButtonUp -= ResizeHandle_MouseLeftButtonUp;
        }
        #endregion

        #region Fuser Tab
        #region Functions/Methods
        private void RegisterFuserEvents()
        {
            // General Settings
            btnStartESP.Click += btnStartESP_Click;
            nudFPSCap.ValueChanged += nudFPSCap_ValueChanged;
            chkAutoFullscreen.Checked += FuserCheckbox_Checked;
            chkAutoFullscreen.Unchecked += FuserCheckbox_Checked;
            sldrFuserFontScale.ValueChanged += FuserSlider_ValueChanged;
            sldrFuserLineScale.ValueChanged += FuserSlider_ValueChanged;
            ccbWidgets.SelectionChanged += widgetsCheckComboBox_SelectionChanged;
            ccbESPOptions.SelectionChanged += espOptionsCheckComboBox_SelectionChanged;

            // Crosshair Settings
            chkCrosshairEnabled.Checked += FuserCheckbox_Checked;
            chkCrosshairEnabled.Unchecked += FuserCheckbox_Checked;
            cboCrosshairType.SelectionChanged += FuserComboBox_SelectionChanged;
            sldrFuserCrosshairScale.ValueChanged += FuserSlider_ValueChanged;

            // Mini Radar Settings
            chkMiniRadarEnabled.Checked += FuserCheckbox_Checked;
            chkMiniRadarEnabled.Unchecked += FuserCheckbox_Checked;
            chkMiniRadarLoot.Checked += FuserCheckbox_Checked;
            chkMiniRadarLoot.Unchecked += FuserCheckbox_Checked;
            sldrFuserMiniRadarScale.ValueChanged += FuserSlider_ValueChanged;

            // Player Information
            cboFuserPlayerType.SelectionChanged += cboFuserPlayerType_SelectionChanged;
            chkHighAlert.Checked += FuserCheckbox_Checked;
            chkHighAlert.Unchecked += FuserCheckbox_Checked;
            chkImportantIndicators.Checked += FuserCheckbox_Checked;
            chkImportantIndicators.Unchecked += FuserCheckbox_Checked;
            chkShowImportantPlayerLoot.Checked += FuserCheckbox_Checked;
            chkShowImportantPlayerLoot.Unchecked += FuserCheckbox_Checked;
            sldrPlayerTypeRenderDistance.ValueChanged += FuserSlider_ValueChanged;
            cboPlayerRenderMode.SelectionChanged += FuserComboBox_SelectionChanged;
            ccbFuserPlayerInformation.SelectionChanged += espPlayerInfoCheckComboBox_SelectionChanged;
            sldrMinimumKD.ValueChanged += FuserSlider_ValueChanged;

            // Entity Information
            cboFuserEntityType.SelectionChanged += cboFuserEntityType_SelectionChanged;
            chkShowImportantCorpseLoot.Checked += FuserCheckbox_Checked;
            chkShowImportantCorpseLoot.Unchecked += FuserCheckbox_Checked;
            chkShowLockedDoors.Checked += FuserCheckbox_Checked;
            chkShowLockedDoors.Unchecked += FuserCheckbox_Checked;
            chkShowUnlockedDoors.Checked += FuserCheckbox_Checked;
            chkShowUnlockedDoors.Unchecked += FuserCheckbox_Checked;
            chkShowTripwireLine.Checked += FuserCheckbox_Checked;
            chkShowTripwireLine.Unchecked += FuserCheckbox_Checked;
            chkShowGrenadeRadius.Checked += FuserCheckbox_Checked;
            chkShowGrenadeRadius.Unchecked += FuserCheckbox_Checked;
            sldrTrailDuration.ValueChanged += FuserSlider_ValueChanged;
            sldrMinTrailDistance.ValueChanged += FuserSlider_ValueChanged;
            cboEntityRenderMode.SelectionChanged += FuserComboBox_SelectionChanged;
            sldrEntityTypeRenderDistance.ValueChanged += FuserSlider_ValueChanged;
            ccbFuserEntityInformation.SelectionChanged += espEntityInfoCheckComboBox_SelectionChanged;
        }

        private void LoadFuserSettings()
        {
            var cfg = Config.ESP;
            // Load available monitors
            LoadMonitors();

            // General
            chkAutoFullscreen.IsChecked = cfg.AutoFullscreen;
            nudFPSCap.Value = cfg.FPSCap;
            sldrFuserFontScale.Value = cfg.FontScale;
            sldrFuserLineScale.Value = cfg.LineScale;
            InitializeFuserOptions();

            // Crosshair
            var crosshairEnabled = cfg.Crosshair.Enabled;
            chkCrosshairEnabled.IsChecked = crosshairEnabled;
            cboCrosshairType.SelectedIndex = cfg.Crosshair.Type;
            sldrFuserCrosshairScale.Value = cfg.Crosshair.Scale;
            sldrFuserCrosshairScale.IsEnabled = crosshairEnabled;

            // Mini Radar
            chkMiniRadarEnabled.IsChecked = cfg.MiniRadar.Enabled;
            chkMiniRadarLoot.IsChecked = cfg.MiniRadar.ShowLoot;
            sldrFuserMiniRadarScale.Value = cfg.MiniRadar.Scale;
            ToggleMiniRadarControls();

            // Player Type Settings
            InitializeFuserPlayerTypeSettings();

            // Entity Type Settings
            InitializeFuserEntityTypeSettings();

            if (cfg.AutoFullscreen)
                btnStartESP_Click(null, null);
        }

        private void InitializeFuserPlayerTypeSettings()
        {
            if (Config.ESP.PlayerTypeESPSettings == null)
                Config.ESP.PlayerTypeESPSettings = new PlayerTypeSettingsESPConfig();

            Config.ESP.PlayerTypeESPSettings.InitializeDefaults();
            Config.Save();
            cboFuserPlayerType.Items.Clear();

            var playerTypeItems = new List<ComboBoxItem>();

            foreach (PlayerType type in Enum.GetValues(typeof(PlayerType)))
            {
                if (type != PlayerType.Default)
                {
                    var displayName = type == PlayerType.AIRaider ?
                        "Raider/Rogue/Guard" :
                        type.GetDescription();
                    var item = new ComboBoxItem
                    {
                        Content = displayName,
                        Tag = type.ToString()
                    };
                    playerTypeItems.Add(item);
                }
            }

            playerTypeItems.Add(new ComboBoxItem { Content = "Aimbot Locked", Tag = "AimbotLocked" });
            playerTypeItems.Add(new ComboBoxItem { Content = "Focused", Tag = "Focused" });
            playerTypeItems.Sort((x, y) => string.Compare(x.Content.ToString(), y.Content.ToString()));

            foreach (var item in playerTypeItems)
            {
                cboFuserPlayerType.Items.Add(item);
            }

            ccbFuserPlayerInformation.Items.Clear();

            foreach (var info in _availableFuserPlayerInformation)
            {
                ccbFuserPlayerInformation.Items.Add(new CheckComboBoxItem { Content = info });
            }

            if (cboFuserPlayerType.Items.Count > 0)
            {
                cboFuserPlayerType.SelectedIndex = 0;
                _currentFuserPlayerType = ((ComboBoxItem)cboFuserPlayerType.SelectedItem).Tag.ToString();
                LoadFuserPlayerTypeSettings(_currentFuserPlayerType);
            }
        }

        private void LoadFuserPlayerTypeSettings(string playerType)
        {
            _isLoadingFuserPlayerSettings = true;

            try
            {
                var settings = Config.ESP.PlayerTypeESPSettings.GetSettings(playerType);

                ccbFuserPlayerInformation.SelectedItems.Clear();

                chkHighAlert.IsChecked = settings.HighAlert;
                chkImportantIndicators.IsChecked = settings.ImportantIndicator;
                chkShowImportantPlayerLoot.IsChecked = settings.ShowImportantLoot;
                sldrPlayerTypeRenderDistance.Value = settings.RenderDistance;

                sldrMinimumKD.Value = settings.MinKD;

                foreach (CheckComboBoxItem item in ccbFuserPlayerInformation.Items)
                {
                    var info = item.Content.ToString();
                    item.IsSelected = settings.Information.Contains(info);
                }

                foreach (ComboBoxItem item in cboPlayerRenderMode.Items)
                {
                    if ((int)settings.RenderMode == cboPlayerRenderMode.Items.IndexOf(item))
                    {
                        cboPlayerRenderMode.SelectedItem = item;
                        break;
                    }
                }
            }
            finally
            {
                _isLoadingFuserPlayerSettings = false;
            }

            UpdatePlayerInformationControlsVisibility();
        }

        private void UpdatePlayerInformationControlsVisibility()
        {
            if (_isLoadingFuserPlayerSettings)
                return;

            kdSettings.Visibility = Visibility.Collapsed;

            var showKD = false;
            foreach (CheckComboBoxItem item in ccbFuserPlayerInformation.SelectedItems)
            {
                var info = item.Content.ToString();
                if (info == "KD")
                {
                    showKD = true;
                    break;
                }
            }

            if (showKD)
                kdSettings.Visibility = Visibility.Visible;
        }

        private void SaveFuserPlayerTypeSettings(string playerType)
        {
            if (_isLoadingFuserPlayerSettings)
                return;

            var settings = Config.ESP.PlayerTypeESPSettings.GetSettings(playerType);
            settings.Information.Clear();
            settings.HighAlert = chkHighAlert.IsChecked == true;
            settings.ImportantIndicator = chkImportantIndicators.IsChecked == true;
            settings.ShowImportantLoot = chkShowImportantPlayerLoot.IsChecked == true;
            settings.RenderDistance = (int)sldrPlayerTypeRenderDistance.Value;
            settings.MinKD = (float)sldrMinimumKD.Value;

            foreach (CheckComboBoxItem item in ccbFuserPlayerInformation.SelectedItems)
            {
                settings.Information.Add(item.Content.ToString());
            }

            settings.RenderMode = (ESPPlayerRenderMode)cboPlayerRenderMode.SelectedIndex;

            Config.Save();
            XMLogging.WriteLine($"Saved ESP player type settings for {playerType}");
            //PlayerPreviewControl.RefreshESPPreview();
        }

        private void InitializeFuserEntityTypeSettings()
        {
            if (Config.ESP.EntityTypeESPSettings == null)
                Config.ESP.EntityTypeESPSettings = new EntityTypeSettingsESPConfig();

            Config.ESP.EntityTypeESPSettings.InitializeDefaults();
            Config.Save();
            cboFuserEntityType.Items.Clear();

            var entityTypeItems = new List<ComboBoxItem>
            {
                new ComboBoxItem { Content = "Static Container", Tag = "StaticContainer" },
                new ComboBoxItem { Content = "Corpse", Tag = "Corpse" },
                new ComboBoxItem { Content = "Regular Loot", Tag = "RegularLoot" },
                new ComboBoxItem { Content = "Important Loot", Tag = "ImportantLoot" },
                new ComboBoxItem { Content = "Quest Item", Tag = "QuestItem" },
                new ComboBoxItem { Content = "Quest Zone", Tag = "QuestZone" },
                new ComboBoxItem { Content = "Switch", Tag = "Switch" },
                new ComboBoxItem { Content = "Transit", Tag = "Transit" },
                new ComboBoxItem { Content = "Exfil", Tag = "Exfil" },
                new ComboBoxItem { Content = "Door", Tag = "Door" },
                new ComboBoxItem { Content = "Grenade", Tag = "Grenade" },
                new ComboBoxItem { Content = "Tripwire", Tag = "Tripwire" },
                new ComboBoxItem { Content = "Mine", Tag = "Mine" },
                new ComboBoxItem { Content = "Mortar Projectile", Tag = "MortarProjectile" },
                new ComboBoxItem { Content = "Airdrop", Tag = "Airdrop" },
                new ComboBoxItem { Content = "BTR", Tag = "BTR" }
            };

            entityTypeItems.Sort((x, y) => string.Compare(x.Content.ToString(), y.Content.ToString()));

            foreach (var item in entityTypeItems)
            {
                cboFuserEntityType.Items.Add(item);
            }

            ccbFuserEntityInformation.Items.Clear();

            foreach (var info in _availableFuserEntityInformation)
            {
                ccbFuserEntityInformation.Items.Add(new CheckComboBoxItem { Content = info });
            }

            if (cboFuserEntityType.Items.Count > 0)
            {
                cboFuserEntityType.SelectedIndex = 0;
                _currentFuserEntityType = ((ComboBoxItem)cboFuserEntityType.SelectedItem).Tag.ToString();
                LoadFuserEntityTypeSettings(_currentFuserEntityType);
            }
        }

        private void LoadFuserEntityTypeSettings(string entityType)
        {
            _isLoadingFuserEntitySettings = true;

            try
            {
                var settings = Config.ESP.EntityTypeESPSettings.GetSettings(entityType);

                sldrEntityTypeRenderDistance.Value = settings.RenderDistance;

                ccbFuserEntityInformation.SelectedItems.Clear();

                foreach (CheckComboBoxItem item in ccbFuserEntityInformation.Items)
                {
                    var info = item.Content.ToString();

                    if (settings.Information.Contains(info))
                        item.IsSelected = true;
                    else
                        item.IsSelected = false;
                }

                switch (entityType)
                {
                    case "Corpse":
                        chkShowImportantCorpseLoot.IsChecked = settings.ShowImportantLoot;
                        break;
                    case "Door":
                        chkShowLockedDoors.IsChecked = settings.ShowLockedDoors;
                        chkShowUnlockedDoors.IsChecked = settings.ShowUnlockedDoors;
                        break;
                    case "Tripwire":
                        chkShowTripwireLine.IsChecked = settings.ShowTripwireLine;
                        break;
                    case "Grenade":
                        chkShowGrenadeRadius.IsChecked = settings.ShowRadius;
                        chkShowGrenadeTrail.IsChecked = settings.ShowGrenadeTrail;
                        sldrTrailDuration.Value = settings.TrailDuration;
                        sldrMinTrailDistance.Value = settings.MinTrailDistance;
                        break;
                }

                foreach (ComboBoxItem item in cboEntityRenderMode.Items)
                {
                    if (item.Content.ToString() == settings.RenderMode.ToString())
                    {
                        cboEntityRenderMode.SelectedItem = item;
                        break;
                    }
                }
            }
            finally
            {
                _isLoadingFuserEntitySettings = false;
            }

            corpseSettings.Visibility = Visibility.Collapsed;
            doorSettings.Visibility = Visibility.Collapsed;
            tripwireSettings.Visibility = Visibility.Collapsed;
            grenadeSettings.Visibility = Visibility.Collapsed;

            switch (_currentFuserEntityType)
            {
                case "Corpse":
                    corpseSettings.Visibility = Visibility.Visible;
                    break;
                case "Door":
                    doorSettings.Visibility = Visibility.Visible;
                    break;
                case "Tripwire":
                    tripwireSettings.Visibility = Visibility.Visible;
                    break;
                case "Grenade":
                    grenadeSettings.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SaveFuserEntityTypeSettings(string entityType)
        {
            if (_isLoadingFuserEntitySettings)
                return;

            var settings = Config.ESP.EntityTypeESPSettings.GetSettings(entityType);
            settings.RenderDistance = (int)sldrEntityTypeRenderDistance.Value;
            settings.Information.Clear();

            foreach (CheckComboBoxItem item in ccbFuserEntityInformation.SelectedItems)
            {
                settings.Information.Add(item.Content.ToString());
            }

            if (cboEntityRenderMode.SelectedItem is ComboBoxItem selectedItem)
            {
                var renderModeText = selectedItem.Content.ToString();
                if (Enum.TryParse<EntityRenderMode>(renderModeText, out var renderMode))
                    settings.RenderMode = renderMode;
            }

            switch (entityType)
            {
                case "Corpse":
                    settings.ShowImportantLoot = chkShowImportantCorpseLoot.IsChecked == true;
                    break;
                case "Door":
                    settings.ShowLockedDoors = chkShowLockedDoors.IsChecked == true;
                    settings.ShowUnlockedDoors = chkShowUnlockedDoors.IsChecked == true;
                    break;
                case "Tripwire":
                    settings.ShowTripwireLine = chkShowTripwireLine.IsChecked == true;
                    break;
                case "Grenade":
                    settings.ShowRadius = chkShowGrenadeRadius.IsChecked == true;
                    settings.ShowGrenadeTrail = chkShowGrenadeTrail.IsChecked == true;
                    break;
            }

            Config.Save();
            XMLogging.WriteLine($"Saved ESP entity type settings for {entityType}");
            //PlayerPreviewControl.RefreshESPPreview();
        }

        private void InitializeFuserOptions()
        {
            _isLoadingFuserOptionSettings = true;

            try
            {
                ccbWidgets.Items.Clear();
                foreach (var widget in _availableWidgets)
                {
                    ccbWidgets.Items.Add(new CheckComboBoxItem { Content = widget });
                }

                ccbESPOptions.Items.Clear();
                foreach (var option in _availableFuserOptions)
                {
                    ccbESPOptions.Items.Add(new CheckComboBoxItem { Content = option });
                }

                UpdateWidgetOptionSelections();
                UpdateESPOptionsSelections();
            }
            finally
            {
                _isLoadingFuserOptionSettings = false;
            }
        }

        private void UpdateWidgetOptionSelections()
        {
            var optionsToUpdate = new Dictionary<string, bool>
            {
                ["Quest Info Widget"] = Config.ESP.ShowQuestInfoWidget,
                ["Hotkey Info Widget"] = Config.ESP.ShowHotkeyInfoWidget
            };

            foreach (CheckComboBoxItem item in ccbWidgets.Items)
            {
                var content = item.Content.ToString();

                if (optionsToUpdate.TryGetValue(content, out bool shouldBeSelected))
                    item.IsSelected = shouldBeSelected;
            }
        }

        private void UpdateESPOptionsSelections()
        {
            var cfg = Config.ESP;
            ccbESPOptions.SelectedItems.Clear();

            foreach (CheckComboBoxItem item in ccbESPOptions.Items)
            {
                var content = item.Content.ToString();

                var isSelected = content switch
                {
                    "Fireport Aim" => cfg.ShowFireportAim,
                    "Aimbot FOV" => cfg.ShowAimFOV,
                    "Raid Stats" => cfg.ShowRaidStats,
                    "KillFeed" => cfg.ShowKillFeed,
                    "Aimbot Lock" => cfg.ShowAimLock,
                    "Status Text" => cfg.ShowStatusText,
                    "FPS" => cfg.ShowFPS,
                    "Energy/Hydration Bar" => cfg.EnergyHydrationBar,
                    "Magazine Info" => cfg.ShowMagazine,
                    "Closest Player" => cfg.ShowClosestPlayer,
                    "Top Loot" => cfg.ShowTopLoot,
                    _ => false
                };

                item.IsSelected = isSelected;
            }
        }

        public void UpdateSpecificWidgetOption(string widgetName, bool isSelected)
        {
            if (_isLoadingFuserOptionSettings)
                return;

            foreach (CheckComboBoxItem item in ccbWidgets.Items)
            {
                if (item.Content.ToString() == widgetName)
                {
                    item.IsSelected = isSelected;
                    break;
                }
            }

            Config.Save();
            XMLogging.WriteLine($"Updated widget option: {widgetName} = {isSelected}");
        }

        /// <summary>
        /// Scales all ESP font sizes based on the current font scale value
        /// </summary>
        private void ScaleESPFonts()
        {
            var fontScale = Config.ESP.FontScale;

            SKPaints.TextUSECESP.TextSize = 12f * fontScale;
            SKPaints.TextBEARESP.TextSize = 12f * fontScale;
            SKPaints.TextScavESP.TextSize = 12f * fontScale;
            SKPaints.TextFriendlyESP.TextSize = 12f * fontScale;
            SKPaints.TextPlayerScavESP.TextSize = 12f * fontScale;
            SKPaints.TextBossESP.TextSize = 12f * fontScale;
            SKPaints.TextRaiderESP.TextSize = 12f * fontScale;
            SKPaints.TextSpecialESP.TextSize = 12f * fontScale;
            SKPaints.TextStreamerESP.TextSize = 12f * fontScale;
            SKPaints.TextAimbotLockedESP.TextSize = 12f * fontScale;
            SKPaints.TextFocusedESP.TextSize = 12f * fontScale;
            SKPaints.TextLootESP.TextSize = 12f * fontScale;
            SKPaints.TextCorpseESP.TextSize = 12f * fontScale;
            SKPaints.TextImpLootESP.TextSize = 12f * fontScale;
            SKPaints.TextContainerLootESP.TextSize = 11f * fontScale;
            SKPaints.TextMedsESP.TextSize = 12f * fontScale;
            SKPaints.TextFoodESP.TextSize = 12f * fontScale;
            SKPaints.TextWeaponsESP.TextSize = 12f * fontScale;
            SKPaints.TextBackpackESP.TextSize = 12f * fontScale;
            SKPaints.TextQuestItemESP.TextSize = 12f * fontScale;
            SKPaints.TextAirdropESP.TextSize = 12f * fontScale;
            SKPaints.TextWishlistItemESP.TextSize = 12f * fontScale;
            SKPaints.TextQuestHelperESP.TextSize = 12f * fontScale;
            SKPaints.TextExfilOpenESP.TextSize = 12f * fontScale;
            SKPaints.TextExfilPendingESP.TextSize = 12f * fontScale;
            SKPaints.TextExfilClosedESP.TextSize = 12f * fontScale;
            SKPaints.TextExfilInactiveESP.TextSize = 12f * fontScale;
            SKPaints.TextExfilTransitESP.TextSize = 12f * fontScale;
            SKPaints.TextMagazineESP.TextSize = 42f * fontScale;
            SKPaints.TextMagazineInfoESP.TextSize = 16f * fontScale;
            SKPaints.TextBasicESP.TextSize = 12f * fontScale;
            SKPaints.TextBasicESPLeftAligned.TextSize = 12f * fontScale;
            SKPaints.TextStatusSmallEsp.TextSize = 13f * fontScale;
            SKPaints.TextExplosiveESP.TextSize = 13f * fontScale;
            SKPaints.TextSwitchesESP.TextSize = 12f * fontScale;
            SKPaints.TextDoorOpenESP.TextSize = 12f * fontScale;
            SKPaints.TextDoorShutESP.TextSize = 12f * fontScale;
            SKPaints.TextDoorLockedESP.TextSize = 12f * fontScale;
            SKPaints.TextDoorInteractingESP.TextSize = 12f * fontScale;
            SKPaints.TextDoorBreachingESP.TextSize = 12f * fontScale;
            SKPaints.TextPulsingAsteriskESP.TextSize = 18f * fontScale;
            SKPaints.TextPulsingAsteriskOutlineESP.TextSize = 18f * fontScale;
            SKPaints.TextESPFPS.TextSize = 12f * fontScale;
            SKPaints.TextESPRaidStats.TextSize = 12f * fontScale;
            SKPaints.TextESPStatusText.TextSize = 13f * fontScale;
            SKPaints.TextESPClosestPlayer.TextSize = 13f * fontScale;
            SKPaints.TextESPTopLoot.TextSize = 13f * fontScale;
            SKPaints.TextEnergyHydrationBarESP.TextSize = 12f * fontScale;
            SKPaints.TextEnergyHydrationBarOutlineESP.TextSize = 12f * fontScale;
            SKPaints.TextEnergyHydrationBarOutlineESP.StrokeWidth = 1.5f * fontScale;

            var espWindow = ESPForm.Window;
            espWindow?.ESPQuestInfo?.SetScaleFactor(fontScale);
            espWindow?.ESPHotkeyInfo?.SetScaleFactor(fontScale);
            espWindow?.OnRenderContextChanged();
        }

        /// <summary>
        /// Scales all ESP line stroke widths based on the current line scale value
        /// </summary>
        private void ScaleESPLines()
        {
            var lineScale = Config.ESP.LineScale;

            SKPaints.PaintVisible.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintUSECESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintBEARESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintScavESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintFriendlyESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintPlayerScavESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintBossESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintRaiderESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintSpecialESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintStreamerESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintAimbotLockedESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintFocusedESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintBasicESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintHighAlertAimlineESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintHighAlertBorderESP.StrokeWidth = 3f * lineScale;
            SKPaints.PaintLootESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintCorpseESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintImpLootESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintContainerLootESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintMedsESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintFoodESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintWeaponsESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintBackpackESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintQuestItemESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintAirdropESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintWishlistItemESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintQuestHelperESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintExplosiveESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintExplosiveRadiusESP.StrokeWidth = 1.5f * lineScale;
            SKPaints.PaintESPHealthBar.StrokeWidth = 1f * lineScale;
            SKPaints.PaintESPHealthBarBg.StrokeWidth = 1f * lineScale;
            SKPaints.PaintESPHealthBarBorder.StrokeWidth = 1f * lineScale;
            SKPaints.PaintSwitchESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintExfilTransitESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintExfilOpenESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintExfilPendingESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintExfilClosedESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintDoorOpenESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintDoorShutESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintDoorLockedESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintDoorInteractingESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintDoorBreachingESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintEnergyFillESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintHydrationFillESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintEnergyHydrationBackgroundESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintFireportAimESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintAimbotFOVESP.StrokeWidth = 1f * lineScale;
            SKPaints.PaintAimbotLockedLineESP.StrokeWidth = 1f * lineScale;
        }

        /// <summary>
        /// Scales all ESP line stroke widths based on the current line scale value
        /// </summary>
        private void ScaleMiniRadar()
        {
            var newScale = Config.ESP.MiniRadar.Scale;

            #region Paints
            SKPaints.PaintMiniLocalPlayer.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniTeammate.StrokeWidth = 3 * newScale; ;
            SKPaints.PaintMiniUSEC.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniBEAR.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniSpecial.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniStreamer.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniAimbotLocked.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniScav.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniRaider.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniBoss.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniFocused.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniPScav.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniCorpse.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniAirdrop.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniMeds.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniFood.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniWeapons.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniBackpacks.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniQuestItem.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniWishlistItem.StrokeWidth = 3 * newScale;
            SKPaints.MiniQuestHelperPaint.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniLoot.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniImportantLoot.StrokeWidth = 3 * newScale;
            SKPaints.PaintMiniContainerLoot.StrokeWidth = 3 * newScale;
            #endregion
        }

        /// <summary>
        /// Scales all ESP line stroke widths based on the current line scale value
        /// </summary>
        private void ScaleESPCrosshair()
        {
            var scale = Config.ESP.Crosshair.Scale;

            SKPaints.PaintCrosshairESP.StrokeWidth = 1.75f * scale;
            SKPaints.PaintCrosshairESPDot.StrokeWidth = 1.75f * scale;
        }

        private void SavePlayerTypeSettings()
        {
            if (!string.IsNullOrEmpty(_currentFuserPlayerType) && !_isLoadingFuserPlayerSettings)
                SaveFuserPlayerTypeSettings(_currentFuserPlayerType);
        }

        private void SaveEntityTypeSettings()
        {
            if (!string.IsNullOrEmpty(_currentFuserEntityType) && !_isLoadingFuserEntitySettings)
                SaveFuserEntityTypeSettings(_currentFuserEntityType);
        }

        private bool IsOptionSelected(string option)
        {
            foreach (CheckComboBoxItem item in ccbESPOptions.SelectedItems)
            {
                if (item.Content.ToString() == option)
                    return true;
            }

            return false;
        }

        private void ToggleMiniRadarControls()
        {
            var enabled = Config.ESP.MiniRadar.Enabled;

            chkMiniRadarLoot.IsEnabled = enabled;
            sldrFuserMiniRadarScale.IsEnabled = enabled;
        }
        #endregion

        #region Events
        private void FuserCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is string tag)
            {
                var value = cb.IsChecked == true;
                XMLogging.WriteLine($"[Checkbox] {cb.Name} changed to {value}");
                switch (tag)
                {
                    case "AutoFullscreen":
                        Config.ESP.AutoFullscreen = value;
                        break;
                    case "CrosshairEnabled":
                        Config.ESP.Crosshair.Enabled = value;
                        sldrFuserCrosshairScale.IsEnabled = value;
                        break;
                    case "MiniRadarEnabled":
                        Config.ESP.MiniRadar.Enabled = value;
                        ToggleMiniRadarControls();
                        break;
                    case "MiniRadarLoot":
                        Config.ESP.MiniRadar.ShowLoot = value;
                        break;
                    case "HighAlertIndicator":
                    case "ImportantIndicators":
                    case "ShowImportantPlayerLoot":
                        SavePlayerTypeSettings(); break;
                    case "ShowImportantCorpseLoot":
                    case "ShowLockedDoors":
                    case "ShowUnlockedDoors":
                    case "ShowTripwireLine":
                    case "ShowGrenadeRadius":
                        SaveEntityTypeSettings();
                        break;
                }

                Config.Save();
                XMLogging.WriteLine("Saved Config");
            }
        }

        private void FuserSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is TextValueSlider slider && slider.Tag is string tag)
            {
                var intValue = (int)e.NewValue;
                var floatValue = (float)e.NewValue;
                switch (tag)
                {
                    case "PlayerTypeRenderDistance": SavePlayerTypeSettings(); break;
                    case "EntityTypeRenderDistance": SaveEntityTypeSettings(); break;
                    case "FuserFontScale":
                        Config.ESP.FontScale = floatValue;
                        ScaleESPFonts();
                        break;
                    case "FuserLineScale":
                        Config.ESP.LineScale = floatValue;
                        ScaleESPLines();
                        break;
                    case "TrailDuration":
                        Config.ESP.EntityTypeESPSettings.GetSettings("Grenade").TrailDuration = floatValue;
                        break;
                    case "MinTrailDistance":
                        Config.ESP.EntityTypeESPSettings.GetSettings("Grenade").MinTrailDistance = floatValue;
                        break;
                    case "MinimumKD":
                        SavePlayerTypeSettings();
                        break;
                    case "FuserMiniRadarScale":
                        Config.ESP.MiniRadar.Scale = floatValue;
                        ScaleMiniRadar();
                        break;
                    case "FuserCrosshairScale":
                        Config.ESP.Crosshair.Scale = floatValue;
                        ScaleESPCrosshair();
                        break;
                }

                Config.Save();
            }
        }

        private void FuserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cbo && cbo.Tag is string tag)
            {
                switch (tag)
                {
                    case "PlayerRenderMode":
                        SavePlayerTypeSettings();
                        break;
                    case "EntityRenderMode":
                        SaveEntityTypeSettings();
                        break;
                    case "CrosshairType":
                        Config.ESP.Crosshair.Type = cbo.SelectedIndex;
                        break;
                }

                Config.Save();
                XMLogging.WriteLine("[ComboBox] Selection changed and config saved.");
            }
        }

        private void nudFPSCap_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (e.Info is double value)
            {
                var fpsValue = (int)value;
                Config.ESP.FPSCap = fpsValue;
                Config.Save();

                ESPForm.Window?.UpdateRenderTimerInterval(fpsValue);

                XMLogging.WriteLine($"[FPS Cap] Changed to {fpsValue}");
            }
        }

        public void btnStartESP_Click(object sender, RoutedEventArgs e)
        {
            btnStartESP.Content = "Running...";
            btnStartESP.IsEnabled = false;

            var t = new Thread(() =>
            {
                try
                {
                    ESPForm.ShowESP = true;
                    System.Windows.Forms.Application.Run(new ESPForm());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ESP Critical Runtime Error!\n{ex.Message}\n\n{ex.StackTrace}");
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnStartESP.Content = "Start ESP";
                        btnStartESP.IsEnabled = true;
                    });
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void cboFuserPlayerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboFuserPlayerType.SelectedItem is ComboBoxItem item)
            {
                SavePlayerTypeSettings();

                _currentFuserPlayerType = item.Tag.ToString();
                LoadFuserPlayerTypeSettings(_currentFuserPlayerType);
            }
        }

        private void cboFuserEntityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboFuserEntityType.SelectedItem is ComboBoxItem item)
            {
                SaveEntityTypeSettings();

                _currentFuserEntityType = item.Tag.ToString();
                LoadFuserEntityTypeSettings(_currentFuserEntityType);
            }
        }

        private void espPlayerInfoCheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SavePlayerTypeSettings();
            UpdatePlayerInformationControlsVisibility();
        }

        private void espEntityInfoCheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveEntityTypeSettings();
        }

        private void espOptionsCheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingFuserOptionSettings)
                return;

            Config.ESP.ShowFireportAim = IsOptionSelected("Fireport Aim");
            Config.ESP.ShowAimFOV = IsOptionSelected("Aimbot FOV");
            Config.ESP.ShowRaidStats = IsOptionSelected("Raid Stats");
            Config.ESP.ShowKillFeed = IsOptionSelected("KillFeed");
            Config.ESP.ShowAimLock = IsOptionSelected("Aimbot Lock");
            Config.ESP.ShowStatusText = IsOptionSelected("Status Text");
            Config.ESP.ShowFPS = IsOptionSelected("FPS");
            Config.ESP.EnergyHydrationBar = IsOptionSelected("Energy/Hydration Bar");
            Config.ESP.ShowMagazine = IsOptionSelected("Magazine Info");
            Config.ESP.ShowClosestPlayer = IsOptionSelected("Closest Player");
            Config.ESP.ShowTopLoot = IsOptionSelected("Top Loot");

            Config.Save();
            XMLogging.WriteLine("Saved ESP option settings");
        }

        private void widgetsCheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingFuserOptionSettings)
                return;

            foreach (CheckComboBoxItem item in ccbWidgets.Items)
            {
                var widgetOption = item.Content.ToString();
                var isSelected = item.IsSelected;

                switch (widgetOption)
                {
                    case "Quest Info Widget":
                        Config.ESP.ShowQuestInfoWidget = isSelected;
                        break;
                    case "Hotkey Info Widget":
                        Config.ESP.ShowHotkeyInfoWidget = isSelected;
                        break;
                }
            }

            Config.Save();
            XMLogging.WriteLine("Saved widget settings");
        }
        /// <summary>
        /// Load available monitors into the ComboBox
        /// </summary>
        private void LoadMonitors()
        {
            try
            {
                var monitors = MonitorInfo.GetAllMonitors();
                cmbTargetMonitor.ItemsSource = monitors;
                cmbTargetMonitor.DisplayMemberPath = "DisplayName";
                cmbTargetMonitor.SelectedValuePath = "Index";

                // Select configured monitor (default to index 1 if available, otherwise primary)
                var targetIndex = Config.ESP.EspTargetScreen;
                if (targetIndex >= 0 && targetIndex < monitors.Count)
                    cmbTargetMonitor.SelectedIndex = targetIndex;
                else
                    cmbTargetMonitor.SelectedIndex = monitors.Count > 1 ? 1 : 0; // Default to second monitor if available

                XMLogging.WriteLine($"[ESP] Loaded {monitors.Count} monitor(s), selected monitor {cmbTargetMonitor.SelectedIndex}");
            }
            catch (Exception ex)
            {
                XMLogging.WriteLine($"[ESP] Error loading monitors: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle monitor selection changed
        /// </summary>
        private void cmbTargetMonitor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTargetMonitor.SelectedItem is MonitorInfo monitor)
            {
                Config.ESP.EspTargetScreen = monitor.Index;
                Config.Save();
                XMLogging.WriteLine($"[ESP] Target monitor changed to {monitor.DisplayName}");

                // Log for debugging
                XMLogging.WriteLine($"[ESP] Monitor resolution: {monitor.Width}x{monitor.Height} @ ({monitor.Left}, {monitor.Top})");
            }
        }
        #endregion
        #endregion
        #endregion
    }
}