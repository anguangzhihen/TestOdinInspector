//-----------------------------------------------------------------------
// <copyright file="MultiDimArrayUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;

    /// <summary>
    /// Contains utilities for operating on arrays multi-dimentional arrays.
    /// </summary>
    public static class MultiDimArrayUtilities
    {
        /// <summary>
        /// Inserts one column left of the specified column index.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="array">The array.</param>
        public static TElement[,] InsertOneColumnLeft<TElement>(TElement[,] array, int columnIndex)
        {
            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);
            var newArr = new TElement[colCount + 1, Math.Max(rowCount, 1)];

            for (int x = 0; x < colCount; x++)
            {
                int tmpX = x;

                if (tmpX >= columnIndex)
                {
                    tmpX++;
                }

                for (int y = 0; y < rowCount; y++)
                {
                    newArr[tmpX, y] = array[x, y];
                }
            }

            return newArr;
        }

        /// <summary>
        /// Inserts one column right of the specified column index.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="arr">The arr.</param>
        public static TElement[,] InsertOneColumnRight<TElement>(TElement[,] arr, int columnIndex)
        {
            var colCount = arr.GetLength(0);
            var rowCount = arr.GetLength(1);
            var newArr = new TElement[colCount + 1, Math.Max(rowCount, 1)];

            for (int x = 0; x < colCount; x++)
            {
                int tmpX = x;

                if (tmpX > columnIndex)
                {
                    tmpX++;
                }

                for (int _y = 0; _y < rowCount; _y++)
                {
                    newArr[tmpX, _y] = arr[x, _y];
                }
            }

            return newArr;
        }

        /// <summary>
        /// Inserts one row above the specified row index.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="rowIndex">The row index.</param>
        public static TElement[,] InsertOneRowAbove<TElement>(TElement[,] array, int rowIndex)
        {
            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);
            var newArr = new TElement[Math.Max(colCount, 1), rowCount + 1];

            for (int y = 0; y < rowCount; y++)
            {
                int tmpY = y;

                if (tmpY >= rowIndex)
                {
                    tmpY++;
                }

                for (int x = 0; x < colCount; x++)
                {
                    newArr[x, tmpY] = array[x, y];
                }
            }

            return newArr;
        }

        /// <summary>
        /// Inserts one row below the specified row index.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="rowIndex">Index of the row.</param>
        public static TElement[,] InsertOneRowBelow<TElement>(TElement[,] array, int rowIndex)
        {
            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);
            var newArr = new TElement[Math.Max(colCount, 1), rowCount + 1];

            for (int y = 0; y < rowCount; y++)
            {
                int tmpY = y;

                if (tmpY > rowIndex)
                {
                    tmpY++;
                }

                for (int x = 0; x < colCount; x++)
                {
                    newArr[x, tmpY] = array[x, y];
                }
            }

            return newArr;
        }

        /// <summary>
        /// Duplicates the column.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="array">The array.</param>
        public static TElement[,] DuplicateColumn<TElement>(TElement[,] array, int columnIndex)
        {
            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);
            var newArr = new TElement[colCount + 1, Math.Max(rowCount, 1)];

            for (int x = 0; x < colCount; x++)
            {
                int tmpX = x;

                if (tmpX >= columnIndex)
                {
                    tmpX++;
                }

                for (int y = 0; y < rowCount; y++)
                {
                    newArr[tmpX, y] = array[x, y];
                }
            }

            for (int y = 0; y < newArr.GetLength(1); y++)
            {
                newArr[columnIndex, y] = array[columnIndex, y];
            }

            return newArr;
        }

        /// <summary>
        /// Duplicates the row.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="rowIndex">Index of the row.</param>
        public static TElement[,] DuplicateRow<TElement>(TElement[,] array, int rowIndex)
        {
            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);
            var newArr = new TElement[Math.Max(colCount, 1), rowCount + 1];

            for (int y = 0; y < rowCount; y++)
            {
                int tmpY = y;

                if (tmpY >= rowIndex)
                {
                    tmpY++;
                }

                for (int x = 0; x < colCount; x++)
                {
                    newArr[x, tmpY] = array[x, y];
                }
            }

            for (int x = 0; x < newArr.GetLength(0); x++)
            {
                newArr[x, rowIndex] = array[x, rowIndex];
            }

            return newArr;
        }

        /// <summary>
        /// Moves a column.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="fromColumn">From column.</param>
        /// <param name="toColumn">To column.</param>
        public static TElement[,] MoveColumn<TElement>(TElement[,] array, int fromColumn, int toColumn)
        {
            if (fromColumn == toColumn)
            {
                return array;
            }

            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);

            var newArr = new TElement[colCount, rowCount];

            if (fromColumn < toColumn)
            {
                // Move from left to right
                for (int x = 0; x < colCount; x++)
                {
                    int offset = x >= fromColumn && x < toColumn ? 1 : 0;
                    for (int y = 0; y < rowCount; y++)
                    {
                        if (x == toColumn)
                        {
                            newArr[x, y] = array[fromColumn, y];
                        }
                        else
                        {
                            newArr[x, y] = array[x + offset, y];
                        }
                    }
                }
            }
            else
            {
                // Move from right to left
                for (int x = 0; x < colCount; x++)
                {
                    int offset = x > toColumn && x <= fromColumn ? 1 : 0;
                    for (int y = 0; y < rowCount; y++)
                    {
                        if (x == toColumn + 1)
                        {
                            newArr[x, y] = array[fromColumn, y];
                        }
                        else
                        {
                            newArr[x, y] = array[x - offset, y];
                        }
                    }
                }
            }

            return newArr;
        }

        /// <summary>
        /// Moves a row.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        public static TElement[,] MoveRow<TElement>(TElement[,] array, int fromRow, int toRow)
        {
            if (fromRow == toRow)
            {
                return array;
            }

            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1);

            var newArr = new TElement[colCount, rowCount];

            if (fromRow < toRow)
            {
                // Move down
                for (int y = 0; y < rowCount; y++)
                {
                    int offset = y >= fromRow && y < toRow ? 1 : 0;
                    for (int x = 0; x < colCount; x++)
                    {
                        if (y == toRow)
                        {
                            newArr[x, y] = array[x, fromRow];
                        }
                        else
                        {
                            newArr[x, y] = array[x, y + offset];
                        }
                    }
                }
            }
            else
            {
                // Move up
                for (int y = 0; y < rowCount; y++)
                {
                    int offset = y > toRow && y <= fromRow ? 1 : 0;
                    for (int x = 0; x < colCount; x++)
                    {
                        if (y == toRow + 1)
                        {
                            newArr[x, y] = array[x, fromRow];
                        }
                        else
                        {
                            newArr[x, y] = array[x, y - offset];
                        }
                    }
                }
            }

            return newArr;
        }

        /// <summary>
        /// Deletes a column.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="columnIndex">Index of the column.</param>
        public static TElement[,] DeleteColumn<TElement>(TElement[,] array, int columnIndex)
        {
            var colCount = array.GetLength(0) - 1;
            var rowCount = array.GetLength(1);
            if (colCount <= 0)
            {
                colCount = 0;
                rowCount = 0;
            }

            var newArr = new TElement[colCount, rowCount];

            for (int x = 0; x < colCount; x++)
            {
                int tmpX = x;

                if (tmpX >= columnIndex)
                {
                    tmpX++;
                }

                for (int y = 0; y < rowCount; y++)
                {
                    newArr[x, y] = array[tmpX, y];
                }
            }

            return newArr;
        }

        /// <summary>
        /// Deletes the row.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="rowIndex">Index of the row.</param>
        public static TElement[,] DeleteRow<TElement>(TElement[,] array, int rowIndex)
        {
            var colCount = array.GetLength(0);
            var rowCount = array.GetLength(1) - 1;
            if (rowCount <= 0)
            {
                colCount = 0;
                rowCount = 0;
            }

            var newArr = new TElement[colCount, rowCount];

            for (int y = 0; y < rowCount; y++)
            {
                int tmpY = y;

                if (tmpY >= rowIndex)
                {
                    tmpY++;
                }

                for (int x = 0; x < colCount; x++)
                {
                    newArr[x, y] = array[x, tmpY];
                }
            }

            return newArr;
        }
    }
}