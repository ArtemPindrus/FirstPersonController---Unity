using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Extensions {
    public static class CharacterControllerExtensions {
        public static void OnGrounded(this CharacterController characterController, Action subscriber) {
            characterController.AddComponent<CCTOnGroundedEvent>().Initialize(subscriber);
        }

        private class CCTOnGroundedEvent : MonoBehaviour { 
            private CharacterController characterController;
            private bool previousIsGrounded;
            private Action callback;

            public CCTOnGroundedEvent Initialize(Action callback) {
                characterController = GetComponent<CharacterController>();
                this.callback = callback;
                previousIsGrounded = characterController.isGrounded;

                return this;
            }

            public void Update() {
                if (!previousIsGrounded && characterController.isGrounded) { 
                    callback();
                }

                previousIsGrounded = characterController.isGrounded;
            }
        }
    }
}
