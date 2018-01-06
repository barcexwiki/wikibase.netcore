using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wikibase.DataValues;
using Newtonsoft.Json.Linq;

namespace Wikibase
{
    /// <summary>
    /// Possible type of snaks, see <see cref="Snak.Type"/>.
    /// </summary>
    public enum SnakType
    {
        /// <summary>
        /// Snak containing a <see cref="DataValue"/>.
        /// </summary>
        Value,

        /// <summary>
        /// No value.
        /// </summary>
        None,

        /// <summary>
        /// Snak should have a value, but it is unknown.
        /// </summary>
        SomeValue
    }

    /// <summary>
    /// A snak, a property with its value.
    /// </summary>
    public class Snak
    {
        #region Json names

        /// <summary>
        /// The name of the <see cref="Type"/> property in the serialized json object.
        /// </summary>
        private const string SnakTypeJsonName = "snaktype";

        /// <summary>
        /// The name of the <see cref="PropertyId"/> property in the serialized json object.
        /// </summary>
        private const string PropertyJsonName = "property";

        /// <summary>
        /// The name of the <see cref="DataValue"/> property in the serialized json object.
        /// </summary>
        private const string DataValueJsonName = "datavalue";

        // TODO - make it a property instead
        /// <summary>
        /// JSon identifiers for the <see cref="SnakType"/>.
        /// </summary>
        protected static Dictionary<SnakType, string> _snakTypeIdentifiers = new Dictionary<SnakType, string>()
        {
            {SnakType.None,"novalue"},
            {SnakType.SomeValue,"somevalue"},
            {SnakType.Value,"value"},
        };

        #endregion Json names

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public SnakType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the property id.
        /// </summary>
        /// <value>The property id.</value>
        public EntityId PropertyId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        /// <value>The data value.</value>
        public DataValue DataValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new <see cref="Snak"/> with the given values.
        /// </summary>
        /// <param name="type">The snak type.</param>
        /// <param name="propertyId">The property id.</param>
        /// <param name="dataValue">The data value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyId"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="propertyId"/> has <see cref="EntityId.Type"/> not set as <see cref="EntityType.Property"/>.</exception>
        public Snak(SnakType type, EntityId propertyId, DataValue dataValue)
        {
            if (propertyId == null)
                throw new ArgumentNullException(nameof(propertyId));

            if (propertyId.Type != EntityType.Property)
            {
                throw new ArgumentException("propertyId must be a valid property id", nameof(propertyId));
            }
            this.Type = type;
            this.PropertyId = propertyId;
            this.DataValue = dataValue;
        }

        /// <summary>
        /// Creates a new snak from the json object.
        /// </summary>
        /// <param name="data">Json object to parse.</param>
        internal Snak(JToken data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            FillFromArray(data);
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected Snak()
        {
        }

        /// <summary>
        /// Fills the snak with data parsed from a JSon array.
        /// </summary>
        /// <param name="data">JSon array to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected virtual void FillFromArray(JToken data)
        {

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data[SnakTypeJsonName] == null || data[PropertyJsonName] == null)
            {
                throw new ArgumentException("Invalid Snak serialization", nameof(data));
            }
            string type = (string)data[SnakTypeJsonName];
            if (_snakTypeIdentifiers.Any(x => x.Value == type))
            {
                this.Type = _snakTypeIdentifiers.First(x => x.Value == type).Key;
            }
            else
            {
                throw new ArgumentException("JToken contained unknown snaktype");
            }

            this.PropertyId = new EntityId((string)data[PropertyJsonName]);
            JToken readDataValue = data[DataValueJsonName];
            // if type!=value, then there is no datavalue in the read data
            if ((readDataValue != null) && (readDataValue.Type == JTokenType.Object))
            {
                DataValue = DataValueFactory.CreateFromJsonObject(data[DataValueJsonName]);
            }
        }

        /// <summary>
        /// Encodes as a <see cref="JToken"/>.
        /// </summary>
        /// <returns>Encoded class.</returns>
        internal JToken Encode()
        {
            JObject data = new JObject
            {
                { SnakTypeJsonName, _snakTypeIdentifiers[this.Type] },
                { PropertyJsonName, this.PropertyId.PrefixedId }
            };

            if (this.DataValue != null)
            {
                data.Add(new JProperty(DataValueJsonName, this.DataValue.FullEncode()));
            }

            return data;
        }

        /// <summary>
        /// Encodes this snak in a JSON representation
        /// </summary>
        /// <returns>string with the JSON representation of the snak</returns>
        public string ToJson()
        {
            return Encode().ToString();
        }
    }
}