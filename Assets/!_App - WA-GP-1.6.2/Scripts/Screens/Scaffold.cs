using BreathTower.App;
using BreathTower.Core;
using BreathTower.UI;
using UnityEngine.UIElements;

namespace BreathTower.Screens
{
    /// <summary>
    /// Shared screen scaffolding. Instantiates the screen's UXML layout (its
    /// declarative root), applies the night-sky gradient, and returns the
    /// container that screen content should be added to.
    /// </summary>
    public static class Scaffold
    {
        /// <summary>
        /// Builds the screen root from a UXML layout. The returned
        /// <paramref name="content"/> is where the screen adds its children.
        /// </summary>
        public static VisualElement Screen(VisualTreeAsset layout, out VisualElement content)
        {
            VisualElement root;
            if (layout != null)
            {
                var tpl = layout.Instantiate();
                tpl.style.flexGrow = 1;
                root = tpl;
            }
            else
            {
                root = new VisualElement();
                root.style.flexGrow = 1;
            }

            // The screen content area: a vertically-filled, gradient-backed box.
            content = root.Q<VisualElement>("screen") ?? root;
            content.style.flexGrow = 1;
            content.style.backgroundImage = new StyleBackground(
                Gradients.Vertical(Theme.GradientTop, Theme.GradientMid));

            return root;
        }
    }
}
