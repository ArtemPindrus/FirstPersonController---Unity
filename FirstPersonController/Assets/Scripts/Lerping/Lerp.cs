using System;
using UnityEngine;

namespace Lerping {
    public enum LerpDirection { Addition, Substraction, Inactive }

    public class Lerp : MonoBehaviour {
        public event Action LerpFinished;
        protected Func<float, float> lerpingFunction;

        protected float lerpingTime;
        protected float elapsedTime;

        protected bool toDispose;
        protected LerpDirection lerpDirection;

        protected void Update() {
            float percent = elapsedTime / lerpingTime;
            float t = lerpingFunction(percent);

            if (lerpDirection != LerpDirection.Inactive) SettingValues(t);

            if (elapsedTime == lerpingTime) {
                LerpFinished?.Invoke();

                if (toDispose) Dispose();
            }

            if (lerpDirection == LerpDirection.Addition) elapsedTime += Time.deltaTime;
            else if (lerpDirection == LerpDirection.Substraction) elapsedTime -= Time.deltaTime;


            elapsedTime = Mathf.Clamp(elapsedTime, 0, lerpingTime);
        }

        public void SetDirection(LerpDirection direction) { 
            lerpDirection = direction;
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
