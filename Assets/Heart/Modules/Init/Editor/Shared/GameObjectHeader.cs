﻿using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Shared.EditorOnly
{
    /// <summary>
    /// A method that is called before or after the header GUI of a component is drawn.
    /// </summary>
    /// <param name="targets"> The game objects whose headers are being drawn. </param>
    /// <param name="headerRect"> The position and dimensions of the header GUI. </param>
    /// <param name="headerIsSelected"> Is the header currently selected in the Inspector or not? </param>
    /// <param name="supportsRichText"> Does the header that is being drawn support rich text tags or not? </param>
    /// <returns> The height of the element that this method has drawn before or after the header GUI. </returns>
    public delegate void GameObjectHeaderGUIHandler([DisallowNull] GameObject[] targets, Rect headerRect, bool headerIsSelected, bool supportsRichText);
	
    /// <summary>
    /// A class that contains callbacks
    /// </summary>
    public static class GameObjectHeader
    {
        /// <summary>
        /// Callback invoked right before component header GUI is drawn.
        /// </summary>
        public static event GameObjectHeaderGUIHandler BeforeHeaderGUI;

        /// <summary>
        /// Callback invoked right after component header GUI has been drawn.
        /// </summary>
        public static event GameObjectHeaderGUIHandler AfterHeaderGUI;

        internal static void InvokeBeforeHeaderGUI([DisallowNull] GameObject[] targets, Rect headerRect, bool headerIsSelected, bool supportsRichText)
            => BeforeHeaderGUI?.Invoke(targets, headerRect, headerIsSelected, supportsRichText);

        internal static void InvokeAfterHeaderGUI([DisallowNull] GameObject[] targets, Rect headerRect, bool headerIsSelected, bool supportsRichText)
            => AfterHeaderGUI?.Invoke(targets, headerRect, headerIsSelected, supportsRichText);
    }
}