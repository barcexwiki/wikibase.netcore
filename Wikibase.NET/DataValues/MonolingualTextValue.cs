using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

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
        public const string TypeJsonName = "monolingualtext";

        /// <summary>
        /// The name of the <see cref="Text"/> property in the serialized json object.
        /// </summary>
        private const string TextJsonName = "text";

        /// <summary>
        /// The name of the <see cref="Language"/> property in the serialized json object.
        /// </summary>
        private const string LanguageJsonName = "language";

        #endregion Json names

        /// <summary>
        /// Gets or sets the text value.
        /// </summary>
        /// <value>The text value.</value>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the language value.
        /// </summary>
        /// <value>The language value.</value>
        public string Language
        {
            get;
            set;
        }

        /// <summary>
        /// Parses a <see cref="JToken"/> to a <see cref="MonolingualTextValue"/>
        /// </summary>
        /// <param name="value"><see cref="JToken"/> to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a JSON object.</exception>
        internal MonolingualTextValue(JToken value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Type != JTokenType.Object)
                throw new ArgumentException("not a JSON object", nameof(value));

            JObject obj = (JObject)value;
            Text = (string)obj[TextJsonName];
            Language = (string)obj[LanguageJsonName];
        }

        /// <summary>
        /// Creates a new monolingual text value for the given text and language.
        /// </summary>
        /// <param name="language">Language.</param>
        /// <param name="text">Text.</param>
        public MonolingualTextValue(string text, string language)
        {
            Text = text;
            Language = language;
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
        /// Encodes the instance in a <see cref="JToken"/>.
        /// </summary>
        /// <returns>Encoded instance.</returns>
        internal override JToken Encode()
        {
            JToken j = new JObject
            {
                { TextJsonName, Text },
                { LanguageJsonName, Language }
            };
                            
            return j;
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
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, other))
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
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, other))
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
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Text) ? this.Text.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.Language) ? this.Text.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}