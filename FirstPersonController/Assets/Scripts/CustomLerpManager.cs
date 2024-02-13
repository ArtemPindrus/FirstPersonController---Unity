using UnityEngine;
using System.Collections.Generic;

public class CustomLerpManager : MonoBehaviour
{
    [SerializeField] private int count;
    private readonly List<LerpingSingle> lerpings = new();

    public LerpingSingle LerpTo(Transform objTransform, Transform targetTransform, float lerpingTime) {
        LerpingSingle newLerp = new LerpingTransforms(objTransform, targetTransform, lerpingTime);
        lerpings.Add(newLerp);
        return newLerp;
    }

    public LerpingSingle LerpTo(Transform objTransform, Quaternion targetLocRot, float lerpingTime) {
        LerpingSingle newLerp = new LerpingRotations(objTransform, targetLocRot, lerpingTime);
        lerpings.Add(newLerp);
        return newLerp;
    }

    public void Update() {
        for (int i = 0; i < lerpings.Count; i++) {
            if (lerpings[i].Update()) {
                lerpings.RemoveAt(i);
                i--;
            }
        }

        count = lerpings.Count;
    }

    public class LerpingSingle {
        protected readonly float lerpingTime;
        protected float elapsedTime;

        public LerpingSingle(float lerpingTime) {
            this.lerpingTime = lerpingTime;
        }

        public virtual bool Update() {
            if (elapsedTime == lerpingTime) {
                OnLerpFinished?.Invoke();
                OnLerpFinished = null;
                return true;
            }
            return false;
        }

        public delegate void LerpingEvents();
        public event LerpingEvents OnLerpFinished;
    }

    private class LerpingTransforms : LerpingSingle {
        private readonly Transform objTransform;

        private readonly Quaternion startingLocRot;
        private readonly Vector3 startingLocPos;
        private readonly Quaternion targetLocRot;
        private readonly Vector3 targetLocPos;

        public LerpingTransforms(Transform objTransform, Transform targetTransform, float lerpingTime) : base(lerpingTime) { 
            this.objTransform = objTransform;

            startingLocRot = objTransform.localRotation;
            startingLocPos = objTransform.localPosition;
            targetLocRot = targetTransform.localRotation;
            targetLocPos = targetTransform.localPosition;
        }

        public override bool Update() { 
            float percent = elapsedTime/lerpingTime;
            float t = percent * percent * (3f - 2f * percent);

            if (base.Update()) return true;

            Vector3 localPos = Vector3.Lerp(startingLocPos, targetLocPos, t);
            Quaternion localRot = Quaternion.Lerp(startingLocRot, targetLocRot, t);
            objTransform.SetLocalPositionAndRotation(localPos, localRot);

            elapsedTime += Time.deltaTime;
            elapsedTime = Mathf.Clamp(elapsedTime, 0, lerpingTime);

            return false;
        }
    }

    private class LerpingRotations : LerpingSingle {
        private readonly Transform objTransform;
        private readonly Quaternion startingLocRot;
        private readonly Quaternion targetLocRot;

        public LerpingRotations(Transform objTransform, Quaternion targetLocRot, float lerpingTime) : base(lerpingTime) {
            this.objTransform = objTransform;

            startingLocRot = objTransform.localRotation;
            this.targetLocRot = targetLocRot;
        }

        public override bool Update() {
            float percent = elapsedTime / lerpingTime;
            float t = percent * percent * (3f - 2f * percent);

            if (base.Update()) return true;

            objTransform.localRotation = Quaternion.Lerp(startingLocRot, targetLocRot, t);

            elapsedTime += Time.deltaTime;
            elapsedTime = Mathf.Clamp(elapsedTime, 0, lerpingTime);

            return false;
        }
    }
}
