using UnityEngine;
using System.Collections.Generic;
using System;

namespace Scripts {
    public class ConditionActionManager : MonoBehaviour {
        private readonly static List<ConditionActions> conditions = new();

        static ConditionActionManager() { 
            new GameObject("ConditionActionManager").AddComponent<ConditionActionManager>();
        }

        public static void ConstructConditionActions(Func<bool> condition, params Action[] actions) {
            conditions.Add(new(condition, actions));
        }

        private void Update() {
            for (int i = 0; i < conditions.Count; i++) {
                if (conditions[i].Update()) {
                    conditions[i].Execute();
                    conditions.RemoveAt(i);

                    i--;
                }
            }
        }


        private readonly struct ConditionActions {
            public Func<bool> Condition { get; }
            public Action[] Actions { get; }

            public ConditionActions(Func<bool> condition, Action[] actions) { 
                Condition = condition;
                Actions = actions;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>bool whether condition was true</returns>
            public readonly bool Update() {
                if (Condition()) { 
                    foreach (var action in Actions) action();
                    return true;
                }

                return false;
            }

            public readonly void Execute() { 
                foreach(var action in Actions) action();
            }
        }
    }
}