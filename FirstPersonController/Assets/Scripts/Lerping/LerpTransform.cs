using System;
using UnityEngine;

namespace Lerping {
    public class LerpTransform : Lerp {
        private Transform objTransform;


        private Quaternion startingLocRot;
        private Vector3 startingLocPos;

        private Quaternion targetLocRot;
        private Vector3 targetLocPos;

        public static LerpTransform Initialize(Transform gameObject, Quaternion targetLocRot, Vector3 targetLocPos, float lerpingTime, Func<float, float> lerpingFunc = null) {
            GameObject executer = new($"{gameObject.name} Lerp Executer");
            LerpTransform lm = executer.AddComponent<LerpTransform>();

            lm.objTransform = gameObject;

            lm.startingLocRot = gameObject.localRotation;
            lm.startingLocPos = gameObject.localPosition;

            lm.targetLocPos = targetLocPos;
            lm.targetLocRot = targetLocRot;

            lm.lerpingTime = lerpingTime;
            lm.lerpingFunction = lerpingFunc ?? (x => x * x * (3f - 2f * x));

            return lm;
        }

        protected override void SettingValues(float t) {
            Vector3 localPos = Vector3.Lerp(startingLocPos, targetLocPos, t);
            Quaternion localRot = Quaternion.Lerp(startingLocRot, targetLocRot, t);
            objTransform.SetLocalPositionAndRotation(localPos, localRot);
        }
    }
}
