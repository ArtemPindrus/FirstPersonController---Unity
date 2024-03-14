using UnityEngine;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using System.Linq;

namespace Scripts {
    public class ConditionActionManager : MonoBehaviour {
        private readonly static List<ConditionActions> conditions = new();

        static ConditionActionManager() { 
            new GameObject("ConditionActionManager").AddComponent<ConditionActionManager>();
        }

#nullable enable
        public static void ConstructConditionActions(Object? container, Func<bool> condition, params Action[] actions) {
            conditions.Add(new(container, condition, actions));
        }
#nullable disable
        public static void DeleteConditionActionsOfContainer(Object container) {
            conditions.RemoveAll(x => x.Container == container);
        }

        [ContextMenu("Debug Count")]
        private void DebugCount() {
            Debug.Log(conditions.Count);
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

#nullable enable
        private readonly struct ConditionActions {
            public Object? Container { get; }
            public Func<bool> Condition { get; }
            public Action[] Actions { get; }

            public ConditionActions(Object? container, Func<bool> condition, Action[] actions) {
                Container = container;
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
#nullable disable
    }
}