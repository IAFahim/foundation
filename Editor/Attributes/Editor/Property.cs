﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Pancake.AttributeDrawer
{
    public sealed class Property
    {
        private static readonly IReadOnlyList<ValidationResult> EmptyValidationResults = new List<ValidationResult>();

        private readonly PropertyDefinition _definition;
        private readonly int _propertyIndex;
        [CanBeNull] private readonly SerializedObject _serializedObject;
        [CanBeNull] private readonly SerializedProperty _serializedProperty;
        private List<Property> _childrenProperties;
        private List<ValidationResult> _validationResults;

        private GUIContent _displayNameBackingField;

        private string _isExpandedPrefsKey;

        private int _lastUpdateFrame;
        private bool _isUpdating;

        [CanBeNull] private object _value;
        [CanBeNull] private Type _valueType;
        private bool _isValueMixed;

        public event Action<Property> ValueChanged;
        public event Action<Property> ChildValueChanged;
        
        internal PropertyDefinition Definition => _definition;

        internal Property(PropertyTree propertyTree, Property parent, PropertyDefinition definition, SerializedObject serializedObject)
        {
            Parent = parent;
            _definition = definition;
            _propertyIndex = -1;
            _serializedProperty = null;
            _serializedObject = serializedObject;

            PropertyTree = propertyTree;
            PropertyType = GetPropertyType(this);
        }

        internal Property(PropertyTree propertyTree, Property parent, PropertyDefinition definition, int propertyIndex, [CanBeNull] SerializedProperty serializedProperty)
        {
            Parent = parent;
            _definition = definition;
            _propertyIndex = propertyIndex;
            _serializedProperty = serializedProperty?.Copy();
            _serializedObject = _serializedProperty?.serializedObject;

            PropertyTree = propertyTree;
            PropertyType = GetPropertyType(this);
        }

        [PublicAPI] public EPropertyType PropertyType { get; }

        [PublicAPI] public PropertyTree PropertyTree { get; }

        [PublicAPI] public Property Parent { get; }

        [PublicAPI] public Property Owner => IsArrayElement ? Parent.Owner : Parent;

        [PublicAPI] public bool IsRootProperty => Parent == null;

        [PublicAPI] public string RawName => _definition.Name;

        [PublicAPI] public string DisplayName => DisplayNameContent.text;

        public IEqualityComparer Comparer => InspectorEqualityComparer.Of(ValueType);

        [PublicAPI] public GUIContent DisplayNameContent
        {
            get
            {
                if (PropertyOverrideContext.Current != null && PropertyOverrideContext.Current.TryGetDisplayName(this, out var overrideName))
                {
                    return overrideName;
                }

                if (_displayNameBackingField == null)
                {
                    if (TryGetAttribute(out HideLabelAttribute _) || IsArrayElement)
                    {
                        _displayNameBackingField = new GUIContent("");
                    }
                    else
                    {
                        _displayNameBackingField = new GUIContent(ObjectNames.NicifyVariableName(_definition.Name));
                    }
                }

                if (IsArrayElement)
                {
                    if (UnityInspectorUtilities.TryGetSpecialArrayElementName(this, out string specialName))
                    {
                        _displayNameBackingField.text = specialName;
                    }
                }
                else
                {
                    if (_definition.CustomLabel != null)
                    {
                        _displayNameBackingField.text = _definition.CustomLabel.GetValue(this, "");
                    }

                    if (_definition.CustomTooltip != null)
                    {
                        _displayNameBackingField.tooltip = _definition.CustomTooltip.GetValue(this, "");
                    }
                }

                return _displayNameBackingField;
            }
        }

        [PublicAPI] public bool IsVisible
        {
            get
            {
                foreach (var processor in _definition.HideProcessors)
                {
                    if (processor.IsHidden(this))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [PublicAPI] public bool IsEnabled
        {
            get
            {
                if (_definition.IsReadOnly)
                {
                    return false;
                }

                foreach (var processor in _definition.DisableProcessors)
                {
                    if (processor.IsDisabled(this))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [PublicAPI] public Type FieldType => _definition.FieldType;

        [PublicAPI] public Type ArrayElementType => _definition.ArrayElementType;

        [PublicAPI] public bool IsArrayElement => _definition.IsArrayElement;

        [PublicAPI] public bool IsArray => _definition.IsArray;

        public int IndexInArray => IsArrayElement ? _propertyIndex : throw new InvalidOperationException("Cannot read IndexInArray for !IsArrayElement");

        public IReadOnlyList<CustomDrawer> AllDrawers => _definition.Drawers;

        internal IReadOnlyList<string> ExtensionErrors => _definition.ExtensionErrors;

        public bool HasValidators => _definition.Validators.Count != 0;

        public IReadOnlyList<ValidationResult> ValidationResults => _validationResults ?? EmptyValidationResults;

        [PublicAPI] public bool IsExpanded
        {
            get
            {
                if (_serializedProperty != null)
                {
                    return _serializedProperty.isExpanded;
                }

                if (_isExpandedPrefsKey == null)
                {
                    _isExpandedPrefsKey = $"TriInspector.expanded.{PropertyTree.TargetObjectType}.{RawName}";
                }

                return EditorPrefs.GetBool(_isExpandedPrefsKey, false);
            }
            set
            {
                if (IsExpanded == value)
                {
                    return;
                }

                if (_serializedProperty != null)
                {
                    _serializedProperty.isExpanded = value;
                }
                else if (_isExpandedPrefsKey != null)
                {
                    EditorPrefs.SetBool(_isExpandedPrefsKey, value);
                }
            }
        }

        [PublicAPI] [CanBeNull] public Type ValueType
        {
            get
            {
                if (PropertyType != EPropertyType.Reference)
                {
                    return _definition.FieldType;
                }

                UpdateIfRequired();
                return _valueType;
            }
        }

        public bool IsValueMixed
        {
            get
            {
                if (PropertyTree.TargetsCount == 1)
                {
                    return false;
                }

                UpdateIfRequired();
                return _isValueMixed;
            }
        }


        [PublicAPI] [CanBeNull] public object Value
        {
            get
            {
                UpdateIfRequired();
                return _value;
            }
        }

        [PublicAPI] public IReadOnlyList<Property> ChildrenProperties
        {
            get
            {
                if (_childrenProperties != null && PropertyType == EPropertyType.Generic)
                {
                    return _childrenProperties;
                }

                UpdateIfRequired();

                return PropertyType == EPropertyType.Generic || PropertyType == EPropertyType.Reference
                    ? _childrenProperties
                    : throw new InvalidOperationException("Cannot read ChildrenProperties for " + PropertyType);
            }
        }

        [PublicAPI] public IReadOnlyList<Property> ArrayElementProperties
        {
            get
            {
                UpdateIfRequired();

                return PropertyType == EPropertyType.Array
                    ? _childrenProperties
                    : throw new InvalidOperationException("Cannot read ArrayElementProperties for " + PropertyType);
            }
        }

        [PublicAPI]
        public bool TryGetMemberInfo(out MemberInfo memberInfo) { return _definition.TryGetMemberInfo(out memberInfo); }

        public object GetValue(int targetIndex) { return _definition.GetValue(this, targetIndex); }

        [PublicAPI]
        public void SetValue(object value) { ModifyAndRecordForUndo(targetIndex => SetValueRecursive(this, value, targetIndex)); }

        [PublicAPI]
        public void SetValues(Func<int, object> getValue)
        {
            ModifyAndRecordForUndo(targetIndex =>
            {
                var value = getValue.Invoke(targetIndex);
                SetValueRecursive(this, value, targetIndex);
            });
        }

        public void ModifyAndRecordForUndo(Action<int> call)
        {
            PropertyTree.ApplyChanges();

            PropertyTree.ForceCreateUndoGroup();

            for (var targetIndex = 0; targetIndex < PropertyTree.TargetsCount; targetIndex++)
            {
                call.Invoke(targetIndex);
            }

            PropertyTree.Update(forceUpdate: true);

            NotifyValueChanged();

            PropertyTree.RequestValidation();
            PropertyTree.RequestRepaint();
        }

        public void NotifyValueChanged() { NotifyValueChanged(this); }

        private void NotifyValueChanged(Property property)
        {
            if (property == this)
            {
                ValueChanged?.Invoke(property);
            }
            else
            {
                ChildValueChanged?.Invoke(property);
            }

            Parent?.NotifyValueChanged(property);
        }

        private void UpdateIfRequired(bool forceUpdate = false)
        {
            if (_isUpdating)
            {
                throw new InvalidOperationException("Recursive call detected");
            }

            if (_lastUpdateFrame == PropertyTree.RepaintFrame && !forceUpdate)
            {
                return;
            }

            _isUpdating = true;

            try
            {
                _lastUpdateFrame = PropertyTree.RepaintFrame;

                ReadValue(this, out var newValue, out var newValueIsMixed);

                var newValueType = FieldType.IsValueType ? FieldType : ReferenceEquals(_value, newValue) ? _valueType : newValue?.GetType();
                var valueTypeChanged = _valueType != newValueType;

                _value = newValue;
                _valueType = newValueType;
                _isValueMixed = newValueIsMixed;

                switch (PropertyType)
                {
                    case EPropertyType.Generic:
                    case EPropertyType.Reference:
                        if (_childrenProperties == null || valueTypeChanged)
                        {
                            if (_childrenProperties == null)
                            {
                                _childrenProperties = new List<Property>();
                            }

                            _childrenProperties.Clear();

                            var selfType = PropertyType == EPropertyType.Reference ? _valueType : FieldType;
                            if (selfType != null)
                            {
                                var properties = TypeDefinition.GetCached(selfType).Properties;
                                for (var index = 0; index < properties.Count; index++)
                                {
                                    var childDefinition = properties[index];
                                    var childSerializedProperty = _serializedProperty != null
                                        ? _serializedProperty.FindPropertyRelative(childDefinition.Name)
                                        : _serializedObject?.FindProperty(childDefinition.Name);
                                    var childProperty = new Property(PropertyTree,
                                        this,
                                        childDefinition,
                                        index,
                                        childSerializedProperty);

                                    _childrenProperties.Add(childProperty);
                                }
                            }
                        }

                        break;

                    case EPropertyType.Array:
                        if (_childrenProperties == null)
                        {
                            _childrenProperties = new List<Property>();
                        }

                        var listSize = ((IList) newValue)?.Count ?? 0;

                        while (_childrenProperties.Count < listSize)
                        {
                            var index = _childrenProperties.Count;
                            var elementDefinition = _definition.ArrayElementDefinition;
                            var elementSerializedReference = _serializedProperty?.GetArrayElementAtIndex(index);

                            var elementProperty = new Property(PropertyTree,
                                this,
                                elementDefinition,
                                index,
                                elementSerializedReference);

                            _childrenProperties.Add(elementProperty);
                        }

                        while (_childrenProperties.Count > listSize)
                        {
                            _childrenProperties.RemoveAt(_childrenProperties.Count - 1);
                        }

                        break;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        internal void RunValidation()
        {
            UpdateIfRequired();

            if (HasValidators)
            {
                _validationResults = _definition.Validators.Select(it => it.Validate(this)).Where(it => !it.IsValid).ToList();
            }

            if (_childrenProperties != null)
            {
                foreach (var childrenProperty in _childrenProperties)
                {
                    childrenProperty.RunValidation();
                }
            }
        }

        internal void EnumerateValidationResults(Action<Property, ValidationResult> call)
        {
            UpdateIfRequired();

            if (_validationResults != null)
            {
                foreach (var result in _validationResults)
                {
                    call.Invoke(this, result);
                }
            }

            if (_childrenProperties != null)
            {
                foreach (var childrenProperty in _childrenProperties)
                {
                    childrenProperty.EnumerateValidationResults(call);
                }
            }
        }

        [PublicAPI]
        public bool TryGetSerializedProperty(out SerializedProperty serializedProperty)
        {
            serializedProperty = _serializedProperty;
            return serializedProperty != null;
        }

        [PublicAPI]
        public bool TryGetAttribute<TAttribute>(out TAttribute attribute) where TAttribute : Attribute
        {
            if (ValueType != null)
            {
                foreach (var attr in ReflectionUtilities.GetAttributesCached(ValueType))
                {
                    if (attr is TAttribute typedAttr)
                    {
                        attribute = typedAttr;
                        return true;
                    }
                }
            }

            foreach (var attr in _definition.Attributes)
            {
                if (attr is TAttribute typedAttr)
                {
                    attribute = typedAttr;
                    return true;
                }
            }

            attribute = null;
            return false;
        }

        private static void SetValueRecursive(Property property, object value, int targetIndex)
        {
            // for value types we must recursively set all parent objects
            // because we cannot directly modify structs
            // but we can re-set entire parent value
            while (property._definition.SetValue(property, value, targetIndex, out var parentValue) && property.Parent != null)
            {
                property = property.Parent;
                value = parentValue;
            }
        }

        private static void ReadValue(Property property, out object newValue, out bool isMixed)
        {
            newValue = property.GetValue(0);

            if (property.PropertyTree.TargetsCount == 1)
            {
                isMixed = false;
                return;
            }

            switch (property.PropertyType)
            {
                case EPropertyType.Array:
                {
                    var list = (IList) newValue;
                    for (var i = 1; i < property.PropertyTree.TargetsCount; i++)
                    {
                        if (list == null)
                        {
                            break;
                        }

                        var otherList = (IList) property.GetValue(i);
                        if (otherList == null || otherList.Count < list.Count)
                        {
                            newValue = list = otherList;
                        }
                    }

                    isMixed = true;
                    return;
                }
                case EPropertyType.Reference:
                {
                    for (var i = 1; i < property.PropertyTree.TargetsCount; i++)
                    {
                        var otherValue = property.GetValue(i);

                        if (newValue?.GetType() != otherValue?.GetType())
                        {
                            isMixed = true;
                            newValue = null;
                            return;
                        }
                    }

                    isMixed = false;
                    return;
                }
                case EPropertyType.Generic:
                {
                    isMixed = false;
                    return;
                }
                case EPropertyType.Primitive:
                {
                    for (var i = 1; i < property.PropertyTree.TargetsCount; i++)
                    {
                        var otherValue = property.GetValue(i);
                        if (!property.Comparer.Equals(otherValue, newValue))
                        {
                            isMixed = true;
                            return;
                        }
                    }

                    isMixed = false;
                    return;
                }

                default:
                {
                    Debug.LogError($"Unexpected property type: {property.PropertyType}");
                    isMixed = true;
                    return;
                }
            }
        }

        private static EPropertyType GetPropertyType(Property property)
        {
            if (property._serializedProperty != null)
            {
                if (property._serializedProperty.isArray && property._serializedProperty.propertyType != SerializedPropertyType.String)
                {
                    return EPropertyType.Array;
                }

                if (property._serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    return EPropertyType.Reference;
                }

                if (property._serializedProperty.propertyType == SerializedPropertyType.Generic)
                {
                    return EPropertyType.Generic;
                }

                return EPropertyType.Primitive;
            }

            if (property._serializedObject != null)
            {
                return EPropertyType.Generic;
            }

            if (property._definition.FieldType.IsPrimitive || property._definition.FieldType == typeof(string) ||
                typeof(UnityEngine.Object).IsAssignableFrom(property._definition.FieldType))
            {
                return EPropertyType.Primitive;
            }

            if (property._definition.FieldType.IsValueType)
            {
                return EPropertyType.Generic;
            }

            if (property._definition.IsArray)
            {
                return EPropertyType.Array;
            }

            return EPropertyType.Reference;
        }
    }

    public enum EPropertyType
    {
        Array,
        Reference,
        Generic,
        Primitive,
    }
}