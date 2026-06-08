using System.Collections.Generic;
using BreathTower.App;
using BreathTower.Core;
using BreathTower.Data;
using BreathTower.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static BreathTower.UI.Widgets;

namespace BreathTower.Screens
{
    /// <summary>Post-session results. Ports app/summary.tsx.</summary>
    public sealed class SummaryScreen : ScreenBase
    {
        private static readonly Dictionary<string, string> BadgeIcons = new Dictionary<string, string>
        {
            { "Peak Breath", "*" }, { "Focus Master", "*" }, { "Calm Spirit", "*" },
            { "Tower Builder", "*" }, { "Sky Raiser", "*" },
        };

        public override VisualElement Build()
        {
            var root = Scaffold.Screen(App.Assets.summaryLayout, out var content);
            content.Col();

            var session = SaveSystem.GetSessionById(Args.Get("sessionId"));
            if (session == null) return root; // blank night sky, matches loading state

            Color tc = Theme.TechniqueColor(session.techniqueId);

            var scroll = VerticalScroll();
            scroll.style.flexGrow = 1;
            scroll.contentContainer.style.paddingLeft = Theme.SpaceMd;
            scroll.contentContainer.style.paddingRight = Theme.SpaceMd;
            scroll.contentContainer.style.paddingTop = Theme.SpaceLg;
            content.Add(scroll);
            var c = scroll.contentContainer;

            BuildHero(c, session, tc);
            BuildStats(c, session);
            BuildSpecialBlocks(c, session);
            BuildMiniTower(c, session);
            BuildBadges(c, session);
            BuildActions(c);

            c.Add(Box().Height(Theme.SpaceXl));
            Widgets.LockScrollContent(scroll);
            return root;
        }

        private void BuildHero(VisualElement parent, SessionRecord s, Color tc)
        {
            var hero = Box().AlignItems(Align.Center);
            hero.style.marginBottom = Theme.SpaceXl;

            // Drawn celebratory glyph (font-independent): a glowing tower.
            var glyph = Widgets.TowerGlyph(60);
            glyph.style.marginBottom = Theme.SpaceMd;
            hero.Add(glyph);

            var title = Text("Session Complete!", Theme.FontXxl, Theme.TextPrimary, FontStyle.Bold);
            title.style.marginBottom = Theme.SpaceSm;
            hero.Add(title);

            var tag = Row().AlignItems(Align.Center).Radius(Theme.RadiusPill);
            tag.style.backgroundColor = tc.WithAlpha(0.13f);
            tag.style.paddingLeft = tag.style.paddingRight = Theme.SpaceMd;
            tag.style.paddingTop = tag.style.paddingBottom = Theme.SpaceXs;
            var dot = Box().Size(8, 8).Radius(4).Bg(tc);
            dot.style.marginRight = Theme.SpaceXs;
            tag.Add(dot);
            tag.Add(Text(s.techniqueName, Theme.FontSm, tc, FontStyle.Bold));
            hero.Add(tag);

            parent.Add(hero);
        }

        private void BuildStats(VisualElement parent, SessionRecord s)
        {
            var grid = Row().Wrap();
            grid.style.marginBottom = Theme.SpaceMd;
            grid.Add(StatBox(s.totalFloors.ToString(), "Floors Built", Theme.TextPrimary));
            grid.Add(StatBox(s.cyclesCompleted.ToString(), "Cycles", Theme.Cyan));
            grid.Add(StatBox(s.perfectBlocks.ToString(), "Perfect", Theme.Gold));
            grid.Add(StatBox(Format.DurationWords(s.durationSeconds), "Duration", Theme.TextSecondary));
            parent.Add(grid);
        }

        private VisualElement StatBox(string value, string label, Color color)
        {
            var box = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Pad(Theme.SpaceMd).AlignItems(Align.Center);
            box.style.flexGrow = 1;
            box.style.minWidth = Length.Percent(45);
            box.style.marginRight = Theme.SpaceSm;
            box.style.marginBottom = Theme.SpaceSm;

            var v = Text(value, Theme.FontXxxl, color, FontStyle.Bold);
            box.Add(v);
            var l = Text(label.ToUpperInvariant(), Theme.FontXs, Theme.TextMuted);
            l.style.marginTop = 4; l.style.letterSpacing = 1;
            box.Add(l);

            // Pop-in animation.
            box.style.scale = new Scale(new Vector3(0.6f, 0.6f, 1));
            box.experimental.animation.Start(0f, 1f, 360, (el, t) =>
                el.style.scale = new Scale(new Vector3(Mathf.Lerp(0.6f, 1f, t), Mathf.Lerp(0.6f, 1f, t), 1)))
                .Ease(UnityEngine.UIElements.Experimental.Easing.OutBack);
            return box;
        }

        private void BuildSpecialBlocks(VisualElement parent, SessionRecord s)
        {
            if (s.calmBlocks <= 0 && s.focusFloors <= 0) return;
            var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Pad(Theme.SpaceMd);
            card.style.marginBottom = Theme.SpaceMd;

            var title = SectionLabel("Special Blocks");
            card.Add(title);

            var row = Row();
            if (s.calmBlocks > 0)
            {
                var item = Row().AlignItems(Align.Center);
                item.style.marginRight = Theme.SpaceLg;
                var d = Box().Size(10, 10).Radius(5).Bg(Theme.BlockCalm);
                d.style.marginRight = Theme.SpaceXs;
                item.Add(d);
                item.Add(Text($"{s.calmBlocks} Calm Block{(s.calmBlocks > 1 ? "s" : "")}", Theme.FontSm, Theme.TextPrimary, FontStyle.Bold));
                row.Add(item);
            }
            if (s.focusFloors > 0)
            {
                var item = Row().AlignItems(Align.Center);
                var star = Text("*", 14, Theme.Gold);
                star.style.marginRight = Theme.SpaceXs;
                item.Add(star);
                item.Add(Text($"{s.focusFloors} Focus Floor{(s.focusFloors > 1 ? "s" : "")}", Theme.FontSm, Theme.TextPrimary, FontStyle.Bold));
                row.Add(item);
            }
            card.Add(row);
            parent.Add(card);
        }

        private void BuildMiniTower(VisualElement parent, SessionRecord s)
        {
            var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Pad(Theme.SpaceMd);
            card.style.marginBottom = Theme.SpaceMd;
            card.Add(SectionLabel("This Session's Tower"));

            var preview = Box().AlignItems(Align.Center);
            int start = Mathf.Max(0, s.blocks.Count - 12);
            for (int i = s.blocks.Count - 1; i >= start; i--)
            {
                var block = s.blocks[i];
                Color color = block.type == "focus" || block.type == "perfect" ? Theme.BlockPerfect
                    : block.type == "calm" ? Theme.BlockCalm
                    : Theme.TechniqueColor(s.techniqueId);
                var blk = Box().Size(100, 14).Radius(4).Bg(color);
                blk.style.marginBottom = 3;
                preview.Add(blk);
            }
            card.Add(preview);
            parent.Add(card);
        }

        private void BuildBadges(VisualElement parent, SessionRecord s)
        {
            if (s.badges.Count == 0) return;
            var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Pad(Theme.SpaceMd);
            card.style.marginBottom = Theme.SpaceLg;
            card.Add(SectionLabel("Badges Earned"));

            var row = Row().Wrap();
            foreach (var badge in s.badges)
            {
                var b = Box().AlignItems(Align.Center).Pad(Theme.SpaceMd).Radius(Theme.RadiusLg);
                b.style.backgroundColor = Theme.Gold.WithAlpha(0.1f);
                b.style.minWidth = 90;
                b.style.marginRight = Theme.SpaceSm;
                b.style.marginBottom = Theme.SpaceSm;
                b.Border(1, Theme.Gold.WithAlpha(0.3f));
                var emoji = Text(BadgeIcons.TryGetValue(badge, out var ic) ? ic : "*", 28, Theme.Gold).TextCenter();
                emoji.style.marginBottom = Theme.SpaceXs;
                b.Add(emoji);
                b.Add(Text(badge, Theme.FontXs, Theme.Gold, FontStyle.Bold).TextCenter());
                row.Add(b);
            }
            card.Add(row);
            parent.Add(card);
        }

        private void BuildActions(VisualElement parent)
        {
            var again = GradientButton("Build Again", () => App.Replace(ScreenId.Technique));
            again.style.marginBottom = Theme.SpaceSm;
            parent.Add(again);

            var home = Row().Center();
            home.style.paddingTop = home.style.paddingBottom = Theme.SpaceMd;
            var l = Text("Go Home", Theme.FontMd, Theme.TextSecondary, FontStyle.Bold);
            l.pickingMode = PickingMode.Ignore;
            home.Add(l);
            Widgets.MakeClickable(home, () => App.Navigate(ScreenId.Home, NavArgs.Empty, clearHistory: true));
            parent.Add(home);
        }

        private VisualElement SectionLabel(string text)
        {
            var l = Text(text.ToUpperInvariant(), Theme.FontXs, Theme.TextSecondary, FontStyle.Bold);
            l.style.letterSpacing = 1.5f;
            l.style.marginBottom = Theme.SpaceMd;
            return l;
        }
    }
}
