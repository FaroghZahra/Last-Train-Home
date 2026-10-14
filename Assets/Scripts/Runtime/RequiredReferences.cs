using UnityEngine;

namespace LastTrainHome
{
    /// <summary>Fail-fast check for Inspector references a component cannot work without.</summary>
    internal static class RequiredReferences
    {
        /// <summary>
        /// Logs one error naming every unassigned field and disables <paramref name="owner"/> if any are missing.
        /// </summary>
        /// <returns>True when every reference is assigned.</returns>
        public static bool Check(Behaviour owner, params (Object value, string fieldName)[] references)
        {
            string missing = null;
            foreach ((Object value, string fieldName) in references)
            {
                if (value == null)
                {
                    missing = missing == null ? fieldName : $"{missing}, {fieldName}";
                }
            }

            if (missing == null)
            {
                return true;
            }

            Debug.LogError($"{owner.GetType().Name} on '{owner.name}' is missing: {missing}. Assign in the Inspector.", owner);
            owner.enabled = false;
            return false;
        }
    }
}
