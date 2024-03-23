using UnityEngine;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace Work {
    public class WorkManager : MonoBehaviour {
        private static readonly List<WorkInUpdate> updateDoes = new();

        static WorkManager() { 
            new GameObject("ConditionActionManager").AddComponent<WorkManager>();
        }

        #region Constructors
#nullable enable
        public static WorkInUpdate ConstructDoWhenInUpdate(Object? container, Func<bool> when, params Action[] doThis) {
            DoWhenInUpdate work = new(container, doThis, when);
            updateDoes.Add(work);

            return work;
        }

        public static void ConstructDoWhileInUpdate(Object? container, Func<bool> @while, Action[] doThis, params Action[] finallyActions) {
            updateDoes.Add(new DoWhileInUpdate(container, doThis, @while, finallyActions));
        }
#nullable disable
        #endregion

        public static void DeleteWorkInUpdateOfContainer(Object container) {
            updateDoes.RemoveAll(x => x.Container == container);
        }

        [ContextMenu("Debug Count")]
        private void DebugCount() {
            Debug.Log(updateDoes.Count);
        }

        private void Update() {
            for (int i = 0; i < updateDoes.Count; i++) {
                if (updateDoes[i].Update()) {
                    updateDoes.RemoveAt(i);

                    i--;
                }
            }
        }

#nullable enable

        public class WorkInUpdate { 
            public Object? Container { get; }
            protected Func<bool> Condition { get; }
            private Action[] PrimaryActions { get; }

            public bool Completed { get; protected set; }


            public WorkInUpdate(Object? container, Func<bool> condition, params Action[] primaryActions) { 
                Container = container;
                Condition = condition;
                PrimaryActions = primaryActions;
            }

            /// <summary>
            /// Called every Update loop
            /// </summary>
            /// <returns>true if completed</returns>
            /// <exception cref="NotImplementedException"></exception>
            public virtual bool Update() => throw new NotImplementedException();

            public void Kill() => updateDoes.Remove(this);

            public void ExecutePrimaryActions() {
                foreach (var action in PrimaryActions) action();
            }
        }

        private class DoWhileInUpdate : WorkInUpdate {
            public Action[] finallyActions;

            public DoWhileInUpdate(Object? container, Action[] doThis, Func<bool> whileCondition, Action[] @finally) : base(container, whileCondition, doThis) {
                finallyActions = @finally;
            }

            public override bool Update() {
                if (Condition()) {
                    ExecutePrimaryActions();
                    return false;
                } else {
                    ExecuteFinallyActions();
                    Completed = true;
                    return true;
                }
            }

            private void ExecuteFinallyActions() {
                foreach (var finallyAct in finallyActions) finallyAct();
            }
        }

        private class DoWhenInUpdate : WorkInUpdate {

            public DoWhenInUpdate(Object? container, Action[] doThis, Func<bool> when) : base(container, when, doThis) {
                
            }

            public override bool Update() {
                if (Condition()) {
                    ExecutePrimaryActions();
                    Completed = true;
                    return true;
                }

                return false;
            }
        }
#nullable disable
    }
}