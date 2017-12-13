using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase
{
    /// <summary>
    /// A property
    /// </summary>
    public class Property : Entity
    {

           /// <summary>
        /// The name of the <see cref="DataType"/> property in the serialized json object.
        /// </summary>
        private const string DataTypeJsonName = "datatype";

        /// <summary>
        /// The data type
        /// </summary>
        public string DataType
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor creating a blank property.
        /// </summary>
        /// <param name="api">The api</param>
        public Property(WikibaseApi api, string dataType)
            : base(api)
        {
            DataType = dataType;
        }

        /// <summary>
        /// Constructor creating a property from a Json object.
        /// </summary>
        /// <param name="api">The api</param>
        /// <param name="data">The json object to be parsed.</param>
        internal Property(WikibaseApi api, JToken data)
            : base(api, data)
        {
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JToken"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected override void FillData(JToken data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            base.FillData(data);
            if (data["datatype"] != null)
            {
                this.DataType = (string)data["datatype"];
            }
        }

        /// <summary>
        /// Gets the type identifier of the type at server side.
        /// </summary>
        /// <returns>The type identifier.</returns>
        protected override string GetEntityType()
        {
            return "property";
        }


        
        /// <summary>
        /// Save all changes.
        /// </summary>
        /// <param name="summary">The edit summary.</param>
        public override void Save(string summary)
        {
            EntityStatus currentStatus = Status;
            switch (currentStatus)
            {
                case EntityStatus.New:
                    this.changes[DataTypeJsonName] = new JValue(DataType);
                    break;
            }

            base.Save(summary);

            // Clears the dirty sets
        }
    }
}