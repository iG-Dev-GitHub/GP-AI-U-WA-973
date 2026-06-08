using System.Collections.Generic;
using BreathTower.App;
using BreathTower.Core;
using BreathTower.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static BreathTower.UI.Widgets;

namespace BreathTower.Screens
{
    /// <summary>Technique + session-length picker. Ports app/technique.tsx.</summary>
    public sealed class TechniqueScreen : ScreenBase
    {
        private static readonly (string label, int value)[] CyclesLengths =
            { ("10 cycles", 10), ("15 cycles", 15), ("20 cycles", 20), ("30 cycles", 30) };
        private static readonly (string label, int value)[] TimeLengths =
            { ("5 min", 5), ("10 min", 10), ("15 min", 15), ("20 min", 20) };

        private string _selectedId = "4-7-8";
        private string _mode = "cycles"; // cycles | time
        private int _value = 20;

        private string _inhale = "4", _hold = "2", _exhale = "4", _hold2 = "0";

        private VisualElement _techniqueList;
        private VisualElement _customHost;
        private VisualElement _modeRow;
        private VisualElement _lengthRow;

        public override VisualElement Build()
        {
            var root = Scaffold.Screen(App.Assets.techniqueLayout, out var content);
            content.Col();

            var scroll = VerticalScroll();
            scroll.style.flexGrow = 1;
            scroll.contentContainer.style.paddingLeft = Theme.SpaceMd;
            scroll.contentContainer.style.paddingRight = Theme.SpaceMd;
            scroll.contentContainer.style.paddingTop = Theme.SpaceSm;
            content.Add(scroll);
            var c = scroll.contentContainer;

            // Header.
            var header = Row().AlignItems(Align.Center).Justify(Justify.SpaceBetween);
            header.style.marginBottom = Theme.SpaceLg;
            header.Add(RoundButton("‹", () => App.Back()));
            header.Add(Text("Choose Technique", Theme.FontXl, Theme.TextPrimary, FontStyle.Bold));
            header.Add(Box().Width(40));
            c.Add(header);

            c.Add(SectionLabel("BREATHING TECHNIQUE"));
            _techniqueList = Box();
            c.Add(_techniqueList);
            RebuildTechniques();

            _customHost = Box();
            c.Add(_customHost);
            RebuildCustom();

            var lengthLabel = SectionLabel("SESSION LENGTH");
            lengthLabel.style.marginTop = Theme.SpaceLg;
            c.Add(lengthLabel);

            _modeRow = Row();
            _modeRow.style.marginBottom = Theme.SpaceSm;
            c.Add(_modeRow);
            RebuildModes();

            _lengthRow = Row().Wrap();
            _lengthRow.style.marginBottom = Theme.SpaceLg;
            c.Add(_lengthRow);
            RebuildLengths();

            var start = GradientButton("Start Session", OnStart);
            start.style.marginTop = Theme.SpaceSm;
            c.Add(start);

            c.Add(Box().Height(Theme.SpaceXl));
            Widgets.LockScrollContent(scroll);
            return root;
        }

        private void RebuildTechniques()
        {
            _techniqueList.Clear();
            foreach (var tech in Breathing.Techniques)
            {
                bool selected = _selectedId == tech.Id;
                Color tcolor = Theme.TechniqueColor(tech.Id);

                // Column card with an absolutely-positioned accent bar. Using a
                // column (not a flex-row with a flex:1 child) lets the wrapped
                // description measure its height against a definite width, so the
                // card grows to fit and nothing is clipped.
                var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg);
                card.style.position = Position.Relative;
                card.style.marginBottom = Theme.SpaceSm;
                card.Border(2, selected ? tcolor : Color.clear);

                var bar = Box().Absolute().Bg(tcolor);
                bar.style.left = 0; bar.style.top = 0; bar.style.bottom = 0; bar.style.width = 6;
                bar.style.borderTopLeftRadius = bar.style.borderBottomLeftRadius = Theme.RadiusLg;
                bar.pickingMode = PickingMode.Ignore;
                card.Add(bar);

                var contentCol = Box().Pad(Theme.SpaceMd);
                contentCol.style.paddingLeft = Theme.SpaceMd + 8; // clear the accent bar
                var headRow = Row().Justify(Justify.SpaceBetween).AlignItems(Align.Center);
                headRow.style.marginBottom = 4;
                headRow.Add(Text(tech.Name, Theme.FontMd, Theme.TextPrimary, FontStyle.Bold));
                if (selected) headRow.Add(Text("•", Theme.FontLg, tcolor, FontStyle.Bold));
                contentCol.Add(headRow);

                var rhythm = Text(tech.Rhythm, Theme.FontLg, tcolor, FontStyle.Bold);
                rhythm.style.letterSpacing = 2;
                rhythm.style.marginBottom = 6;
                contentCol.Add(rhythm);

                var desc = Text(tech.Description, Theme.FontSm, Theme.TextSecondary);
                desc.style.whiteSpace = WhiteSpace.Normal;
                contentCol.Add(desc);
                card.Add(contentCol);

                string id = tech.Id;
                Widgets.MakeClickable(card, () =>
                {
                    _selectedId = id;
                    RebuildTechniques();
                    RebuildCustom();
                });

                _techniqueList.Add(card);
            }
        }

        private void RebuildCustom()
        {
            _customHost.Clear();
            if (_selectedId != "custom") return;

            var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Pad(Theme.SpaceMd);
            card.style.marginBottom = Theme.SpaceSm;
            card.Border(1, Theme.TechniqueCustom.WithAlpha(0.27f));

            var title = Text("Custom Rhythm (seconds)", Theme.FontSm, Theme.TextPrimary, FontStyle.Bold);
            title.style.marginBottom = Theme.SpaceMd;
            card.Add(title);

            var row = Row();
            row.Add(NumberField("Inhale", _inhale, v => _inhale = v));
            row.Add(NumberField("Hold", _hold, v => _hold = v));
            row.Add(NumberField("Exhale", _exhale, v => _exhale = v));
            row.Add(NumberField("Hold 2", _hold2, v => _hold2 = v));
            row.GapChildren(Theme.SpaceSm);
            card.Add(row);

            _customHost.Add(card);
        }

        private VisualElement NumberField(string label, string value, System.Action<string> onChange)
        {
            var field = Box().Flex(1).AlignItems(Align.Center);
            var l = Text(label, Theme.FontXs, Theme.TextMuted);
            l.style.marginBottom = Theme.SpaceXs;
            field.Add(l);

            var input = new TextField { value = value };
            input.style.width = Length.Percent(100);
            input.maxLength = 2;
            StyleNumberInput(input);
            input.RegisterValueChangedCallback(e =>
            {
                string digits = Digits(e.newValue);
                if (digits != e.newValue) input.SetValueWithoutNotify(digits);
                onChange(digits);
            });
            field.Add(input);
            return field;
        }

        private void StyleNumberInput(TextField input)
        {
            input.style.backgroundColor = Theme.SurfaceHighlight;
            var inner = input.Q(null, "unity-text-field__input");
            if (inner != null)
            {
                inner.style.backgroundColor = Theme.SurfaceHighlight;
                inner.style.color = Theme.TextPrimary;
                inner.style.unityTextAlign = TextAnchor.MiddleCenter;
                inner.style.fontSize = Theme.FontLg;
                inner.style.borderTopWidth = inner.style.borderBottomWidth =
                    inner.style.borderLeftWidth = inner.style.borderRightWidth = 0;
            }
            input.style.borderTopLeftRadius = input.style.borderTopRightRadius =
                input.style.borderBottomLeftRadius = input.style.borderBottomRightRadius = Theme.RadiusSm;
            input.style.paddingTop = input.style.paddingBottom = Theme.SpaceSm;
            input.style.color = Theme.TextPrimary;
        }

        private void RebuildModes()
        {
            _modeRow.Clear();
            _modeRow.Add(ModeButton("Cycles", "cycles"));
            _modeRow.Add(ModeButton("Time", "time"));
            _modeRow.GapChildren(Theme.SpaceSm);
        }

        private VisualElement ModeButton(string label, string mode)
        {
            bool active = _mode == mode;
            var btn = Box().Flex(1).Center().Radius(Theme.RadiusMd);
            btn.style.paddingTop = btn.style.paddingBottom = Theme.SpaceSm;
            btn.style.backgroundColor = active ? Theme.Cyan : Theme.Surface;
            var l = Text(label, Theme.FontSm, active ? Theme.Bg : Theme.TextSecondary, FontStyle.Bold);
            l.pickingMode = PickingMode.Ignore;
            btn.Add(l);
            Widgets.MakeClickable(btn, () =>
            {
                _mode = mode;
                _value = mode == "cycles" ? 20 : 10;
                RebuildModes();
                RebuildLengths();
            });
            return btn;
        }

        private void RebuildLengths()
        {
            _lengthRow.Clear();
            var lengths = _mode == "cycles" ? CyclesLengths : TimeLengths;
            foreach (var (label, value) in lengths)
            {
                bool active = _value == value;
                var btn = Box().Radius(Theme.RadiusPill).Bg(Theme.Surface);
                btn.style.paddingTop = btn.style.paddingBottom = Theme.SpaceSm;
                btn.style.paddingLeft = btn.style.paddingRight = Theme.SpaceMd;
                btn.style.marginRight = Theme.SpaceSm;
                btn.style.marginBottom = Theme.SpaceSm;
                btn.Border(1, active ? Theme.Cyan : Color.clear);
                var l = Text(label, Theme.FontSm, active ? Theme.Cyan : Theme.TextSecondary, FontStyle.Bold);
                l.pickingMode = PickingMode.Ignore;
                btn.Add(l);
                int v = value;
                Widgets.MakeClickable(btn, () => { _value = v; RebuildLengths(); });
                _lengthRow.Add(btn);
            }
        }

        private void OnStart()
        {
            var args = new NavArgs()
                .Set("techniqueId", _selectedId)
                .Set("sessionType", _mode)
                .Set("sessionValue", _value);

            if (_selectedId == "custom")
            {
                args.Set("customInhale", string.IsNullOrEmpty(_inhale) ? "4" : _inhale)
                    .Set("customHold", string.IsNullOrEmpty(_hold) ? "0" : _hold)
                    .Set("customExhale", string.IsNullOrEmpty(_exhale) ? "4" : _exhale)
                    .Set("customHold2", string.IsNullOrEmpty(_hold2) ? "0" : _hold2);
            }

            App.Navigate(ScreenId.Session, args);
        }

        private static string Digits(string s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var ch in s) if (char.IsDigit(ch)) sb.Append(ch);
            return sb.ToString();
        }

        private VisualElement SectionLabel(string text)
        {
            var l = Text(text.ToUpperInvariant(), Theme.FontXs, Theme.TextMuted, FontStyle.Bold);
            l.style.letterSpacing = 1.5f;
            l.style.marginBottom = Theme.SpaceSm;
            return l;
        }
    }
}
