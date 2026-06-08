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
    /// <summary>Onboarding carousel. Ports app/welcome.tsx.</summary>
    public sealed class WelcomeScreen : ScreenBase
    {
        private struct Slide
        {
            public string Kind, Title, Subtitle, Body;
        }

        private static readonly Slide[] Slides =
        {
            new Slide { Kind = "tower", Title = "Choose a breathing rhythm", Subtitle = "raise the tower",
                Body = "Pick from 4-7-8, Box Breathing, Equal Breathing, or create your own custom rhythm." },
            new Slide { Kind = "ring", Title = "Breathe in sync", Subtitle = "stack perfect blocks",
                Body = "Follow the breathing guide and complete each cycle to add a glowing block to your tower." },
            new Slide { Kind = "star", Title = "Stay focused", Subtitle = "reach the peak",
                Body = "Build your tower higher with every session. Earn badges, unlock Focus Floors, and track your journey." },
        };

        private int _active;
        private VisualElement _slideHost;
        private readonly List<VisualElement> _dots = new List<VisualElement>();
        private Label _btnLabel;

        public override VisualElement Build()
        {
            var root = Scaffold.Screen(App.Assets.welcomeLayout, out var content);
            content.Col();

            AddStars(content);

            // Slide area (fills available space).
            _slideHost = Box().Flex(1).Center();
            content.Add(_slideHost);

            // Dots.
            var dots = Row().Center().MarginBottom(Theme.SpaceLg);
            for (int i = 0; i < Slides.Length; i++)
            {
                int idx = i;
                var dot = Box().Height(8).Radius(4).Bg(Theme.TextSecondary.WithAlpha(0.3f));
                dot.style.width = 8;
                dot.style.marginLeft = dot.style.marginRight = 4;
                Widgets.MakeClickable(dot, () => Show(idx));
                _dots.Add(dot);
                dots.Add(dot);
            }
            content.Add(dots);

            // Button.
            var btn = GradientButton("Next →", OnNext);
            btn.style.marginLeft = btn.style.marginRight = Theme.SpaceXl;
            btn.style.marginBottom = Theme.SpaceXl;
            _btnLabel = btn.Q<Label>();
            content.Add(btn);

            Show(0);
            EnableSwipe(_slideHost);
            return root;
        }

        private void AddStars(VisualElement parent)
        {
            var positions = new (float x, float y)[]
            {
                (25,20),(70,45),(130,18),(190,55),(240,28),(300,42),
                (350,15),(60,80),(160,70),(280,68),(360,38),(330,72),
            };
            foreach (var (x, y) in positions)
            {
                var star = Box().Absolute().Pos(x, y).Size(3, 3).Radius(2)
                    .Bg(Theme.TextPrimary.WithAlpha(0.7f)).NoPick();
                parent.Add(star);
            }
        }

        private void Show(int index)
        {
            _active = Mathf.Clamp(index, 0, Slides.Length - 1);
            _slideHost.Clear();
            _slideHost.Add(BuildSlide(Slides[_active]));

            for (int i = 0; i < _dots.Count; i++)
            {
                bool on = i == _active;
                _dots[i].style.backgroundColor = on ? Theme.Cyan : Theme.TextSecondary.WithAlpha(0.3f);
                _dots[i].style.width = on ? 24 : 8;
            }

            _btnLabel.text = _active < Slides.Length - 1 ? "Next →" : "Start Breathing";
        }

        private VisualElement BuildSlide(Slide slide)
        {
            var s = Box().Center().PadH(Theme.SpaceXl);
            s.style.paddingBottom = 80;

            var emojiWrap = Box().Center().MarginBottom(Theme.SpaceXl);
            // Drawn icon per slide (font-independent): tower / ring / star.
            VisualElement icon = slide.Kind == "ring" ? Widgets.RingGlyph(72)
                : slide.Kind == "star" ? (VisualElement)Text("*", 72, Theme.Gold, FontStyle.Bold).TextCenter()
                : Widgets.TowerGlyph(70);
            icon.style.marginBottom = Theme.SpaceMd;
            emojiWrap.Add(icon);

            // Decorative tower preview (5 blocks).
            var preview = Box().Center();
            var colors = new[] { Theme.TechniqueBox, Theme.TechniqueBox, Theme.BlockCalm, Theme.TechniqueBox, Theme.BlockPerfect };
            for (int i = 0; i < 5; i++)
            {
                var blk = Box().Size(80, 14).Radius(4).Bg(colors[i]).Opacity(0.5f + i * 0.1f);
                blk.style.marginBottom = 3;
                preview.Add(blk);
            }
            emojiWrap.Add(preview);
            s.Add(emojiWrap);

            var textWrap = Box().Center();
            var title = Text(slide.Title, Theme.FontXxl, Theme.TextPrimary, FontStyle.Bold).TextCenter();
            title.style.letterSpacing = -0.5f;
            title.style.marginBottom = Theme.SpaceSm;

            var subtitle = Text($"— {slide.Subtitle} —", Theme.FontMd, Theme.Cyan, FontStyle.Bold).TextCenter();
            subtitle.style.letterSpacing = 1;
            subtitle.style.marginBottom = Theme.SpaceSm;

            var body = Text(slide.Body, Theme.FontMd, Theme.TextSecondary).TextCenter();
            body.style.marginTop = Theme.SpaceSm;
            body.style.maxWidth = 360;

            textWrap.Add(title);
            textWrap.Add(subtitle);
            textWrap.Add(body);
            s.Add(textWrap);
            return s;
        }

        private void EnableSwipe(VisualElement target)
        {
            float startX = 0;
            bool dragging = false;
            target.RegisterCallback<PointerDownEvent>(e => { startX = e.position.x; dragging = true; });
            target.RegisterCallback<PointerUpEvent>(e =>
            {
                if (!dragging) return;
                dragging = false;
                float dx = e.position.x - startX;
                if (dx < -40 && _active < Slides.Length - 1) Show(_active + 1);
                else if (dx > 40 && _active > 0) Show(_active - 1);
            });
        }

        private void OnNext()
        {
            if (_active < Slides.Length - 1)
            {
                Show(_active + 1);
            }
            else
            {
                SaveSystem.MarkTutorialSeen();
                App.Replace(ScreenId.Home);
            }
        }
    }
}
