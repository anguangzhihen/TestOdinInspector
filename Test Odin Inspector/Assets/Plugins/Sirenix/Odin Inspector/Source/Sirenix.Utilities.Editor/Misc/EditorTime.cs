#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EditorTime.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// A utility class for getting delta time for the GUI editor.
    /// </summary>
    [InitializeOnLoad]
    public class EditorTimeHelper
    {
        private static EditorTimeHelper time;

        /// <summary>
        /// Gets an EditorTimeHelper instance for the current drawing window.
        /// </summary>
        public static EditorTimeHelper Time
        {
            get { return time ?? (time = new EditorTimeHelper()); }
            set { time = value; }
        }

        private GUIFrameCounter guiState = new GUIFrameCounter();
        private float[] averageDeltaTimes;
        private int deltaTimeIndex = 0;
        private const float DELTA_TIME_THRESHOLD = 0.13f;
        private readonly Stopwatch stopwatch;
        private float deltaTime;
        private long prevMillisecondsElapsed;
        private float newDeltaTime;
        private uint averageDeltaTimeIndex = 0;
        private float averageDeltaTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorTimeHelper"/> class.
        /// </summary>
        public EditorTimeHelper()
        {
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            this.averageDeltaTimes = new float[5];
            this.averageDeltaTimes.Populate(0.1f);
        }

        /// <summary>
        /// Gets the delta time.
        /// </summary>
        public float DeltaTime
        {
            get { return this.deltaTime; }
        }

        /// <summary>
        /// Updates the delta time.
        /// </summary>
        public void Update()
        {
            // We need some data to start with, otherwise things won't animate smoothly in the beginning.
            if (this.deltaTimeIndex != this.averageDeltaTimes.Length)
            {
                GUIHelper.RequestRepaint();
            }

            if (this.guiState.Update().IsNewFrame)
            {
                this.newDeltaTime = (this.stopwatch.ElapsedMilliseconds - this.prevMillisecondsElapsed) / 1000f;
                this.prevMillisecondsElapsed = this.stopwatch.ElapsedMilliseconds;

                if (this.newDeltaTime <= DELTA_TIME_THRESHOLD)
                {
                    unchecked
                    {
                        this.averageDeltaTimes[this.averageDeltaTimeIndex++ % this.averageDeltaTimes.Length] = this.newDeltaTime;

                        if (this.deltaTimeIndex != this.averageDeltaTimes.Length)
                        {
                            this.deltaTimeIndex++;
                        }
                    }
                }

                this.averageDeltaTime = 0f;
                for (int i = 0; i < this.averageDeltaTimes.Length; i++)
                {
                    this.averageDeltaTime += this.averageDeltaTimes[i];
                }
                this.averageDeltaTime /= this.averageDeltaTimes.Length;
            }

            if (Event.current.type == EventType.Layout)
            {
                this.deltaTime = Mathf.Min(this.newDeltaTime, this.averageDeltaTime);
            }
        }
    }
}
#endif