using UnityEngine;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace Scripts {
    public class BehaviorOnConditionManager : MonoBehaviour {
        private static readonly List<BehaviorOnCondition> conditions = new();

        static BehaviorOnConditionManager() { 
            new GameObject("ConditionActionManager").AddComponent<BehaviorOnConditionManager>();
        }

#nullable enable
        public static void ConstructBehaviorOnCondition(Object? container, Func<bool> condition, params Action[] actions) { 
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
        private readonly struct BehaviorOnCondition {
            public Object? Container { get; }
            private Func<bool> Condition { get; }
            private Action[] Actions { get; }

            public BehaviorOnCondition(Object? container, Func<bool> condition, Action[] actions) {
                Container = container;
                Condition = condition;
                Actions = actions;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>bool whether condition was true</returns>
            public bool Update() {
                return Condition();
            }

            public void Execute() { 
                foreach(var action in Actions) action();
            }
        }
#nullable disable
    }
}