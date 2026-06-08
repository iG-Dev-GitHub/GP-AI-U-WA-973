using UnityEngine;
using UnityEngine.UIElements;

namespace BreathTower.App
{
    /// <summary>
    /// Strongly-typed registry of every asset the app needs at runtime.
    /// Replaces all Resources.Load / string-path lookups: assets are wired
    /// through serialized references in the inspector, so the build keeps
    /// working after asset renaming and code/asset obfuscation.
    /// </summary>
    [CreateAssetMenu(menuName = "Breath Tower/App Assets", fileName = "AppAssets")]
    public sealed class AppAssets : ScriptableObject
    {
        [Header("Fonts")]
        [Tooltip("Primary UI font (TrueType). Rendered dynamically at runtime.")]
        public Font primaryFont;

        [Tooltip("Optional fallback font used for glyphs the primary font lacks (e.g. emoji).")]
        public Font fallbackFont;

        [Header("Styles")]
        [Tooltip("Shared USS stylesheet applied to the root panel.")]
        public StyleSheet appStyleSheet;

        [Header("Screen layouts (UXML)")]
        public VisualTreeAsset welcomeLayout;
        public VisualTreeAsset homeLayout;
        public VisualTreeAsset techniqueLayout;
        public VisualTreeAsset sessionLayout;
        public VisualTreeAsset summaryLayout;
        public VisualTreeAsset towerLayout;
        public VisualTreeAsset settingsLayout;
    }
}
