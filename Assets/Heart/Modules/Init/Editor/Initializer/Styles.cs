﻿using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	internal static class Styles
	{
		internal readonly static GUIStyle RefTag;
		internal readonly static GUIStyle ServiceTag;
		internal readonly static GUIStyle ValueProvider;
		internal readonly static GUIStyle Discard;
		internal readonly static GUIContent AddButtonIcon;
		internal readonly static GUIStyle AddButtonStyle;
		internal readonly static GUIStyle AddButtonBackground;

		static Styles()
		{
			ServiceTag = new GUIStyle("AssetLabel");
			ServiceTag.contentOffset = new Vector2(0f, -1f);
			ValueProvider = ServiceTag;
			RefTag = ServiceTag;
			Discard = new GUIStyle("SearchCancelButton");
			Discard.alignment = TextAnchor.MiddleRight;
			AddButtonIcon = EditorGUIUtility.TrIconContent("Toolbar Plus");
			AddButtonStyle = "RL FooterButton";
			AddButtonBackground = "RL Footer";
		}
	}
}