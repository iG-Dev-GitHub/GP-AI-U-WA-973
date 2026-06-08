using System;
using BreathTower.App;
using BreathTower.Core;
using BreathTower.Data;
using BreathTower.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static BreathTower.UI.Widgets;

namespace BreathTower.Screens
{
    /// <summary>Settings + saved rhythms + danger zone. Ports app/settings.tsx.</summary>
    public sealed class SettingsScreen : ScreenBase
    {
        private AppSettings _settings;
        private VisualElement _rhythmsCard;
        private VisualElement _dangerCard;

        private bool _showAddRhythm;
        private string _rName = "", _rInhale = "4", _rHold = "0", _rExhale = "4", _rHold2 = "0";
        private bool _showResetConfirm;

        public override VisualElement Build()
        {
            _settings = SaveSystem.Data.settings;

            var root = Scaffold.Screen(App.Assets.settingsLayout, out var content);
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
            header.Add(Text("Settings", Theme.FontXl, Theme.TextPrimary, FontStyle.Bold));
            header.Add(Box().Width(40));
            c.Add(header);

            // Preferences.
            c.Add(SectionLabel("Preferences"));
            var prefs = Card();
            prefs.style.marginBottom = Theme.SpaceSm;
            prefs.Add(SettingRow("Haptic Feedback", "Vibrate on block placement",
                Switch(_settings.vibrationEnabled, v => { _settings.vibrationEnabled = v; SaveSystem.UpdateSettings(_settings); })));
            prefs.Add(DividerLine());
            prefs.Add(SettingRow("Sound Effects", "Coming soon",
                Switch(_settings.soundEnabled, v => { _settings.soundEnabled = v; SaveSystem.UpdateSettings(_settings); })));
            c.Add(prefs);

            // Saved rhythms.
            var rl = SectionLabel("Saved Rhythms");
            rl.style.marginTop = Theme.SpaceLg;
            c.Add(rl);
            _rhythmsCard = Card();
            _rhythmsCard.style.marginBottom = Theme.SpaceSm;
            c.Add(_rhythmsCard);
            RebuildRhythms();

            // About.
            var al = SectionLabel("About");
            al.style.marginTop = Theme.SpaceLg;
            c.Add(al);
            var about = Card();
            about.Add(SettingRow("Breath Tower Rise Builder", "Version 1.0.0 · Offline breathing tracker",
                Text("v1.0", Theme.FontSm, Theme.TextMuted)));
            c.Add(about);

            // Danger zone.
            var dl = SectionLabel("Danger Zone");
            dl.style.marginTop = Theme.SpaceLg;
            dl.style.color = Theme.Danger.WithAlpha(0.67f);
            c.Add(dl);
            _dangerCard = Card();
            _dangerCard.Border(1, Theme.Danger.WithAlpha(0.13f));
            c.Add(_dangerCard);
            RebuildDanger();

            c.Add(Box().Height(Theme.SpaceXl));
            Widgets.LockScrollContent(scroll);
            return root;
        }

        // ── Rhythms ─────────────────────────────────────────────────────────
        private void RebuildRhythms()
        {
            _rhythmsCard.Clear();

            if (_settings.customRhythms.Count == 0)
            {
                var empty = Text("No saved rhythms yet", Theme.FontSm, Theme.TextMuted);
                empty.style.paddingTop = empty.style.paddingBottom = Theme.SpaceSm;
                empty.style.paddingLeft = empty.style.paddingRight = Theme.SpaceSm;
                _rhythmsCard.Add(empty);
            }
            else
            {
                for (int i = 0; i < _settings.customRhythms.Count; i++)
                {
                    var r = _settings.customRhythms[i];
                    if (i > 0) _rhythmsCard.Add(DividerLine());

                    var row = Row().AlignItems(Align.Center).Justify(Justify.SpaceBetween);
                    row.style.paddingTop = row.style.paddingBottom = Theme.SpaceSm;

                    var info = Box().Flex(1);
                    info.Add(Text(r.name, Theme.FontSm, Theme.TextPrimary, FontStyle.Bold));
                    string sub = $"Inhale {r.inhale}s" +
                                 (r.hold > 0 ? $" · Hold {r.hold}s" : "") +
                                 $" · Exhale {r.exhale}s" +
                                 (r.hold2 > 0 ? $" · Hold {r.hold2}s" : "");
                    info.Add(Text(sub, Theme.FontXs, Theme.TextMuted));
                    row.Add(info);

                    string id = r.id;
                    var del = Box().Pad(Theme.SpaceSm);
                    var trash = Text("×", Theme.FontMd, Theme.Danger);
                    trash.pickingMode = PickingMode.Ignore;
                    del.Add(trash);
                    Widgets.MakeClickable(del, () => { SaveSystem.DeleteCustomRhythm(id); RebuildRhythms(); });
                    row.Add(del);

                    _rhythmsCard.Add(row);
                }
            }

            if (!_showAddRhythm)
            {
                var addBtn = Row().AlignItems(Align.Center);
                addBtn.style.paddingTop = addBtn.style.paddingBottom = Theme.SpaceSm;
                addBtn.style.marginTop = Theme.SpaceXs;
                var plus = Text("+", Theme.FontLg, Theme.Cyan, FontStyle.Bold);
                plus.pickingMode = PickingMode.Ignore;
                plus.style.marginRight = Theme.SpaceXs;
                addBtn.Add(plus);
                var lbl = Text("Add Custom Rhythm", Theme.FontSm, Theme.Cyan, FontStyle.Bold);
                lbl.pickingMode = PickingMode.Ignore;
                addBtn.Add(lbl);
                Widgets.MakeClickable(addBtn, () => { _showAddRhythm = true; RebuildRhythms(); });
                _rhythmsCard.Add(addBtn);
            }
            else
            {
                _rhythmsCard.Add(BuildAddForm());
            }
        }

        private VisualElement BuildAddForm()
        {
            var form = Box();
            form.style.paddingTop = Theme.SpaceMd;

            var title = Text("New Rhythm", Theme.FontSm, Theme.TextPrimary, FontStyle.Bold);
            title.style.marginBottom = Theme.SpaceSm;
            form.Add(title);

            var nameInput = MakeTextField(_rName, "Name (optional)", v => _rName = v, numeric: false);
            nameInput.style.marginBottom = Theme.SpaceSm;
            form.Add(nameInput);

            var row = Row();
            row.style.marginBottom = Theme.SpaceSm;
            row.Add(NumField("Inhale", _rInhale, v => _rInhale = v));
            row.Add(NumField("Hold", _rHold, v => _rHold = v));
            row.Add(NumField("Exhale", _rExhale, v => _rExhale = v));
            row.Add(NumField("Hold 2", _rHold2, v => _rHold2 = v));
            row.GapChildren(Theme.SpaceSm);
            form.Add(row);

            var btns = Row().Justify(Justify.FlexEnd);
            btns.Add(PillButton("Cancel", Theme.SurfaceHighlight, Theme.TextSecondary, () =>
            {
                _showAddRhythm = false; RebuildRhythms();
            }));
            var save = PillButton("Save", Theme.Cyan, Theme.Bg, OnSaveRhythm);
            save.style.marginLeft = Theme.SpaceSm;
            btns.Add(save);
            form.Add(btns);
            return form;
        }

        private void OnSaveRhythm()
        {
            int inhale = ParseOr(_rInhale, 4);
            int hold = ParseOr(_rHold, 0);
            int exhale = ParseOr(_rExhale, 4);
            int hold2 = ParseOr(_rHold2, 0);

            var rhythm = new CustomRhythm
            {
                id = "custom_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                name = string.IsNullOrWhiteSpace(_rName) ? $"{inhale}-{exhale} Rhythm" : _rName.Trim(),
                inhale = inhale, hold = hold, exhale = exhale, hold2 = hold2,
            };
            SaveSystem.SaveCustomRhythm(rhythm);

            _showAddRhythm = false;
            _rName = ""; _rInhale = "4"; _rHold = "0"; _rExhale = "4"; _rHold2 = "0";
            RebuildRhythms();
        }

        // ── Danger zone ─────────────────────────────────────────────────────
        private void RebuildDanger()
        {
            _dangerCard.Clear();
            if (!_showResetConfirm)
            {
                var btn = Row().AlignItems(Align.Center);
                btn.style.paddingTop = btn.style.paddingBottom = Theme.SpaceSm;
                var icon = Text("!", Theme.FontLg, Theme.Danger);
                icon.pickingMode = PickingMode.Ignore;
                icon.style.marginRight = Theme.SpaceSm;
                btn.Add(icon);
                var l = Text("Reset All Data", Theme.FontMd, Theme.Danger, FontStyle.Bold);
                l.pickingMode = PickingMode.Ignore;
                btn.Add(l);
                Widgets.MakeClickable(btn, () => { _showResetConfirm = true; RebuildDanger(); });
                _dangerCard.Add(btn);
            }
            else
            {
                var box = Box();
                var t = Text("This will erase all your sessions and tower data. Are you sure?", Theme.FontSm, Theme.TextSecondary);
                t.style.marginBottom = Theme.SpaceMd;
                box.Add(t);
                var btns = Row().Justify(Justify.FlexEnd);
                btns.Add(PillButton("Cancel", Theme.SurfaceHighlight, Theme.TextSecondary, () =>
                {
                    _showResetConfirm = false; RebuildDanger();
                }));
                var yes = PillButton("Yes, Reset", Theme.Danger, Theme.TextPrimary, () =>
                {
                    SaveSystem.ResetAllData();
                    App.Replace(ScreenId.Welcome);
                });
                yes.style.marginLeft = Theme.SpaceSm;
                btns.Add(yes);
                box.Add(btns);
                _dangerCard.Add(box);
            }
        }

        // ── Reusable bits ───────────────────────────────────────────────────
        private VisualElement SettingRow(string label, string sub, VisualElement right)
        {
            var row = Row().AlignItems(Align.Center).Justify(Justify.SpaceBetween);
            row.style.paddingTop = row.style.paddingBottom = Theme.SpaceSm;
            var text = Box().Flex(1);
            text.style.marginRight = Theme.SpaceMd;
            text.Add(Text(label, Theme.FontMd, Theme.TextPrimary, FontStyle.Bold));
            if (!string.IsNullOrEmpty(sub))
            {
                var s = Text(sub, Theme.FontXs, Theme.TextMuted);
                s.style.marginTop = 2;
                text.Add(s);
            }
            row.Add(text);
            row.Add(right);
            return row;
        }

        private VisualElement Switch(bool value, Action<bool> onChange)
        {
            bool state = value;
            var track = Box().Size(48, 28).Radius(14);
            track.style.justifyContent = Justify.Center;
            var knob = Box().Size(22, 22).Radius(11);
            knob.style.marginLeft = 3; knob.style.marginRight = 3;

            void Render()
            {
                track.style.backgroundColor = state ? Theme.Cyan.WithAlpha(0.27f) : Theme.Surface;
                knob.style.backgroundColor = state ? Theme.Cyan : Theme.TextMuted;
                knob.style.alignSelf = state ? Align.FlexEnd : Align.FlexStart;
            }
            Render();
            track.Add(knob);
            Widgets.MakeClickable(track, () => { state = !state; Render(); onChange(state); });
            return track;
        }

        private VisualElement PillButton(string label, Color bg, Color fg, Action onClick)
        {
            var btn = Box().Radius(Theme.RadiusPill).Center();
            btn.style.paddingTop = btn.style.paddingBottom = Theme.SpaceSm;
            btn.style.paddingLeft = btn.style.paddingRight = Theme.SpaceMd;
            btn.style.backgroundColor = bg;
            var l = Text(label, Theme.FontSm, fg, FontStyle.Bold);
            l.pickingMode = PickingMode.Ignore;
            btn.Add(l);
            Widgets.MakeClickable(btn, onClick);
            return btn;
        }

        private VisualElement NumField(string label, string value, Action<string> onChange)
        {
            var field = Box().Flex(1).AlignItems(Align.Center);
            var l = Text(label, Theme.FontXs, Theme.TextMuted);
            l.style.marginBottom = 4;
            field.Add(l);
            field.Add(MakeTextField(value, "0", onChange, numeric: true, maxLen: 2));
            return field;
        }

        private TextField MakeTextField(string value, string placeholder, Action<string> onChange, bool numeric, int maxLen = 0)
        {
            var input = new TextField { value = value };
            if (maxLen > 0) input.maxLength = maxLen;
            input.style.width = Length.Percent(100);
            input.style.backgroundColor = Theme.SurfaceHighlight;
            input.style.color = Theme.TextPrimary;
            input.style.borderTopLeftRadius = input.style.borderTopRightRadius =
                input.style.borderBottomLeftRadius = input.style.borderBottomRightRadius = Theme.RadiusSm;
            input.style.paddingTop = input.style.paddingBottom = Theme.SpaceSm;

            var inner = input.Q(null, "unity-text-field__input");
            if (inner != null)
            {
                inner.style.backgroundColor = Theme.SurfaceHighlight;
                inner.style.color = Theme.TextPrimary;
                inner.style.borderTopWidth = inner.style.borderBottomWidth =
                    inner.style.borderLeftWidth = inner.style.borderRightWidth = 0;
                if (numeric)
                {
                    inner.style.unityTextAlign = TextAnchor.MiddleCenter;
                    inner.style.fontSize = Theme.FontLg;
                }
            }

            input.RegisterValueChangedCallback(e =>
            {
                if (numeric)
                {
                    string digits = OnlyDigits(e.newValue);
                    if (digits != e.newValue) input.SetValueWithoutNotify(digits);
                    onChange(digits);
                }
                else onChange(e.newValue);
            });
            return input;
        }

        private static int ParseOr(string s, int fallback)
            => int.TryParse(s, out var n) ? n : fallback;

        private static string OnlyDigits(string s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var ch in s) if (char.IsDigit(ch)) sb.Append(ch);
            return sb.ToString();
        }

        private VisualElement DividerLine()
        {
            var d = Box().Height(1).Bg(Theme.TextPrimary.WithAlpha(0.07f));
            d.style.marginTop = d.style.marginBottom = Theme.SpaceXs;
            return d;
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
