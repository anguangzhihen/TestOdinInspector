#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GlobalPersistentContext.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

	using Sirenix.Serialization;
	using Sirenix.Utilities;
	using System;

	/// <summary>
	/// Context that persists across reloading and restarting Unity.
	/// </summary>
    [AlwaysFormatsSelf]
	public abstract class GlobalPersistentContext : ISelfFormatter
    {
		/// <summary>
		/// Time stamp for when the persistent context value was last used.
		/// Used for purging unused context.
		/// </summary>
		[OdinSerialize]
		public long TimeStamp { get; protected set; }

        public abstract Type ValueType { get; }

		/// <summary>
		/// Instatiates a persistent context.
		/// </summary>
		protected GlobalPersistentContext()
		{ }

		/// <summary>
		/// Updates the time stamp to now.
		/// </summary>
		protected void UpdateTimeStamp()
		{
			this.TimeStamp = DateTime.Now.Ticks;
		}

        public abstract void Serialize(IDataWriter writer);
        public abstract void Deserialize(IDataReader reader);
    }

	/// <summary>
	/// Context that persists across reloading and restarting Unity.
	/// </summary>
	/// <typeparam name="T">The type of the context value.</typeparam>
    [AlwaysFormatsSelf]
	public sealed class GlobalPersistentContext<T> : GlobalPersistentContext
	{
        private static readonly Serializer<T> ValueSerializer = Serializer.Get<T>();

		private T value;

		/// <summary>
		/// The value of the context.
		/// </summary>
		public T Value
		{
			get
			{
				this.UpdateTimeStamp();
				return this.value;
			}
			set
			{
				this.value = value;
				this.UpdateTimeStamp();
			}
		}

        public override Type ValueType { get { return typeof(T); } }

        /// <summary>
        /// Creates a new persistent context object.
        /// </summary>
        public static GlobalPersistentContext<T> Create()
		{
			var c = new GlobalPersistentContext<T>();
			c.UpdateTimeStamp();
			return c;
		}

        public override void Deserialize(IDataReader reader)
        {
            long time;
            reader.ReadInt64(out time);
            this.TimeStamp = time;
            this.value = ValueSerializer.ReadValue(reader);
        }

        public override void Serialize(IDataWriter writer)
        {
            writer.WriteInt64(null, this.TimeStamp);

            ValueSerializer.WriteValue(this.value, writer);
        }

        /// <summary>
        /// Formats a string with the time stamp, and the value.
        /// </summary>
        public override string ToString()
		{
			return new DateTime(this.TimeStamp).ToString("dd/MM/yy HH:mm:ss") + " <" + typeof(T).GetNiceName() + "> " + (this.value != null ? this.value.ToString() : "(null)");
		}
	}
}
#endif