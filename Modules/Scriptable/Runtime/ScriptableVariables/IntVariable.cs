﻿using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_int.asset", menuName = "Pancake/Scriptable/ScriptableVariables/int")]
    [EditorIcon("scriptable_variable")]
    public class IntVariable : ScriptableVariable<int>
    {
        [SerializeField] private bool _isClamped = false;
        public bool IsClamped => _isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField] [ShowIf("_isClamped", true)]
        private Vector2Int _minMax = new Vector2Int(int.MinValue, int.MaxValue);

        public Vector2Int MinMax { get => _minMax; set => _minMax = value; }

        public override void Save()
        {
            Data.Save(Guid, Value);
            base.Save();
        }

        public override void Load()
        {
            Value = Data.Load(Guid, InitialValue);
            base.Load();
        }

        public void Add(int value) { Value += value; }

        public override int Value
        {
            get => _value;
            set
            {
                var clampedValue = IsClamped ? Mathf.Clamp(value, _minMax.x, _minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (IsClamped)
            {
                var clampedValue = Mathf.Clamp(_value, _minMax.x, _minMax.y);
                if (_value < clampedValue || _value > clampedValue)
                    _value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}