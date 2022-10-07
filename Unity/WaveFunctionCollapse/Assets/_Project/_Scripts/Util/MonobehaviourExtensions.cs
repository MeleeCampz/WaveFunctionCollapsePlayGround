using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public static class MonobehaviourExtensions
    {
        /// <summary>
        /// Disable MB if object is null an print error message
        /// </summary>
        /// <param name="mb">Self</param>
        /// <param name="toNullCheck">object to null check</param>
        /// <param name="errorMessage">error message to show if null</param>
        /// <returns>return if the object was null</returns>
        public static bool DisableIfNull(this MonoBehaviour mb, object toNullCheck, string errorMessage)
        {
            if (toNullCheck == null)
            {
                Debug.LogError(errorMessage, mb);
                mb.enabled = false;
                return false;
            }
            return true;
        }
    }
}