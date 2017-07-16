/*
Copyright 2012 MCForge
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
*/
using System;
using System.Collections.Generic;
//Written for use in MCForge5 (Not a MCLawl fork)
namespace FootballPlugin
{
    /// <summary>
    /// Misc utils and extentions.
    /// </summary>
    public static class MiscUtils
    {


        /// <summary>
        /// Determines whether [contains ignore case] [the specified array].
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="test">The test.</param>
        /// <returns>
        ///   <c>true</c> if [contains ignore case] [the specified array]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsIgnoreCase(this string[] array, string test)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i].ToLower() == test.ToLower())
                    return true;
            return false;
        }



        /// <summary>
        /// Gets the object if it exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static object GetIfExist<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (key == null)
                return null;
            if (dict.ContainsKey(key))
                return dict[key];
            return null;
        }

        public static bool RemoveValue<TKey, TValue>(this Dictionary<TKey, IList<TValue>> dict, TKey key, TValue valueToRemove)
        {
            if (!dict.ContainsKey(key))
                return false;

            foreach (var value in dict)
            {
                if (value.Key.Equals(key))
                {
                    return value.Value.Remove(valueToRemove);
                }
            }
            return false;
        }

        public static void AddValue<TKey, TValue>(this Dictionary<TKey, IList<TValue>> dict, TKey key, TValue valueToAdd)
        {
            if (!dict.CreateIfNotExist<TKey, IList<TValue>>(key, new List<TValue> { valueToAdd }))
                return;

            foreach (var value in dict)
                if (value.Key.Equals(key))
                    value.Value.Add(valueToAdd);

        }

        /// <summary>
        /// Puts object in list if it does not exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>If it exists, returns true. Else, returns false</returns>
        public static bool CreateIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Converts the list into a string
        /// </summary>
        /// <param name="list"></param>
        /// <returns>The string value of the list</returns>
        public static string ToString<T>(this T[] array)
        {
            string ret = "";
            foreach (T item in array)
            {
                ret += item.ToString() + "\n";
            }
            return ret;
        }

        /// <summary>
        /// Changes the value or Creates it if it doesnt exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void ChangeOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            dict.CreateIfNotExist<TKey, TValue>(key, value);
            dict[key] = value;
        }

        /// <summary>
        /// Get an object with out the need to cast
        /// </summary>
        /// <typeparam name="TKey">The type of key</typeparam>
        /// <typeparam name="TValue">The type of object to return</typeparam>
        /// <param name="dict">The dictionary to use</param>
        /// <param name="key">The key of the dictionary</param>
        /// <returns>An object casted to the specified type, or null if not found</returns>
        /// <remarks>Must have a nullable type interface</remarks>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            return (TValue)dict.GetIfExist<TKey, TValue>(key);
        }

        /// <summary>
        /// If an array contains that object it returns <c>true</c> otherwise <c>false</c>
        /// </summary>
        /// <typeparam name="T">Type of the array and object</typeparam>
        /// <param name="theArray">The array to check</param>
        /// <param name="obj">object to check</param>
        /// <returns>If an array contains that object it returns <c>true</c> otherwise <c>false</c></returns>
        public static bool Contains<T>(this T[] theArray, T obj)
        {
            for (int i = 0; i < theArray.Length; i++)
            {
                T d = theArray[i];
                if (d.Equals(obj))
                    return true;
            }
            return false;
        }
    }
}