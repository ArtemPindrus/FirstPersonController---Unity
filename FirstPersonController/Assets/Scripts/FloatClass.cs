using UnityEngine;

namespace Assets.Scripts.FirstPersonPlayer {
    public class FloatClass {
        public float Value { get; private set; }

        public FloatClass(float value) { 
            Value = value;
        }

        public void Change(float newValue) {
            Value = newValue;
        }
    }
}
