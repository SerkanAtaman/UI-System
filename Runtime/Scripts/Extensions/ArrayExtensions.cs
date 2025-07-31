namespace SeroJob.UiSystem
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] array, T value)
        {
            if (array == null || array.Length == 0) return false;

            foreach (var t in array)
            {
                if (t.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            if (array == null) return true;
            if (array.Length == 0) return true;

            return false;
        }

        public static T[] Expand<T>(this T[] array, T[] values)
        {
            if (array == null)
            {
                UIDebugger.LogError("Can not expand a null array", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            if (values == null || values.Length == 0) return array;

            int newArrayCount = array.Length + values.Length;

            foreach (var value in values)
            {
                if (array.Contains(value)) newArrayCount--;
            }

            T[] newArray = new T[newArrayCount];
            int index = 0;

            for (int i = 0; i < array.Length; i++)
            {
                newArray[index] = array[i];
                index++;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (!array.Contains(values[i]))
                {
                    newArray[index] = values[i];
                    index++;
                }
            }

            return newArray;
        }

        public static T[] Remove<T>(this T[] array, T value)
        {
            if (array == null || value == null)
            {
                UIDebugger.LogError("Can not shrink a null array", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            if (!array.Contains(value))
            {
                UIDebugger.LogError("Can not shrink a null array because array does not contain given value", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            if (array.Length == 1) return new T[0];

            int newArrayLength = array.Length - 1;
            int index = 0;
            T[] newArray = new T[newArrayLength];

            for(int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value)) continue;

                newArray[index] = array[i];
                index++;
            }

            return newArray;
        }

        public static T[] Shrink<T>(this T[] array, T[] values)
        {
            if (array == null)
            {
                UIDebugger.LogError("Can not shrink a null array", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            if (values == null || values.Length == 0) return array;

            if (values.Length > array.Length)
            {
                UIDebugger.LogError("Can not shrink an array by moree than its length", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            int valueToRemoveCount = 0;

            foreach (T value in values)
            {
                if (array.Contains(value))
                {
                    valueToRemoveCount++;
                }
            }

            T[] valuesToRemove = new T[valueToRemoveCount];
            int index = 0;

            foreach (T value in values)
            {
                if (array.Contains(value))
                {
                    valuesToRemove[index] = value;
                    index++;
                }
            }

            int newArrayLength = array.Length * valueToRemoveCount;
            T[] newArray = new T[newArrayLength];
            index = 0;

            foreach (var value in array)
            {
                if (valuesToRemove.Contains(value))
                {
                    newArray[index] = value;
                    index++;
                }
            }

            return newArray;
        }

        public static T[] Add<T>(this T[] array, T value, int index)
        {
            if(array == null)
            {
                UIDebugger.LogError("Can not add item to a null array", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            if(value == null)
            {
                UIDebugger.LogError("Can not add a null item to an array", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            if(index < 0 || index > array.Length)
            {
                UIDebugger.LogError("Target array index is out of the bounds of the given array", "UISystem", UIDebugger.DebugColor.White, UIDebugger.DebugColor.Red);
                return array;
            }

            int newArrayLength = array.Length + 1;
            int baseArrayIndex = 0;
            T[] newArray = new T[newArrayLength];

            for(int i = 0; i < newArrayLength; i++)
            {
                if(i == index)
                {
                    newArray[i] = value;
                }
                else
                {
                    newArray[i] = array[baseArrayIndex];
                    baseArrayIndex++;
                }
            }

            return newArray;
        }

        public static int IndexOf<T>(this T[] array, T element)
        {
            if (array == null || element == null) return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(element)) return i;
            }

            return -1;
        }
    }
}