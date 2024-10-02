﻿using Pancake;
using Pancake.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Editor = PancakeEditor.Common.Editor;

namespace PancakeEditor
{
    [CustomPropertyDrawer(typeof(PagePickupAttribute))]
    internal sealed class PagePickupDrawer : PropertyDrawer
    {
        private string _selectedId;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width * 0.4f, position.height);
            var buttonRect = new Rect(position.x + position.width * 0.4f, position.y, position.width * 0.6f, position.height);

            EditorGUI.LabelField(labelRect, ObjectNames.NicifyVariableName(property.name.ToCamelCase()));

            _selectedId = string.IsNullOrEmpty(property.stringValue) ? "Select type..." : property.stringValue;

            var buttonColor = string.IsNullOrEmpty(property.stringValue) ? Uniform.Error : new Color(0f, 0.18f, 0.53f, 0.31f);
            var originalColor = GUI.backgroundColor;

            GUI.backgroundColor = buttonColor;

            if (GUI.Button(buttonRect, _selectedId))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("None (-1)"), string.IsNullOrEmpty(property.stringValue), () => { SetAndApplyProperty(property, string.Empty); });

                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = GetSubClasses(type);
                    for (var i = 0; i < result.Count; i++)
                    {
                        int cacheIndex = i;
                        bool isSelected = property.stringValue == result[cacheIndex].Name;
                        menu.AddItem(new GUIContent(result[cacheIndex].Name), isSelected, () => { SetAndApplyProperty(property, result[cacheIndex].Name); });
                    }
                }

                menu.DropDown(new Rect(Editor.CurrentEvent.MousePosition, Vector2.zero));
            }

            GUI.backgroundColor = originalColor;

            EditorGUI.EndProperty();
        }

        private List<Type> GetSubClasses(Type baseType) { return baseType.GetAllSubClass<Popup>().Filter(t => !t.Name.Equals("Page`1")); }

        private Type GetTypeByFullName()
        {
            TypeExtensions.FindTypeByFullName("Pancake.UI.Page", out var type);
            return type;
        }

        private void SetAndApplyProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
            _selectedId = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}