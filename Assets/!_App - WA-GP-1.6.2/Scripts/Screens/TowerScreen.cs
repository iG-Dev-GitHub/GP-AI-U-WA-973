using BreathTower.App;
using BreathTower.Core;
using BreathTower.Data;
using BreathTower.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static BreathTower.UI.Widgets;

namespace BreathTower.Screens
{
    /// <summary>Full tower + session history. Ports app/tower.tsx.</summary>
    public sealed class TowerScreen : ScreenBase
    {
        public override VisualElement Build()
        {
            var root = Scaffold.Screen(App.Assets.towerLayout, out var content);
            content.Col();

            var data = SaveSystem.Data;
            int totalFloors = data.totalFloors;
            var sessions = data.sessions;
            int focusCount = 0;
            foreach (var s in sessions)
                foreach (var b in s.blocks)
                    if (b.type == "focus") focusCount++;

            // Header.
            var header = Row().AlignItems(Align.Center).Justify(Justify.SpaceBetween);
            header.PadH(Theme.SpaceMd); header.PadV(Theme.SpaceSm);
            header.Add(RoundButton("‹", () => App.Back()));
            header.Add(Text("Your Tower", Theme.FontXl, Theme.TextPrimary, FontStyle.Bold));
            var floorsBadge = Row();
            floorsBadge.style.alignItems = Align.FlexEnd;
            floorsBadge.style.backgroundColor = Theme.Surface;
            floorsBadge.style.paddingLeft = floorsBadge.style.paddingRight = Theme.SpaceSm;
            floorsBadge.style.paddingTop = floorsBadge.style.paddingBottom = Theme.SpaceXs;
            floorsBadge.Radius(Theme.RadiusPill);
            floorsBadge.Add(Text(totalFloors.ToString(), Theme.FontLg, Theme.Gold, FontStyle.Bold));
            floorsBadge.Add(Text(" fl", Theme.FontXs, Theme.TextMuted));
            header.Add(floorsBadge);
            content.Add(header);

            var scroll = VerticalScroll();
            scroll.style.flexGrow = 1;
            scroll.contentContainer.style.paddingLeft = Theme.SpaceMd;
            scroll.contentContainer.style.paddingRight = Theme.SpaceMd;
            scroll.contentContainer.style.paddingTop = Theme.SpaceXs;
            content.Add(scroll);
            var c = scroll.contentContainer;

            BuildHero(c, totalFloors, sessions.Count, focusCount);

            bool hasBlocks = totalFloors > 0;
            if (!hasBlocks)
            {
                c.Add(BuildEmpty());
            }
            else
            {
                var label = SectionLabel("Tower Blocks (newest on top)");
                c.Add(label);
                var towerContainer = Box();
                towerContainer.style.marginBottom = Theme.SpaceMd;
                foreach (var session in sessions)
                {
                    towerContainer.Add(BuildSessionDivider(session));
                    for (int i = session.blocks.Count - 1; i >= 0; i--)
                        towerContainer.Add(BuildTowerBlock(session.blocks[i], session.techniqueId));
                }
                c.Add(towerContainer);
            }

            if (sessions.Count > 0)
            {
                var histLabel = SectionLabel("Session History");
                histLabel.style.marginTop = Theme.SpaceXl;
                c.Add(histLabel);
                foreach (var session in sessions)
                    c.Add(BuildHistoryCard(session));
            }

            c.Add(Box().Height(Theme.SpaceXl));
            Widgets.LockScrollContent(scroll);
            return root;
        }

        private void BuildHero(VisualElement parent, int totalFloors, int sessionCount, int focusCount)
        {
            var hero = Box().AlignItems(Align.Center).Radius(Theme.RadiusXl).Pad(Theme.SpaceXl);
            hero.style.marginBottom = Theme.SpaceLg;
            hero.style.backgroundImage = new StyleBackground(Gradients.Vertical(Theme.Surface, Theme.Bg));

            hero.Add(Text(totalFloors.ToString(), Theme.FontHuge, Theme.TextPrimary, FontStyle.Bold));
            var l = Text("TOTAL FLOORS", Theme.FontSm, Theme.TextSecondary);
            l.style.letterSpacing = 1.5f;
            hero.Add(l);

            if (totalFloors > 0)
            {
                var sub = Row().AlignItems(Align.Center);
                sub.style.marginTop = Theme.SpaceSm;
                sub.Add(Text($"{sessionCount} session{(sessionCount != 1 ? "s" : "")}", Theme.FontXs, Theme.TextMuted));
                var d = Box().Size(3, 3).Radius(2).Bg(Theme.TextMuted);
                d.style.marginLeft = d.style.marginRight = Theme.SpaceSm;
                sub.Add(d);
                sub.Add(Text($"{focusCount} Focus Floors", Theme.FontXs, Theme.TextMuted));
                hero.Add(sub);
            }
            parent.Add(hero);
        }

        private VisualElement BuildEmpty()
        {
            var card = Box().AlignItems(Align.Center).Pad(Theme.SpaceXl).Bg(Theme.Surface).Radius(Theme.RadiusXl);
            card.style.marginBottom = Theme.SpaceLg;
            var e = Widgets.ConstructionGlyph(60, Theme.Crane, Theme.TechniqueBox);
            e.style.marginBottom = Theme.SpaceMd;
            card.Add(e);
            var t = Text("Your tower is empty", Theme.FontLg, Theme.TextPrimary, FontStyle.Bold);
            t.style.marginBottom = Theme.SpaceXs;
            card.Add(t);
            var sub = Text("Complete breathing sessions to build your tower floor by floor.", Theme.FontSm, Theme.TextSecondary).TextCenter();
            sub.style.marginBottom = Theme.SpaceLg;
            card.Add(sub);
            var btn = Box().Bg(Theme.Cyan).Radius(Theme.RadiusPill);
            btn.style.paddingTop = btn.style.paddingBottom = Theme.SpaceSm;
            btn.style.paddingLeft = btn.style.paddingRight = Theme.SpaceXl;
            var bl = Text("Start First Session", Theme.FontSm, Theme.Bg, FontStyle.Bold);
            bl.pickingMode = PickingMode.Ignore;
            btn.Add(bl);
            Widgets.MakeClickable(btn, () => App.Navigate(ScreenId.Technique));
            card.Add(btn);
            return card;
        }

        private VisualElement BuildSessionDivider(SessionRecord session)
        {
            Color tc = Theme.TechniqueColor(session.techniqueId);
            var row = Row().AlignItems(Align.Center);
            row.style.marginTop = Theme.SpaceSm; row.style.marginBottom = Theme.SpaceSm;

            var line1 = Box().Flex(1).Height(1).Bg(tc.WithAlpha(0.27f));
            line1.style.marginRight = Theme.SpaceSm;
            row.Add(line1);

            var tag = Box().Radius(Theme.RadiusPill);
            tag.style.backgroundColor = tc.WithAlpha(0.13f);
            tag.style.paddingLeft = tag.style.paddingRight = Theme.SpaceSm;
            tag.style.paddingTop = tag.style.paddingBottom = 3;
            tag.Add(Text($"{Format.Date(session.date)} · {session.totalFloors}f", Theme.FontXs, tc, FontStyle.Bold));
            row.Add(tag);

            var line2 = Box().Flex(1).Height(1).Bg(tc.WithAlpha(0.27f));
            line2.style.marginLeft = Theme.SpaceSm;
            row.Add(line2);
            return row;
        }

        private VisualElement BuildTowerBlock(BlockRecord block, string techniqueId)
        {
            Color color = block.type == "focus" || block.type == "perfect" ? Theme.BlockPerfect
                : block.type == "calm" ? Theme.BlockCalm
                : Theme.TechniqueColor(techniqueId);

            var el = Box().Radius(4).Center();
            el.style.height = block.type == "focus" ? 22 : 18;
            el.style.marginBottom = 2;
            el.style.backgroundColor = color;

            if (block.type == "focus")
                el.Add(Text($"* Focus Floor #{block.cycleNum}", 9, Theme.Bg, FontStyle.Bold));
            else if (block.type == "calm")
            {
                var stripe = Box().Absolute().Height(2).Radius(1).Bg(Theme.TextPrimary.WithAlpha(0.4f));
                stripe.style.left = Length.Percent(20); stripe.style.right = Length.Percent(20);
                stripe.style.top = Length.Percent(40);
                el.Add(stripe);
            }
            return el;
        }

        private VisualElement BuildHistoryCard(SessionRecord session)
        {
            Color tc = Theme.TechniqueColor(session.techniqueId);
            var card = Box().Bg(Theme.Surface).Radius(Theme.RadiusLg).Overflow(true);
            card.style.marginBottom = Theme.SpaceSm;

            var row = Row().AlignItems(Align.Center).Pad(Theme.SpaceMd);
            var bar = Box().Size(4, 40).Radius(2).Bg(tc);
            bar.style.marginRight = Theme.SpaceMd;
            row.Add(bar);

            var col = Box().Flex(1);
            col.Add(Text(Format.Date(session.date), Theme.FontXs, Theme.TextMuted));
            col.Add(Text(session.techniqueName, Theme.FontSm, Theme.TextPrimary, FontStyle.Bold));
            row.Add(col);

            var stats = Box().AlignItems(Align.FlexEnd);
            stats.Add(Text(session.totalFloors.ToString(), Theme.FontXl, Theme.TextPrimary, FontStyle.Bold));
            stats.Add(Text("floors", Theme.FontXs, Theme.TextMuted));
            row.Add(stats);
            card.Add(row);

            if (session.badges.Count > 0)
            {
                var badgeRow = Row().Wrap();
                badgeRow.style.paddingLeft = badgeRow.style.paddingRight = Theme.SpaceMd;
                badgeRow.style.paddingBottom = Theme.SpaceSm;
                foreach (var badge in session.badges)
                {
                    var b = Box().Radius(Theme.RadiusPill);
                    b.style.backgroundColor = Theme.Gold.WithAlpha(0.1f);
                    b.style.paddingLeft = b.style.paddingRight = Theme.SpaceSm;
                    b.style.paddingTop = b.style.paddingBottom = 3;
                    b.style.marginRight = Theme.SpaceXs; b.style.marginBottom = Theme.SpaceXs;
                    b.Add(Text(badge, 10, Theme.Gold, FontStyle.Bold));
                    badgeRow.Add(b);
                }
                card.Add(badgeRow);
            }
            return card;
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
