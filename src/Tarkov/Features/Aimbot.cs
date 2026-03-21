using eft_dma_radar.Common.DMA.Features;
using eft_dma_radar.Common.Misc;
using eft_dma_radar.Common.Unity;
using eft_dma_radar.Misc.Makcu;
using eft_dma_radar.Tarkov.EFTPlayer;
using eft_dma_radar.UI.Misc;
using static eft_dma_radar.Tarkov.EFTPlayer.Player;

namespace eft_dma_radar.Tarkov.Features
{
    /// <summary>
    /// Makcu-based aimbot: projects the best enemy's bone to screen and
    /// physically moves the mouse via the Makcu hardware device.
    /// </summary>
    internal sealed class Aimbot : IFeature
    {
        /// <summary>Set by the hotkey system when the engage key is held.</summary>
        public static volatile bool Engaged;

        // ── Singleton ────────────────────────────────────────────────────────

        private static readonly Aimbot _instance;

        static Aimbot()
        {
            _instance = new Aimbot();
            IFeature.Register(_instance);
        }

        public static Aimbot Instance => _instance;

        // ── State ─────────────────────────────────────────────────────────────

        private volatile bool _active;
        private Player _lockedTarget;

        // ── IFeature ──────────────────────────────────────────────────────────

        public bool CanRun => _active;

        void IFeature.OnApply() { }

        void IFeature.OnGameStart() => _active = true;

        void IFeature.OnGameStop()
        {
            _active = false;
            Engaged = false;
            ClearLockedTarget();
        }

        void IFeature.OnRaidStart() { }

        void IFeature.OnRaidEnd() => ClearLockedTarget();

        // ── Constructor / Thread ──────────────────────────────────────────────

        private Aimbot()
        {
            new Thread(Worker)
            {
                IsBackground = true,
                Name = "Aimbot",
                Priority = ThreadPriority.AboveNormal,
            }.Start();
        }

        private void Worker()
        {
            XMLogging.WriteLine("[Aimbot] Thread started.");

            while (true)
            {
                try
                {
                    if (!_active)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var config = GetConfig();
                    if (config is null || !config.Enabled)
                    {
                        ClearLockedTarget();
                        Thread.Sleep(100);
                        continue;
                    }

                    if (!MakcuManager.IsConnected)
                    {
                        ClearLockedTarget();
                        Thread.Sleep(250);
                        continue;
                    }

                    if (!Engaged)
                    {
                        ClearLockedTarget();
                        Thread.Sleep(10);
                        continue;
                    }

                    var localPlayer = Memory.LocalPlayer;
                    if (localPlayer is null)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var players = Memory.Players;
                    if (players is null)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var target = GetBestTarget(players, config);

                    if (target is null)
                    {
                        ClearLockedTarget();
                        Thread.Sleep(10);
                        continue;
                    }

                    SetLockedTarget(target);
                    AimAt(target, config);

                    Thread.Sleep(5); // ~200 Hz
                }
                catch (Exception ex)
                {
                    XMLogging.WriteLine($"[Aimbot] Error: {ex.Message}");
                    Thread.Sleep(250);
                }
            }
        }

        // ── Target acquisition ────────────────────────────────────────────────

        private Player GetBestTarget(IReadOnlyCollection<Player> players, AimbotConfig cfg)
        {
            // Keep the current locked target if it's still valid and in FOV
            if (_lockedTarget is not null)
            {
                if (_lockedTarget.IsAlive && _lockedTarget.IsActive && !_lockedTarget.IsFriendly &&
                    (cfg.AimAI || !_lockedTarget.IsAI) &&
                    IsInFov(_lockedTarget, cfg))
                {
                    return _lockedTarget;
                }
                ClearLockedTarget();
            }

            Player best = null;
            float bestFov = float.MaxValue;
            float fovLimit = FovToPixels(cfg.FovDegrees);

            foreach (var player in players)
            {
                if (player is LocalPlayer) continue;
                if (!player.IsAlive || !player.IsActive) continue;
                if (player.IsFriendly) continue;
                if (!cfg.AimAI && player.IsAI) continue;

                var skeleton = player.Skeleton;
                if (skeleton is null) continue;
                if (!skeleton.Bones.TryGetValue(cfg.AimBone, out var bone)) continue;

                var pos = bone.Position;
                if (!CameraManagerBase.WorldToScreen(ref pos, out var scrPos, onScreenCheck: true))
                    continue;

                float fov = CameraManagerBase.GetFovMagnitude(scrPos);
                if (fov > fovLimit || fov >= bestFov) continue;

                bestFov = fov;
                best = player;
            }

            return best;
        }

        private static bool IsInFov(Player player, AimbotConfig cfg)
        {
            var skeleton = player.Skeleton;
            if (skeleton is null) return false;
            if (!skeleton.Bones.TryGetValue(cfg.AimBone, out var bone)) return false;

            var pos = bone.Position;
            if (!CameraManagerBase.WorldToScreen(ref pos, out var scrPos, onScreenCheck: true))
                return false;

            return CameraManagerBase.GetFovMagnitude(scrPos) <= FovToPixels(cfg.FovDegrees);
        }

        // ── Mouse movement ────────────────────────────────────────────────────

        private static void AimAt(Player target, AimbotConfig cfg)
        {
            try
            {
                var skeleton = target.Skeleton;
                if (skeleton is null) return;
                if (!skeleton.Bones.TryGetValue(cfg.AimBone, out var bone)) return;

                var pos = bone.Position;
                if (!CameraManagerBase.WorldToScreen(ref pos, out var scrPos)) return;

                var center = CameraManagerBase.ViewportCenter;
                float deltaX = scrPos.X - center.X;
                float deltaY = scrPos.Y - center.Y;

                // Skip if already inside the deadzone
                float dist = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
                if (dist < cfg.Deadzone) return;

                // Lerp from zero toward full delta each frame. alpha in (0,1]: lower = smoother.
                // lerp(0, delta, alpha) == delta * alpha
                float moveX = deltaX * cfg.AlphaX;
                float moveY = deltaY * cfg.AlphaY;

                // Add Gaussian noise (Box-Muller transform) to humanize movement
                if (cfg.GaussianNoise > 0f)
                {
                    float u1 = 1f - Random.Shared.NextSingle();
                    float u2 = Random.Shared.NextSingle();
                    float mag = cfg.GaussianNoise * MathF.Sqrt(-2f * MathF.Log(u1));
                    moveX += mag * MathF.Cos(2f * MathF.PI * u2);
                    moveY += mag * MathF.Sin(2f * MathF.PI * u2);
                }

                int dx = (int)MathF.Round(moveX);
                int dy = (int)MathF.Round(moveY);

                if (dx == 0 && dy == 0) return;

                MakcuManager.Device?.MouseMove(dx, dy);
            }
            catch
            {
                // Swallow per-frame aim errors
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a FOV angle (degrees from crosshair) to a pixel radius on screen.
        /// Uses half the viewport width as the reference for 90° horizontal FOV,
        /// which provides a rough but consistent pixel-based FOV circle.
        /// </summary>
        private static float FovToPixels(float degrees)
        {
            float halfWidth = CameraManagerBase.Viewport.Width / 2f;
            return halfWidth * MathF.Tan(degrees * (MathF.PI / 180f));
        }

        private static AimbotConfig GetConfig() =>
            (SharedProgram.Config as Config)?.Aimbot;

        private void ClearLockedTarget()
        {
            if (_lockedTarget is not null)
            {
                _lockedTarget.IsAimbotLocked = false;
                _lockedTarget = null;
            }
        }

        private void SetLockedTarget(Player target)
        {
            if (_lockedTarget == target) return;
            ClearLockedTarget();
            _lockedTarget = target;
            target.IsAimbotLocked = true;
        }
    }
}
