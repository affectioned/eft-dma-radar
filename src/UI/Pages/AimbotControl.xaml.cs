using eft_dma_radar.Misc.Makcu;
using eft_dma_radar.Tarkov.Features;
using eft_dma_radar.UI.Misc;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace eft_dma_radar.UI.Pages
{
    public partial class AimbotControl : UserControl
    {
        #region Fields
        private Point _dragStartPoint;
        private bool _loading;

        public event EventHandler CloseRequested;
        public event EventHandler BringToFrontRequested;
        public event EventHandler<PanelDragEventArgs> DragRequested;
        public event EventHandler<PanelResizeEventArgs> ResizeRequested;
        #endregion

        private static Config Config => SharedProgram.Config as Config;

        public AimbotControl()
        {
            _loading = true;
            InitializeComponent();
            Name = "AimbotControl";
            PopulateBoneCombo();
            _loading = false;
            btnCloseHeader.Click += btnCloseHeader_Click;
            DragHandle.MouseLeftButtonDown += DragHandle_MouseLeftButtonDown;
            Loaded += (_, _) => LoadSettings();
        }

        #region Load / Save

        private void LoadSettings()
        {
            _loading = true;
            try
            {
                var cfg = Config?.Aimbot;
                if (cfg is null) return;

                chkAutoConnect.IsChecked = cfg.AutoConnect;
                chkEnabled.IsChecked = cfg.Enabled;
                txtMakcuPort.Text = cfg.MakcuPort;
                nudFov.Value = cfg.FovDegrees;
                nudAlphaX.Value = cfg.AlphaX;
                nudAlphaY.Value = cfg.AlphaY;
                nudDeadzone.Value = cfg.Deadzone;
                nudGaussianNoise.Value = cfg.GaussianNoise;
                chkShowFovCircle.IsChecked = Config?.ESP?.ShowAimFOV ?? false;
                chkAimAI.IsChecked = cfg.AimAI;
                SelectBone(cfg.AimBone);
            }
            finally
            {
                _loading = false;
            }

            UpdateStatusLabel();
        }

        private void SaveSettings()
        {
            if (_loading) return;
            var cfg = Config?.Aimbot;
            if (cfg is null) return;

            cfg.AutoConnect = chkAutoConnect.IsChecked == true;
            cfg.Enabled = chkEnabled.IsChecked == true;
            cfg.MakcuPort = txtMakcuPort.Text?.Trim() ?? "COM3";
            cfg.FovDegrees = (float)nudFov.Value;
            cfg.AlphaX = (float)nudAlphaX.Value;
            cfg.AlphaY = (float)nudAlphaY.Value;
            cfg.Deadzone = (float)nudDeadzone.Value;
            cfg.GaussianNoise = (float)nudGaussianNoise.Value;
            cfg.AimAI = chkAimAI.IsChecked == true;

            if (cboAimBone.SelectedItem is BoneItem bi)
                cfg.AimBone = bi.Bone;

            Config?.Save();
        }

        #endregion

        #region Bone Combo

        private record BoneItem(eft_dma_radar.Common.Unity.Bones Bone, string Label)
        {
            public override string ToString() => Label;
        }

        private void PopulateBoneCombo()
        {
            cboAimBone.Items.Add(new BoneItem(eft_dma_radar.Common.Unity.Bones.HumanHead,   "Head"));
            cboAimBone.Items.Add(new BoneItem(eft_dma_radar.Common.Unity.Bones.HumanNeck,   "Neck"));
            cboAimBone.Items.Add(new BoneItem(eft_dma_radar.Common.Unity.Bones.HumanSpine3, "Thorax"));
            cboAimBone.Items.Add(new BoneItem(eft_dma_radar.Common.Unity.Bones.HumanSpine2, "Spine 2"));
            cboAimBone.Items.Add(new BoneItem(eft_dma_radar.Common.Unity.Bones.HumanSpine1, "Spine 1"));
            cboAimBone.Items.Add(new BoneItem(eft_dma_radar.Common.Unity.Bones.HumanPelvis, "Pelvis"));
            cboAimBone.SelectedIndex = 0;
        }

        private void SelectBone(eft_dma_radar.Common.Unity.Bones bone)
        {
            foreach (BoneItem item in cboAimBone.Items)
            {
                if (item.Bone == bone)
                {
                    cboAimBone.SelectedItem = item;
                    return;
                }
            }
            cboAimBone.SelectedIndex = 0;
        }

        #endregion

        #region Status Label

        public void UpdateStatusLabel()
        {
            Dispatcher.InvokeAsync(() =>
            {
                bool connected = MakcuManager.IsConnected;
                txtStatus.Text = connected ? "Connected" : "Disconnected";
                txtStatus.Foreground = connected
                    ? new SolidColorBrush(Colors.LimeGreen)
                    : new SolidColorBrush(Colors.Red);
            });
        }

        #endregion

        #region Event Handlers

        private void btnCloseHeader_Click(object sender, RoutedEventArgs e) =>
            CloseRequested?.Invoke(this, EventArgs.Empty);

        private void chkEnabled_Changed(object sender, RoutedEventArgs e) => SaveSettings();
        private void chkAutoConnect_Changed(object sender, RoutedEventArgs e) => SaveSettings();
        private void chkShowFovCircle_Changed(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            if (Config?.ESP is { } esp)
            {
                esp.ShowAimFOV = chkShowFovCircle.IsChecked == true;
                Config?.Save();
            }
        }

        private void chkAimAI_Changed(object sender, RoutedEventArgs e) => SaveSettings();
        private void Setting_Changed(object sender, HandyControl.Data.FunctionEventArgs<double> e) => SaveSettings();
        private void cboAimBone_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => SaveSettings();
        private void txtMakcuPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => SaveSettings();

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            var port = txtMakcuPort.Text?.Trim();
            if (string.IsNullOrEmpty(port)) return;

            try
            {
                MakcuManager.Connect(port);
                SaveSettings();
            }
            catch (Exception ex)
            {
                Common.Misc.XMLogging.WriteLine($"[AimbotControl] Connect failed: {ex.Message}");
            }
            finally
            {
                UpdateStatusLabel();
            }
        }

        #endregion

        #region Drag Handling

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
                var offset = e.GetPosition(this) - _dragStartPoint;
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
    }
}
