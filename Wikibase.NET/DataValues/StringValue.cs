using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

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
        public const string TypeJsonName = "string";

        #endregion Json names

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>The string value.</value>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StringValue"/> with the given value.
        /// </summary>
        /// <param name="value">Value to be added.</param>
        public StringValue(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Parses a <see cref="JToken"/> to a <see cref="StringValue"/>
        /// </summary>
        /// <param name="value"><see cref="JToken"/> to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a JSON string value.</exception>
        internal StringValue(JToken value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Type != JTokenType.String)
                throw new ArgumentException("not a JSON string value", nameof(value));

            Value = (string)value;
        }

        /// <summary>
        /// Gets the data type identifier.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected override string JsonName
        {
            get
            {
                return TypeJsonName;
            }
        }

        /// <summary>
        /// Encodes the instance in a <see cref="JsonValue"/>.
        /// </summary>
        /// <returns>Encoded instance.</returns>
        internal override JToken Encode()
        {
            JToken j = new JValue(Value);
            return j;
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
            if (object.ReferenceEquals(null, value))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, value))
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
            if (object.ReferenceEquals(null, number))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, number))
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