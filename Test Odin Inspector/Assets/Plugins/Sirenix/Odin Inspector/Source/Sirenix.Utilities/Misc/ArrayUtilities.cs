//-----------------------------------------------------------------------
// <copyright file="ArrayUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;

    /// <summary>
    /// Contains utilities for operating on arrays.
    /// </summary>
    public static class ArrayUtilities
    {
        /// <summary>
        /// Creates a new array with an added element.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The new array.</returns>
        /// <exception cref="System.ArgumentNullException">The given array was null.</exception>
        public static T[] CreateNewArrayWithAddedElement<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            T[] newArray = new T[array.Length + 1];

            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[newArray.Length - 1] = value;
            return newArray;
        }

        /// <summary>
        /// Creates a new array with an element inserted at a given index.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index">The index to insert at.</param>
        /// <param name="value">The value to insert.</param>
        /// <exception cref="System.ArgumentNullException">The given array was null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The index to insert at was out of range.</exception>
        public static T[] CreateNewArrayWithInsertedElement<T>(T[] array, int index, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            T[] newArray = new T[array.Length + 1];

            for (int i = 0; i < newArray.Length; i++)
            {
                if (i < index)
                {
                    newArray[i] = array[i];
                }
                else if (i > index)
                {
                    newArray[i] = array[i - 1];
                }
                else
                {
                    newArray[i] = value;
                }
            }

            return newArray;
        }

        /// <summary>
        /// Creates a new array with an element removed.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index">The index to remove an element at.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">The given array was null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The given index to remove an element at was out of range.</exception>
        public static T[] CreateNewArrayWithRemovedElement<T>(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (index < 0 || index >= array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            T[] newArray = new T[array.Length - 1];

            for (int i = 0; i < array.Length; i++)
            {
                if (i < index)
                {
                    newArray[i] = array[i];
                }
                else if (i > index)
                {
                    newArray[i - 1] = array[i];
                }
            }

            return newArray;
        }
    }
}