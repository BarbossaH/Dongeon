using System.Collections;
using UnityEngine;

namespace Utilities
{
    public static class HelperUtilities
    {
        public static bool ValidateCheckEmptyString(Object thisObject, string fileName, string stringToCheck)
        {
            if (stringToCheck == "")
            {
                Debug.Log(fileName+" is empty and must contain a value in object" + thisObject.name);
                return true;
            }
            return false;
        }

        public static bool ValidateCheckEnumerableValues(Object thisObject, string fileName,
            IEnumerable enumerableToCheck)
        {
            bool isValid = true;
            int count = 0;

            foreach (var item in enumerableToCheck)
            {
                if (item == null)
                {
                    Debug.Log(fileName+" has null values in object "+ thisObject.name);
                    isValid = false;
                }
                else
                {
                    count++;
                }
            }
            if (count==0)
            {
                Debug.Log(fileName+ "  has no values in object"+ thisObject.name);
                isValid = false;
            }
            return isValid;
        }
    }
}