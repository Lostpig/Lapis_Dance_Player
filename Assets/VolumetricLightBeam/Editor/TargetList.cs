#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    public class TargetList<T> : IEnumerable<T> where T : MonoBehaviour
    {
        public int Count { get { return m_Targets != null ? m_Targets.Count : 0; } }
        public T this[int key] { get { return m_Targets != null ? m_Targets[key] : null; } }

        public List<T> m_Targets = null;

        public TargetList(UnityEngine.Object[] entities)
        {
            m_Targets = new List<T>();
            foreach (var entity in entities)
            {
                if (entity is T)
                {
                    m_Targets.Add(entity as T); // directly get the component from the object
                }
                else
                {
                    // otherwise get access from the current MonoBehaviour, in case the current MonoBehaviour is not the type wanted as target
                    var behaviour = entity as MonoBehaviour;
                    var comp = behaviour.GetComponent<T>();
                    if (comp)
                        m_Targets.Add(comp);
                }
            }
            Debug.Assert(m_Targets.Count > 0);
        }

        public bool HasAtLeastOneTargetWith(System.Func<T, bool> lambda)
        {
            foreach (var target in m_Targets)
            {
                if (lambda(target))
                {
                    return true;
                }
            }
            return false;
        }

        public void RecordUndoAction(string name, System.Action<T> lambda)
        {
            Undo.RecordObjects(m_Targets.ToArray(), name);

            foreach (var target in m_Targets)
            {
                lambda(target);
            }
        }

        // make this object foreach compatible
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var target in m_Targets)
                yield return target;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
#endif

