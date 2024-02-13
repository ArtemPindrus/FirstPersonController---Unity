using System;
using UnityEngine;

namespace Lerping {
    public class Lerp : MonoBehaviour {
        public event Action LerpFinished;
        protected Func<float, float> lerpingFunction;

        protected float lerpingTime;
        protected float elapsedTime;

        protected bool toDispose;

        protected void Update() {
            float percent = elapsedTime / lerpingTime;
            float t = lerpingFunction(percent);

            SettingValues(t);

            if (elapsedTime == lerpingTime) {
                LerpFinished?.Invoke();

                if (toDispose) Dispose();
            }

            elapsedTime += Time.deltaTime;
            elapsedTime = Mathf.Clamp(elapsedTime, 0, lerpingTime);
        }

        protected virtual void SettingValues(float t) {
            throw new NotImplementedException();
        }

        protected void Dispose() {
            

            LerpFinished = null;
            Destroy(gameObject);
        }

    }
}
