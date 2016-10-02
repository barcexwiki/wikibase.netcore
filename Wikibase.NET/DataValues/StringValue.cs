using System;
using System.Collections.Generic;
using System.Text;
using MinimalJson;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Encapsulates the string value type.
    /// </summary>
    public class StringValue : DataValue
    {
        #region Json names

        /// <summary>
        /// The identifier of this data type in the serialized json object.
        /// </summary>
        public const String TypeJsonName = "string";

        #endregion Json names

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>The string value.</value>
        public String Value
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StringValue"/> with the given value.
        /// </summary>
        /// <param name="value">Value to be added.</param>
        public StringValue(String value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Parses a <see cref="JsonValue"/> to a <see cref="StringValue"/>
        /// </summary>
        /// <param name="value"><see cref="JsonValue"/> to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        internal StringValue(JsonValue value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            this.Value = value.asString();
        }

        /// <summary>
        /// Gets the data type identifier.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected override String JsonName()
        {
            return TypeJsonName;
        }

        /// <summary>
        /// Encodes the instance in a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>Encoded instance.</returns>
        internal override JsonValue Encode()
        {
            return JsonValue.valueOf(Value);
        }

        private bool IsEqual(StringValue other)
        {
            StringValue s = other as StringValue;

            return (s != null)
                && (this.Value == s.Value);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public override bool Equals(object value)
        {
            // Is null?
            if (Object.ReferenceEquals(null, value))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, value))
            {
                return false;
            }

            // Is the same type?
            if (value.GetType() != this.GetType())
            {
                return false;
            }

            return IsEqual((StringValue)value);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public bool Equals(StringValue number)
        {
            // Is null?
            if (Object.ReferenceEquals(null, number))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, number))
            {
                return true;
            }

            return IsEqual(number);
        }

        /// <summary>
        /// Gets the hash code of this object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}