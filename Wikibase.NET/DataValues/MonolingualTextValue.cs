using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinimalJson;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Encapsulates the monolingual text value type.
    /// </summary>
    public class MonolingualTextValue : DataValue
    {
        #region Json names

        /// <summary>
        /// The identifier of this data type in the serialized json object.
        /// </summary>
        public const String TypeJsonName = "monolingualtext";

        /// <summary>
        /// The name of the <see cref="Text"/> property in the serialized json object.
        /// </summary>
        private const String TextJsonName = "text";

        /// <summary>
        /// The name of the <see cref="Language"/> property in the serialized json object.
        /// </summary>
        private const String LanguageJsonName = "language";

        #endregion Json names

        /// <summary>
        /// Gets or sets the text value.
        /// </summary>
        /// <value>The text value.</value>
        public String Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the language value.
        /// </summary>
        /// <value>The language value.</value>
        public String Language
        {
            get;
            set;
        }

        /// <summary>
        /// Parses a <see cref="JsonValue"/> to a <see cref="MonolingualTextValue"/>
        /// </summary>
        /// <param name="value"><see cref="JsonValue"/> to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        internal MonolingualTextValue(JsonValue value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            JsonObject obj = value.asObject();
            this.Text = obj.get(TextJsonName).asString();
            this.Language = obj.get(LanguageJsonName).asString();
        }

        /// <summary>
        /// Creates a new monolingual text value for the given text and language.
        /// </summary>
        /// <param name="language">Language.</param>
        /// <param name="text">Text.</param>
        public MonolingualTextValue(String text, String language)
        {
            Text = text;
            Language = language;
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
            return new JsonObject()
                .add(TextJsonName, Text)
                .add(LanguageJsonName, Language);
        }

        private bool IsEqual(MonolingualTextValue other)
        {
            MonolingualTextValue text = other as MonolingualTextValue;

            return (text != null)
                && (this.Language == text.Language)
                && (this.Text == text.Text);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public override bool Equals(object other)
        {
            // Is null?
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, other))
            {
                return false;
            }

            // Is the same type?
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            return IsEqual((MonolingualTextValue)other);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public bool Equals(MonolingualTextValue other)
        {
            // Is null?
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return IsEqual(other);
        }

        /// <summary>
        /// Gets the hash code of this object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // Prime numbers
                const int Base = (int)2166136261;
                const int Multiplier = 16777619;

                int hashCode = Base;
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.Text) ? this.Text.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.Language) ? this.Text.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}