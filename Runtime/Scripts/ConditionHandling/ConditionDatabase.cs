using System.Collections.Generic;
using System.Linq;
using System;

namespace SeroJob.UiSystem.ConditionHandling
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class ConditionDatabase
    {
        private static BaseCondition[] _conditions;

        private static string[] _conditionDefinitions;

        public static BaseCondition[] Conditions => _conditions;
        public static string[] ConditionDefinitions => _conditionDefinitions;

        static ConditionDatabase()
        {
            _conditions = GetAllConditions().ToArray();
            _conditionDefinitions = GetAllConditionDefinitions();
        }

        private static string[] GetAllConditionDefinitions()
        {
            int count = _conditions.Length;

            string[] definitions = new string[count];

            for(int i = 0; i < count; i++)
            {
                var element = _conditions[i];
                definitions[i] = element.Definition;
            }

            return definitions;
        }

        private static IEnumerable<BaseCondition> GetAllConditions()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(BaseCondition)))
                .Select(type => Activator.CreateInstance(type) as BaseCondition);
        }

        public static BaseCondition GetConditionFromDefinition(string definition)
        {
            BaseCondition result = null;
            int count = _conditions.Length;

            for (int i = 0; i < count; i++)
            {
                var element = _conditions[i];
                if(element.Definition == definition)
                {
                    result = element;
                    break;
                }
            }

            return result;
        }
    }
}