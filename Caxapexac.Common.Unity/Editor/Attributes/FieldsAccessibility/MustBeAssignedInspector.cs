using System;
using System.Linq;
using System.Reflection;
using Caxapexac.Common.Sharp.Editor.Attributes.Utils.EditorExtensions;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;


namespace Caxapexac.Common.Sharp.Editor.Attributes.FieldsAccessibility
{
    [InitializeOnLoad]
    public class MustBeAssignedInspector
    {
        /// <summary>
        /// A way to conditionally disable MustBeAssigned check
        /// </summary>
        public static Func<FieldInfo, MonoBehaviour, bool> ExcludeFieldFilter;

        static MustBeAssignedInspector()
        {
            //TODO MyEditorEvents.OnSave += AssertComponentsInScene;
            PrefabStage.prefabSaved += AssertComponentsInPrefab;
        }

        private static void AssertComponentsInScene()
        {
            var components = MyEditor.GetAllBehavioursInScenes();
            AssertComponent(components);
        }

        private static void AssertComponentsInPrefab(GameObject prefab)
        {
            MonoBehaviour[] components = prefab.GetComponentsInChildren<MonoBehaviour>();
            AssertComponent(components);
        }

        private static void AssertComponent(MonoBehaviour[] components)
        {
            foreach (MonoBehaviour behaviour in components)
            {
                if (behaviour == null) continue;
                Type typeOfScript = behaviour.GetType();
                FieldInfo[] mustBeAssignedFields = typeOfScript
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.IsDefined(typeof(MustBeAssignedAttribute), false))
                    .ToArray();

                foreach (FieldInfo field in mustBeAssignedFields)
                {
                    object propValue = field.GetValue(behaviour);

                    // Used by external systems to exclude specific fields.
                    // Specifically for ConditionalFieldAttribute
                    if (FieldIsExcluded(field, behaviour)) continue;


                    bool valueTypeWithDefaultValue = field.FieldType.IsValueType && Activator.CreateInstance(field.FieldType).Equals(propValue);
                    if (valueTypeWithDefaultValue)
                    {
                        Debug.LogError($"{typeOfScript.Name} caused: {field.Name} is Value Type with default value",
                            behaviour.gameObject);
                        continue;
                    }


                    bool nullReferenceType = propValue == null || propValue.Equals(null);
                    if (nullReferenceType)
                    {
                        Debug.LogError($"{typeOfScript.Name} caused: {field.Name} is not assigned (null value)",
                            behaviour.gameObject);
                        continue;
                    }


                    bool emptyString = field.FieldType == typeof(string) && (string)propValue == string.Empty;
                    if (emptyString)
                    {
                        Debug.LogError($"{typeOfScript.Name} caused: {field.Name} is not assigned (empty string)",
                            behaviour.gameObject);
                        continue;
                    }


                    var arr = propValue as Array;
                    bool emptyArray = arr != null && arr.Length == 0;
                    if (emptyArray)
                    {
                        Debug.LogError($"{typeOfScript.Name} caused: {field.Name} is not assigned (empty array)",
                            behaviour.gameObject);
                    }
                }
            }
        }

        private static bool FieldIsExcluded(FieldInfo field, MonoBehaviour behaviour)
        {
            if (ExcludeFieldFilter == null) return false;

            foreach (var filterDelegate in ExcludeFieldFilter.GetInvocationList())
            {
                var filter = filterDelegate as Func<FieldInfo, MonoBehaviour, bool>;
                if (filter != null && filter(field, behaviour)) return true;
            }

            return false;
        }
    }
}