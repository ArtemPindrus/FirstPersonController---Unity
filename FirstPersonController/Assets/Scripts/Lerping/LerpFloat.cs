using System;
using UnityEngine;

namespace Lerping {
    public class LerpFloat : Lerp {
        private Action<float> setter;
        private float start;
        private float end;

        public static LerpFloat Initialize(float start, float end, Action<float> setter, float lerpingTime, bool toDispose = true, Func<float, float> lerpingFunc = null) {
            GameObject executer = new($"Lerp Executer");
            LerpFloat lm = executer.AddComponent<LerpFloat>();

            lm.start = start;
            lm.end = end;
            lm.lerpingTime = lerpingTime;
            lm.setter = setter;
            lm.lerpingFunction = lerpingFunc ?? (x => x * x * (3f - 2f * x));
            lm.toDispose = toDispose;

            return lm;
        }

        protected override void SettingValues(float t) {
            setter.Invoke(Mathf.Lerp(start, end, t));
        }
    }
}
