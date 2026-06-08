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
    /// <summary>Home dashboard. Ports app/home.tsx.</summary>
    public sealed class HomeScreen : ScreenBase
    {
        private static readonly (float x, float w, float h)[] Buildings =
        {
            (0,50,90),(55,35,120),(93,60,72),(156,40,100),(200,55,65),
            (258,38,105),(298,52,82),(352,45,115),(400,50,78),
        };

        private static readonly (int b, float x, float y)[] Windows =
        {
            (0,12,20),(0,30,20),(0,12,45),(0,30,45),(0,12,65),
            (1,8,20),(1,20,20),(1,8,45),(1,20,45),(1,8,70),(1,20,70),
            (2,12,18),(2,35,18),(2,12,40),(2,35,40),
            (3,10,22),(3,25,22),(3,10,48),(3,25,48),
            (4,12,15),(4,30,15),(4,12,38),
            (5,10,20),(5,22,20),(5,10,45),(5,22,45),
            (6,10,18),(6,28,18),(6,10,40),(6,28,40),
            (7,12,22),(7,28,22),(7,12,50),(7,28,50),(7,12,78),(7,28,78),
            (8,12,20),(8,28,20),(8,12,48),
        };

        public override VisualElement Build()
        {
            var root = Scaffold.Screen(App.Assets.homeLayout, out var content);
            content.Col();

            var data = SaveSystem.Data;
            int totalFloors = data.totalFloors;
            var sessions = data.sessions.GetRange(0, Mathf.Min(20, data.sessions.Count));

            var scroll = VerticalScroll();
            scroll.style.flexGrow = 1;
            scroll.contentContainer.style.paddingLeft = Theme.SpaceMd;
            scroll.contentContainer.style.paddingRight = Theme.SpaceMd;
            scroll.contentContainer.style.paddingTop = Theme.SpaceSm;
            content.Add(scroll);

            BuildHeader(scroll.contentContainer);
            BuildHero(scroll.contentContainer, totalFloors);
            BuildActions(scroll.contentContainer);

            var sectionTitle = SectionLabel("Recent Sessions");
            scroll.contentContainer.Add(sectionTitle);

            if (sessions.Count == 0)
                scroll.contentContainer.Add(BuildEmpty());
            else
                foreach (var s in sessions)
                    scroll.contentContainer.Add(BuildSessionCard(s));

            scroll.contentContainer.Add(Box().Height(Theme.SpaceXl));
            Widgets.LockScrollContent(scroll);
            return root;
        }

        private void BuildHeader(VisualElement parent)
        {
            var header = Row().AlignItems(Align.Center).Justify(Justify.SpaceBetween);
            header.style.marginBottom = Theme.SpaceLg;
            header.PadH(Theme.SpaceXs);

            header.Add(Text("Breath Tower", Theme.FontXl, Theme.TextPrimary, FontStyle.Bold));

            var settings = Box().Pad(Theme.SpaceSm).Radius(Theme.RadiusPill).Bg(Theme.Surface);
            var gear = Text("•••", Theme.FontLg, Theme.TextSecondary).TextCenter(); // ⚙
            gear.pickingMode = PickingMode.Ignore;
            settings.Add(gear);
            Widgets.MakeClickable(settings, () => App.Navigate(ScreenId.Settings));
            header.Add(settings);

            parent.Add(header);
        }

        private void BuildHero(VisualElement parent, int totalFloors)
        {
            var hero = Box().Radius(Theme.RadiusXl).Overflow(true);
            hero.style.marginBottom = Theme.SpaceMd;

            var grad = Box().Pad(Theme.SpaceMd).Overflow(true);
            grad.style.minHeight = 240;
            grad.style.justifyContent = Justify.FlexEnd;
            grad.style.backgroundImage = new StyleBackground(Gradients.Vertical(Theme.Surface, Theme.SurfaceAlt));
            hero.Add(grad);

            // City silhouette.
            var city = Box().Absolute().Pos(0, null, 0, 0).Height(130);
            city.style.left = 0; city.style.right = 0;
            foreach (var (bx, bw, bh) in IndexedBuildings())
            {
                var b = Box().Absolute().Bg(Theme.HeroBuilding);
                b.style.bottom = 0; b.style.left = bx; b.style.width = bw; b.style.height = bh;
                city.Add(b);
            }
            // Windows
            for (int wi = 0; wi < Windows.Length; wi++)
            {
                var (bIdx, wx, wy) = Windows[wi];
                var bld = Buildings[bIdx];
                var win = Box().Absolute().Size(6, 5).Bg(Theme.WindowLight.WithAlpha(0.7f));
                win.style.left = bld.x + wx; win.style.bottom = wy; win.style.borderTopLeftRadius = 1;
                city.Add(win);
            }
            grad.Add(city);

            // Mini tower.
            var miniTower = Box().AlignSelf(Align.Center).AlignItems(Align.Center);
            miniTower.style.marginBottom = Theme.SpaceSm;
            int count = Mathf.Min(totalFloors, 14);
            for (int i = 0; i < count; i++)
            {
                int floor = i + 1;
                Color c = floor % 10 == 0 ? Theme.BlockFocus : floor % 5 == 0 ? Theme.BlockCalm : Theme.BlockPerfect;
                var blk = Box().Size(80, 14).Radius(4).Bg(c);
                blk.style.marginBottom = 2;
                miniTower.Add(blk);
            }
            grad.Add(miniTower);

            var stats = Box().AlignItems(Align.Center);
            stats.style.paddingBottom = Theme.SpaceSm;
            stats.Add(Text(totalFloors.ToString(), Theme.FontHuge, Theme.TextPrimary, FontStyle.Bold));
            var label = Text("TOTAL FLOORS", Theme.FontSm, Theme.TextSecondary);
            label.style.letterSpacing = 1.5f;
            stats.Add(label);
            grad.Add(stats);

            parent.Add(hero);
        }

        private IEnumerable<(float x, float w, float h)> IndexedBuildings()
        {
            foreach (var b in Buildings) yield return b;
        }

        private void BuildActions(VisualElement parent)
        {
            var main = GradientButton("+  New Session", () => App.Navigate(ScreenId.Technique));
            main.style.marginBottom = Theme.SpaceSm;
            parent.Add(main);

            var second = Row().Center();
            second.style.paddingTop = Theme.SpaceMd; second.style.paddingBottom = Theme.SpaceMd;
            second.style.marginBottom = Theme.SpaceLg;
            var secondLabel = Text("View Full Tower", Theme.FontMd, Theme.TextSecondary, FontStyle.Bold);
            secondLabel.pickingMode = PickingMode.Ignore;
            second.Add(secondLabel);
            Widgets.MakeClickable(second, () => App.Navigate(ScreenId.Tower));
            parent.Add(second);
        }

        private VisualElement BuildEmpty()
        {
            var card = Box().Center().Pad(Theme.SpaceXl).Bg(Theme.Surface).Radius(Theme.RadiusXl);
            var emoji = Widgets.TowerGlyph(56);
            emoji.style.marginBottom = Theme.SpaceMd;
            card.Add(emoji);
            var t = Text("No sessions yet", Theme.FontLg, Theme.TextPrimary, FontStyle.Bold);
            t.style.marginBottom = Theme.SpaceXs;
            card.Add(t);
            card.Add(Text("Start your first breathing session to build your tower!", Theme.FontSm, Theme.TextSecondary).TextCenter());
            return card;
        }

        private VisualElement BuildSessionCard(SessionRecord s)
        {
            Color tc = Theme.TechniqueColor(s.techniqueId);
            var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Pad(Theme.SpaceMd);
            card.style.marginBottom = Theme.SpaceSm;

            // Technique tag.
            var tag = Row().AlignItems(Align.Center).AlignSelf(Align.FlexStart).Radius(Theme.RadiusPill);
            tag.style.backgroundColor = tc.WithAlpha(0.13f);
            tag.style.paddingLeft = tag.style.paddingRight = Theme.SpaceSm;
            tag.style.paddingTop = tag.style.paddingBottom = 4;
            tag.style.marginBottom = Theme.SpaceSm;
            var dot = Box().Size(6, 6).Radius(3).Bg(tc);
            dot.style.marginRight = Theme.SpaceXs;
            tag.Add(dot);
            tag.Add(Text(s.techniqueName, Theme.FontXs, tc, FontStyle.Bold));
            card.Add(tag);

            // Stats row.
            var row = Row().AlignItems(Align.Center);
            row.Add(StatCol(s.totalFloors.ToString(), "Floors"));
            row.Add(Divider());
            row.Add(StatCol(s.perfectBlocks.ToString(), "Perfect"));
            row.Add(Divider());
            row.Add(StatCol(Format.DurationClock(s.durationSeconds), "Duration"));

            var dateWrap = Box().Flex(1.5f).AlignItems(Align.FlexEnd);
            dateWrap.Add(Text(Format.Date(s.date), Theme.FontXs, Theme.TextMuted));
            if (s.badges.Count > 0)
            {
                var badgeRow = Row().Wrap().Justify(Justify.FlexEnd);
                badgeRow.style.marginTop = 4;
                int shown = Mathf.Min(2, s.badges.Count);
                for (int i = 0; i < shown; i++)
                {
                    var badge = Box().Radius(Theme.RadiusPill);
                    badge.style.backgroundColor = Theme.Gold.WithAlpha(0.15f);
                    badge.style.paddingLeft = badge.style.paddingRight = 6;
                    badge.style.paddingTop = badge.style.paddingBottom = 2;
                    badge.style.marginLeft = 4; badge.style.marginBottom = 4;
                    badge.Add(Text(s.badges[i], 9, Theme.Gold, FontStyle.Bold));
                    badgeRow.Add(badge);
                }
                dateWrap.Add(badgeRow);
            }
            row.Add(dateWrap);
            card.Add(row);

            return card;
        }

        private VisualElement StatCol(string val, string label)
        {
            var col = Box().AlignItems(Align.Center).Flex(1);
            col.Add(Text(val, Theme.FontLg, Theme.TextPrimary, FontStyle.Bold));
            var l = Text(label, Theme.FontXs, Theme.TextMuted);
            l.style.marginTop = 2;
            col.Add(l);
            return col;
        }

        private VisualElement Divider()
        {
            var d = Box().Size(1, 30).Bg(Theme.TextPrimary.WithAlpha(0.1f));
            return d;
        }

        private VisualElement SectionLabel(string text)
        {
            var l = Text(text.ToUpperInvariant(), Theme.FontXs, Theme.TextSecondary, FontStyle.Bold);
            l.style.letterSpacing = 1.5f;
            l.style.marginBottom = Theme.SpaceSm;
            return l;
        }
    }
}
