﻿using Pancake.AttributeDrawer;
using UnityEditor;
using UnityEngine;

[assembly: RegisterAttributeDrawer(typeof(LabelWidthDrawer), DrawerOrder.Decorator)]

namespace Pancake.AttributeDrawer
{
    public class LabelWidthDrawer : AttributeDrawer<LabelWidthAttribute>
    {
        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = Attribute.Width;
            next.OnGUI(position);
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}