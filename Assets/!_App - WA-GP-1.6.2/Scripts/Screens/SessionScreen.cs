using System.Collections.Generic;
using BreathTower.App;
using BreathTower.Core;
using BreathTower.Data;
using BreathTower.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using static BreathTower.UI.Widgets;

namespace BreathTower.Screens
{
    /// <summary>
    /// The breathing session — core gameplay. Ports app/session.tsx including
    /// the phase timer, breathing-ring animation, crane sway, block drop, flash
    /// feedback, haptics and auto-finish.
    /// </summary>
    public sealed class SessionScreen : ScreenBase
    {
        private const float TowerAreaH = 320f;
        private const float BlockW = 110f;
        private const float BlockH = 20f;
        private const int MaxBlocks = 10;
        private const float RingSize = 170f;

        private static readonly (float x, float w, float h)[] Buildings =
        {
            (0,45,80),(48,32,105),(83,55,68),(140,38,95),(181,50,62),
            (233,35,98),(271,48,76),(321,42,110),(366,50,72),
        };
        private static readonly (float x, float y)[] Stars =
        {
            (25,10),(70,28),(130,14),(190,42),(250,20),(310,35),
            (370,12),(55,55),(175,60),(290,52),(340,62),(95,45),
        };

        // ── Session runtime state ───────────────────────────────────────────
        private Technique _technique;
        private Color _techniqueColor;
        private string _techniqueId;
        private string _sessionType;
        private int _targetCycles;
        private float _targetMs;

        private bool _active;
        private bool _started;
        private bool _finishing;
        private int _phaseIdx;
        private float _phaseMs;
        private float _totalMs;
        private int _cycles;
        private readonly List<BlockRecord> _blocks = new List<BlockRecord>();
        private string _sessionId;

        private float _craneTime;
        private float _flashOpacity;
        private bool _completionActive;
        private float _completionTime;

        // ── UI refs ─────────────────────────────────────────────────────────
        private VisualElement _flash, _completion, _ring, _ringGlow, _crane, _towerStack;
        private Label _phaseLabel, _timerNum, _timerUnit, _readyLabel, _cycleText;
        private VisualElement _controlsHost;
        private VisualElement _ringCenter;

        public override VisualElement Build()
        {
            _techniqueId = Args.Get("techniqueId", "4-7-8");
            _sessionType = Args.Get("sessionType", "cycles");
            int sessionValue = Args.GetInt("sessionValue", 20);
            _targetCycles = _sessionType == "cycles" ? sessionValue : int.MaxValue;
            _targetMs = _sessionType == "time" ? sessionValue * 60_000f : float.MaxValue;

            _technique = _techniqueId == "custom"
                ? Breathing.BuildCustom(
                    Args.GetInt("customInhale", 4), Args.GetInt("customHold", 0),
                    Args.GetInt("customExhale", 4), Args.GetInt("customHold2", 0))
                : Breathing.FindTechnique(_techniqueId);
            _techniqueColor = Theme.TechniqueColor(_techniqueId);
            _sessionId = SaveSystem.GenerateSessionId();

            var root = Scaffold.Screen(App.Assets.sessionLayout, out var content);
            content.Col();

            // Flash + completion overlays.
            _flash = Box().Inset0().Bg(Theme.TextPrimary.WithAlpha(0.12f)).NoPick();
            _flash.style.opacity = 0;
            _completion = Box().Inset0().Bg(Theme.Cyan.WithAlpha(0.08f)).NoPick();
            _completion.style.opacity = 0;

            BuildHeader(content);
            BuildTowerArea(content);
            BuildBreathArea(content);

            // Overlays last so they sit on top.
            content.Add(_completion);
            content.Add(_flash);
            return root;
        }

        private void BuildHeader(VisualElement parent)
        {
            var header = Row().AlignItems(Align.Center).Justify(Justify.SpaceBetween);
            header.PadH(Theme.SpaceMd); header.PadV(Theme.SpaceSm);

            var exit = Box().Size(36, 36).Radius(18).Bg(Theme.Surface).Center();
            var x = Text("×", Theme.FontMd, Theme.TextSecondary);
            x.pickingMode = PickingMode.Ignore;
            exit.Add(x);
            Widgets.MakeClickable(exit, EndEarly);
            header.Add(exit);

            var center = Box().Flex(1).AlignItems(Align.Center);
            center.Add(Text(_technique.Name, Theme.FontSm, Theme.TextSecondary, FontStyle.Bold));
            header.Add(center);

            var cycleTag = Row();
            cycleTag.style.alignItems = Align.FlexEnd;
            _cycleText = Text(CycleText(), Theme.FontLg, Theme.TextPrimary, FontStyle.Bold);
            cycleTag.Add(_cycleText);
            cycleTag.Add(Text(" cycles", Theme.FontXs, Theme.TextMuted));
            header.Add(cycleTag);

            parent.Add(header);
        }

        private void BuildTowerArea(VisualElement parent)
        {
            var area = Box().WidthPct(100).Overflow(true);
            area.style.height = TowerAreaH;
            area.style.position = Position.Relative;

            foreach (var (sx, sy) in Stars)
            {
                var star = Box().Absolute().Pos(sx, sy).Size(2, 2).Radius(1)
                    .Bg(Theme.TextPrimary.WithAlpha(0.6f)).NoPick();
                area.Add(star);
            }

            // Crane (shown once started).
            _crane = BuildCrane();
            _crane.style.display = DisplayStyle.None;
            area.Add(_crane);

            // Tower stack — grows from the bottom.
            _towerStack = Box().Absolute();
            _towerStack.style.left = 0; _towerStack.style.right = 0; _towerStack.style.bottom = 62;
            _towerStack.style.alignItems = Align.Center;
            _towerStack.style.flexDirection = FlexDirection.ColumnReverse;

            // City silhouette. Added BEFORE the tower so the (opaque) buildings
            // sit behind it — otherwise the taller buildings flanking the centre
            // clip the lower blocks and the base appears to taper to a point.
            var city = Box().Absolute().Height(60);
            city.style.left = 0; city.style.right = 0; city.style.bottom = 0;
            foreach (var (bx, bw, bh) in Buildings)
            {
                var b = Box().Absolute().Bg(Theme.CityBuilding);
                b.style.bottom = 0; b.style.left = bx; b.style.width = bw; b.style.height = bh;
                city.Add(b);
            }
            area.Add(city);

            // Tower paints in front of the skyline.
            area.Add(_towerStack);

            parent.Add(area);
        }

        private VisualElement BuildCrane()
        {
            var wrap = Box().Absolute();
            wrap.style.top = 8; wrap.style.left = Length.Percent(50);
            wrap.style.translate = new Translate(-65, 0, 0);
            wrap.style.width = 130; wrap.style.height = 80;

            var mast = Box().Absolute().Bg(Theme.Crane).Radius(4);
            mast.style.right = 10; mast.style.top = 20; mast.style.width = 8; mast.style.height = 60;
            wrap.Add(mast);

            var arm = Box().Absolute();
            arm.style.top = 20; arm.style.right = 10; arm.style.width = 110; arm.style.height = 60;

            var beam = Box().Absolute().Bg(Theme.Crane).Radius(3);
            beam.style.top = 0; beam.style.left = 0; beam.style.right = 0; beam.style.height = 6;
            arm.Add(beam);
            var cable = Box().Absolute().Bg(Theme.Crane);
            cable.style.top = 6; cable.style.left = 12; cable.style.width = 2; cable.style.height = 30;
            arm.Add(cable);
            var hang = Box().Absolute().Radius(3).Bg(_techniqueColor);
            hang.style.top = 36; hang.style.left = 3; hang.style.width = 20; hang.style.height = 14;
            arm.Add(hang);

            wrap.Add(arm);
            _craneArm = arm;
            return wrap;
        }

        private VisualElement _craneArm;

        private void BuildBreathArea(VisualElement parent)
        {
            var area = Box().Flex(1).AlignItems(Align.Center).Justify(Justify.SpaceAround).PadH(Theme.SpaceMd);

            _phaseLabel = Text("Ready", Theme.FontXl, Theme.PhaseColor("inhale"), FontStyle.Bold);
            _phaseLabel.style.letterSpacing = 1;
            area.Add(_phaseLabel);

            var ringContainer = Box().Center();
            ringContainer.style.position = Position.Relative;
            ringContainer.style.width = RingSize + 40;
            ringContainer.style.height = RingSize + 40;

            _ringGlow = Box().Absolute().Size(RingSize, RingSize).Radius(RingSize / 2).NoPick();
            _ringGlow.Border(20, Theme.PhaseColor("inhale"));
            _ringGlow.style.opacity = 0.25f;

            _ring = Box().Size(RingSize, RingSize).Radius(RingSize / 2).Center();
            _ring.Border(4, Theme.PhaseColor("inhale"));
            _ring.style.backgroundColor = Theme.TextPrimary.WithAlpha(0.04f);

            _ringCenter = Box().AlignItems(Align.Center);
            _readyLabel = Text("Tap Begin", Theme.FontMd, Theme.TextMuted, FontStyle.Bold);
            _timerNum = Text("4.0", Theme.FontHuge, Theme.PhaseColor("inhale"), FontStyle.Bold);
            _timerUnit = Text("sec", Theme.FontSm, Theme.TextMuted);
            _timerNum.style.display = DisplayStyle.None;
            _timerUnit.style.display = DisplayStyle.None;
            _ringCenter.Add(_readyLabel);
            _ringCenter.Add(_timerNum);
            _ringCenter.Add(_timerUnit);
            _ring.Add(_ringCenter);

            ringContainer.Add(_ringGlow);
            ringContainer.Add(_ring);
            area.Add(ringContainer);

            _controlsHost = Box().WidthPct(100).AlignItems(Align.Center);
            area.Add(_controlsHost);
            BuildBeginControl();

            // Initialize breathing scale.
            ApplyBreathScale(0.65f);
            parent.Add(area);
        }

        private void BuildBeginControl()
        {
            _controlsHost.Clear();
            var begin = GradientButton("Begin", StartSession);
            begin.style.width = Length.Percent(80);
            _controlsHost.Add(begin);
        }

        private void BuildEndControl()
        {
            _controlsHost.Clear();
            var end = Box().Radius(Theme.RadiusPill).Center();
            end.style.paddingTop = end.style.paddingBottom = Theme.SpaceSm;
            end.style.paddingLeft = end.style.paddingRight = Theme.SpaceXl;
            end.Border(1, Theme.TextPrimary.WithAlpha(0.2f));
            var l = Text("End Session", Theme.FontSm, Theme.TextSecondary, FontStyle.Bold);
            l.pickingMode = PickingMode.Ignore;
            end.Add(l);
            Widgets.MakeClickable(end, EndEarly);
            _controlsHost.Add(end);
        }

        // ── Control handlers ────────────────────────────────────────────────
        private void StartSession()
        {
            _active = true;
            _started = true;
            _phaseIdx = 0;
            _phaseMs = 0;
            _totalMs = 0;
            _cycles = 0;
            _finishing = false;
            _blocks.Clear();

            _crane.style.display = DisplayStyle.Flex;
            SetPhaseUi(_technique.Phases[0]);
            _readyLabel.style.display = DisplayStyle.None;
            _timerNum.style.display = DisplayStyle.Flex;
            _timerUnit.style.display = DisplayStyle.Flex;
            BuildEndControl();
        }

        private void EndEarly()
        {
            if (_finishing) return;
            // Before the session has started (the "Ready" state) the × simply
            // leaves the screen — there is no run to end yet, so it must not be
            // a dead button.
            if (!_started)
            {
                App.Back();
                return;
            }
            if (!_active) return;
            _finishing = true;
            Finish();
        }

        // ── Per-frame timer / animation ─────────────────────────────────────
        public override void Tick(float dt)
        {
            // Continuous crane + flash + completion animations.
            if (_started)
            {
                _craneTime += dt;
                float angle = 12f * Mathf.Sin(2f * Mathf.PI * _craneTime / 5f); // 5s sway period
                if (_craneArm != null) _craneArm.style.rotate = new Rotate(new Angle(angle));
            }

            if (_flashOpacity > 0f)
            {
                _flashOpacity = Mathf.Max(0f, _flashOpacity - dt / 0.45f);
                _flash.style.opacity = _flashOpacity * 0.7f;
            }

            if (_completionActive)
            {
                _completionTime += dt;
                float v = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(_completionTime * Mathf.PI / 0.8f));
                _completion.style.opacity = v * 0.6f;
                if (_completionTime > 2.4f) { _completionActive = false; _completion.style.opacity = 0; }
            }

            if (!_active) return;

            float tickMs = dt * 1000f;
            _phaseMs += tickMs;
            _totalMs += tickMs;

            var phases = _technique.Phases;
            var ph = phases[_phaseIdx];
            float phDurMs = ph.Duration * 1000f;

            float timeLeft = Mathf.Max(0f, (phDurMs - _phaseMs) / 1000f);
            _timerNum.text = timeLeft.ToString("0.0");

            // Drive the breathing ring scale analytically from phase progress.
            UpdateBreathScale(ph, Mathf.Clamp01(_phaseMs / phDurMs));

            if (_phaseMs >= phDurMs)
            {
                int nextIdx = (_phaseIdx + 1) % phases.Count;
                _phaseIdx = nextIdx;
                _phaseMs = 0;
                SetPhaseUi(phases[nextIdx]);

                if (nextIdx == 0)
                    OnCycleComplete();
            }

            if (_sessionType == "time" && _totalMs >= _targetMs && !_finishing)
            {
                _finishing = true;
                Root.schedule.Execute(Finish).StartingIn(1000);
            }
        }

        private void OnCycleComplete()
        {
            _cycles++;
            _cycleText.text = CycleText();

            string type = Breathing.DetermineBlockType(_cycles);
            var block = new BlockRecord { id = "b_" + _cycles, type = type, cycleNum = _cycles, techniqueId = _techniqueId };
            _blocks.Add(block);
            RebuildTower(newestId: block.id);

            // Flash.
            _flashOpacity = 1f;
            _flash.style.opacity = 0.7f;

            Haptics.Light();
            if (type == "focus") Haptics.Success();

            if (_sessionType == "cycles" && _cycles >= _targetCycles && !_finishing)
            {
                _finishing = true;
                Root.schedule.Execute(Finish).StartingIn(1000);
            }
        }

        // ── Breathing scale ─────────────────────────────────────────────────
        private void UpdateBreathScale(Phase ph, float t)
        {
            float scale;
            if (ph.Name == "inhale")
                scale = Mathf.Lerp(0.65f, 1.15f, EaseOutQuad(t));
            else if (ph.Name == "exhale")
                scale = Mathf.Lerp(1.15f, 0.65f, EaseInQuad(t));
            else // hold / hold2 — gentle pulse around 1.15
                scale = 1.15f + 0.05f * Mathf.Sin(2f * Mathf.PI * (_phaseMs / 1200f));
            ApplyBreathScale(scale);
        }

        private void ApplyBreathScale(float scale)
        {
            _ring.style.scale = new Scale(new Vector3(scale, scale, 1));
            _ringGlow.style.scale = new Scale(new Vector3(scale * 1.3f, scale * 1.3f, 1));
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;

        // ── Tower rendering ─────────────────────────────────────────────────
        private void RebuildTower(string newestId)
        {
            _towerStack.Clear();
            int start = Mathf.Max(0, _blocks.Count - MaxBlocks);
            for (int i = start; i < _blocks.Count; i++)
            {
                var block = _blocks[i];
                var el = BuildBlock(block);
                _towerStack.Add(el);
                if (block.id == newestId)
                    AnimateBlockDrop(el);
            }
        }

        private VisualElement BuildBlock(BlockRecord block)
        {
            Color color = block.type == "focus" || block.type == "perfect" ? Theme.BlockPerfect
                : block.type == "calm" ? Theme.BlockCalm
                : _techniqueColor;

            var el = Box().Size(BlockW, BlockH).Radius(5).Center();
            el.style.backgroundColor = color;
            el.style.marginBottom = 2;

            if (block.type == "focus")
            {
                var star = Text("*", 10, Theme.Bg, FontStyle.Bold);
                star.pickingMode = PickingMode.Ignore;
                el.Add(star);
            }
            else if (block.type == "calm")
            {
                var line = Box().Height(2).Radius(1).Bg(Theme.TextPrimary.WithAlpha(0.4f));
                line.style.width = Length.Percent(60);
                el.Add(line);
            }
            return el;
        }

        private void AnimateBlockDrop(VisualElement el)
        {
            el.style.translate = new Translate(0, -40, 0);
            el.style.scale = new Scale(new Vector3(0.7f, 1, 1));
            el.experimental.animation.Start(0f, 1f, 420, (e, v) =>
            {
                e.style.translate = new Translate(0, Mathf.Lerp(-40, 0, v), 0);
                float sx = Mathf.Lerp(0.7f, 1f, v);
                e.style.scale = new Scale(new Vector3(sx, 1, 1));
            }).Ease(Easing.OutBack);
        }

        // ── Phase UI ────────────────────────────────────────────────────────
        private void SetPhaseUi(Phase ph)
        {
            Color c = Theme.PhaseColor(ph.Name);
            string label = ph.Name == "inhale" ? "Inhale" : ph.Name == "exhale" ? "Exhale" : "Hold";
            _phaseLabel.text = _started ? label : "Ready";
            _phaseLabel.style.color = c;
            _timerNum.style.color = c;
            _ring.SetBorderColor(c);
            _ringGlow.SetBorderColor(c);
            _timerNum.text = ph.Duration.ToString("0.0");
        }

        // ── Finish ──────────────────────────────────────────────────────────
        private void Finish()
        {
            if (!_active && _completionActive) return;
            _active = false;
            _completionActive = true;
            _completionTime = 0;

            int durationSec = Mathf.FloorToInt(_totalMs / 1000f);
            int perfect = 0, calm = 0, focus = 0;
            foreach (var b in _blocks)
            {
                if (b.type == "perfect" || b.type == "focus") perfect++;
                if (b.type == "calm") calm++;
                if (b.type == "focus") focus++;
            }

            var record = new SessionRecord
            {
                id = _sessionId,
                date = System.DateTime.UtcNow.ToString("o"),
                techniqueId = _techniqueId,
                techniqueName = _technique.Name,
                cyclesCompleted = _cycles,
                perfectBlocks = perfect,
                calmBlocks = calm,
                focusFloors = focus,
                totalFloors = _cycles,
                durationSeconds = durationSec,
                blocks = new List<BlockRecord>(_blocks),
            };
            record.badges = SaveSystem.ComputeBadges(record);
            SaveSystem.SaveSession(record);

            Root.schedule.Execute(() =>
                App.Replace(ScreenId.Summary, new NavArgs().Set("sessionId", record.id))
            ).StartingIn(1200);
        }

        public override void Dispose()
        {
            _active = false;
        }

        private string CycleText()
        {
            bool showTarget = _sessionType == "cycles" && _targetCycles != int.MaxValue;
            return showTarget ? $"{_cycles}/{_targetCycles}" : _cycles.ToString();
        }
    }
}
