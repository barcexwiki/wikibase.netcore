using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinimalJson;

namespace Wikibase
{
    /// <summary>
    /// An entity
    /// </summary>
    public abstract class Entity
    {
        private enum AliasStatus
        {
            Existing,
            New,
            Removed
        }
        public enum EntityStatus
        {
            New,
            Loaded,
            Changed,
            ToBeDeleted,
            Deleted
        }

        private class EntityAlias
        {
            public string Language { get; set; }
            public string Label { get; set; }
            public AliasStatus Status { get; set; }

            public EntityAlias(string language, string label, AliasStatus status)
            {
                Language = language;
                Label = label;
                Status = status;
            }
        }


        /// <summary>
        /// The entity object Status (New, Loaded, Changed, Deleted)
        /// </summary>
        public EntityStatus Status
        {
            get;
            protected set;
        }

        /// <summary>
        /// The entity id
        /// </summary>
        public EntityId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// The last revision id
        /// </summary>
        public int LastRevisionId
        {
            get;
            set;
        }

        /// <summary>
        /// The api
        /// </summary>
        public WikibaseApi Api
        {
            get;
            private set;
        }

        /// <summary>
        /// Labels, the actual name. Key is the language editifier, value the label.
        /// </summary>
        private Dictionary<String, String> _labels = new Dictionary<String, String>();

        /// <summary>
        /// Descriptions, to explain the item. Key is the language editifier, value the description.
        /// </summary>
        private Dictionary<String, String> _descriptions = new Dictionary<String, String>();

        /// <summary>
        /// Aliases.
        /// </summary>
        private List<EntityAlias> _aliases = new List<EntityAlias>();

        /// <summary>
        /// Claims. 
        /// </summary>
        private List<Claim> _claims = new List<Claim>();

        /// <summary>
        /// Changes cache.
        /// </summary>
        protected JsonObject changes = new JsonObject();

        /// <summary>
        /// List of languages codes whose labels have changed
        /// </summary>
        protected HashSet<string> dirtyLabels = new HashSet<string>();

        /// <summary>
        /// List of languages codes whose descriptions have changed
        /// </summary>
        protected HashSet<string> dirtyDescriptions = new HashSet<string>();


        /// <summary>
        /// Constructor creating a blank entity instance.
        /// </summary>
        /// <param name="api">The api.</param>
        public Entity(WikibaseApi api)
        {
            this.Api = api;
            this.FillData(new JsonObject());
            Status = EntityStatus.New;
        }

        /// <summary>
        /// Constructor creating an entitiy from a Json object.
        /// </summary>
        /// <param name="api">The api</param>
        /// <param name="data">The json object to be parsed.</param>
        internal Entity(WikibaseApi api, JsonObject data)
        {
            this.Api = api;
            this.FillData(data);
            Status = EntityStatus.Loaded;
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JsonObject"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected virtual void FillData(JsonObject data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            // Clears the dirty sets
            dirtyLabels.Clear();
            dirtyDescriptions.Clear();

            if (data.get("id") != null)
            {
                this.Id = new EntityId(data.get("id").asString());
            }
            if (data.get("lastrevid") != null)
            {
                this.LastRevisionId = data.get("lastrevid").asInt();
            }
            JsonValue returnedLabels = data.get("labels");
            if ((returnedLabels != null) && (returnedLabels.isObject()))
                if (data.get("labels") != null)
                {
                    _labels.Clear();
                    foreach (JsonObject.Member member in returnedLabels.asObject())
                    {
                        JsonObject obj = member.value.asObject();
                        _labels.Add(obj.get("language").asString(), obj.get("value").asString());
                    }
                }
            JsonValue returnedDescriptions = data.get("descriptions");
            if ((returnedDescriptions != null) && (returnedDescriptions.isObject()))
            {
                _descriptions.Clear();
                foreach (JsonObject.Member member in returnedDescriptions.asObject())
                {
                    JsonObject obj = member.value.asObject();
                    _descriptions.Add(obj.get("language").asString(), obj.get("value").asString());
                }
            }
            JsonValue returnedAliases = data.get("aliases");
            if ((returnedAliases != null) && (returnedAliases.isObject()))
            {
                // strange - after save an empty array is returned, whereas by a normal get the fully alias list is returned
                _aliases.Clear();
                foreach (JsonObject.Member member in returnedAliases.asObject())
                {
                    List<String> list = new List<String>();
                    foreach (JsonValue value in member.value.asArray())
                    {
                        _aliases.Add(new EntityAlias(member.name, value.asObject().get("value").asString(), AliasStatus.Existing));
                    }
                }
            }
            JsonValue returnedClaims = data.get("claims");
            if ((returnedClaims != null) && (returnedClaims.isObject()))
            {
                _claims.Clear();
                foreach (JsonObject.Member member in returnedClaims.asObject())
                {
                    foreach (JsonValue value in member.value.asArray())
                    {
                        Claim claim = Claim.NewFromArray(this, value.asObject());
                        _claims.Add(claim);
                    }
                }
            }
        }

        internal static Entity NewFromArray(WikibaseApi api, JsonObject data)
        {
            if (data.get("type") != null)
            {
                switch (data.get("type").asString())
                {
                    case "item":
                        return new Item(api, data);
                    case "property":
                        return new Property(api, data);
                }
            }
            throw new Exception("Unknown type");
        }

        /// <summary>
        /// Get all labels.
        /// </summary>
        /// <returns>The labels</returns>
        /// <remarks>Key is the language, value the label.</remarks>
        public Dictionary<String, String> GetLabels()
        {
            return new Dictionary<String, String>(_labels);
        }

        /// <summary>
        /// Get the label for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns>The label.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public String GetLabel(String lang)
        {
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language");
            return _labels.ContainsKey(lang) ? _labels[lang] : null;
        }


        /// <summary>
        /// Set the label for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The label.</param>
        /// <exception cref="ArgumentException"><paramref name="lang"/> or <paramref name="value"/> is empty string or <c>null</c>.</exception>
        public void SetLabel(String lang, String value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("empty description");
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language");

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot set a label for an entity with status " + Status);
            }

            if (GetLabel(lang) != value)
            {
                _labels[lang] = value;
                this.dirtyLabels.Add(lang);
                Touch();
            }
        }

        /// <summary>
        /// Remove the label for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns><c>true</c> if the label was removed successfully, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public Boolean RemoveLabel(String lang)
        {
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language");

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot remove a label from an entity with status " + Status);
            }

            if (_labels.Remove(lang))
            {
                this.dirtyLabels.Add(lang);
                Touch();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all descriptions.
        /// </summary>
        /// <returns>The descriptions.</returns>
        /// <remarks>Keys is the language, value the description.</remarks>
        public Dictionary<String, String> GetDescriptions()
        {
            return new Dictionary<String, String>(_descriptions);
        }

        /// <summary>
        /// Get the description for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns>The description.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public string GetDescription(String lang)
        {
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language");
            return _descriptions.ContainsKey(lang) ? _descriptions[lang] : null;
        }

        /// <summary>
        /// Set the description for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The description.</param>
        /// <exception cref="ArgumentException"><paramref name="lang"/> or <paramref name="value"/> is empty string or <c>null</c>.</exception>
        public void SetDescription(String lang, String value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("empty description");
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language");

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot set a description for an entity with status " + Status);
            }

            if (GetDescription(lang) != value)
            {
                _descriptions[lang] = value;
                this.dirtyDescriptions.Add(lang);
                Touch();
            }
        }

        /// <summary>
        /// Remove the description for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns><c>true</c> if the description was removed successfully, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public Boolean RemoveDescription(String lang)
        {
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language");

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot remove a description from an entity with status " + Status);
            }

            if (_descriptions.Remove(lang))
            {
                this.dirtyDescriptions.Add(lang);
                Touch();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all aliases.
        /// </summary>
        /// <returns>The aliases</returns>
        /// <value>Key is the language, value a list of aliases.</value>
        public Dictionary<String, List<String>> GetAliases()
        {
            IEnumerable<EntityAlias> filtered = from a in _aliases
                                                where a.Status == AliasStatus.Existing || a.Status == AliasStatus.New
                                                select a;

            Dictionary<String, List<String>> result = new Dictionary<string, List<string>>();

            foreach (EntityAlias a in filtered)
            {
                if (!result.ContainsKey(a.Language))
                    result[a.Language] = new List<string>();

                result[a.Language].Add(a.Label);
            }

            return result;
        }


        /// <summary>
        /// Get the aliases for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns>The aliases, or <c>null</c> if no aliases are defined for the language.</returns>
        public string[] GetAliases(string lang)
        {
            IEnumerable<string> filtered = from a in _aliases
                                           where a.Language == lang && (a.Status == AliasStatus.Existing || a.Status == AliasStatus.New)
                                           select a.Label;

            //return filtered.Any() ? filtered.ToArray() : null;
            return filtered.ToArray();
        }


        /// <summary>
        /// Add an alias for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The alias.</param>
        public void AddAlias(string lang, string value)
        {
            IEnumerable<EntityAlias> filtered = from a in _aliases
                                                where a.Language == lang && a.Label == value
                                                select a;

            EntityAlias alias = filtered.FirstOrDefault();

            if (alias != null)
            {
                if (alias.Status == AliasStatus.Removed)
                    alias.Status = AliasStatus.Existing;
            }
            else
            {
                _aliases.Add(new EntityAlias(lang, value, AliasStatus.New));
                Touch();
            }
        }


        /// <summary>
        /// Remove the alias for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The alias.</param>
        /// <returns><c>true</c> if the alias was removed successfully, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> or <paramref name="value"/> is empty string or <c>null</c>.</exception>
        public void RemoveAlias(string lang, string value)
        {
            if (String.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language", "lang");
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("empty value", "value");

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot remove an alias from an entity with status " + Status);
            }

            IEnumerable<EntityAlias> filtered = from a in _aliases
                                                where a.Language == lang && a.Label == value
                                                select a;

            EntityAlias alias = filtered.FirstOrDefault();

            if (alias != null)
            {
                switch (alias.Status)
                {
                    case AliasStatus.Existing:
                        alias.Status = AliasStatus.Removed;
                        break;
                    case AliasStatus.New:
                        _aliases.Remove(alias);
                        break;
                }
                Touch();
            }
        }


        /// <summary>
        /// Gets all claims.
        /// </summary>
        /// <value>All claims.</value>
        public IEnumerable<Claim> Claims
        {
            get
            {
                return _claims.Where(c => c.status == Claim.ClaimStatus.Existing || c.status == Claim.ClaimStatus.New);
            }
        }

        /// <summary>
        /// Get the claims for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The claims.</returns>
        public Claim[] GetClaims(String property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var claimList = from c in Claims
                            where c.MainSnak.PropertyId.PrefixedId.ToUpper() == property.ToUpper()
                            select c;

            return claimList.ToArray();
        }

        /// <summary>
        /// Remove the claim.
        /// </summary>
        /// <param name="claim">The claim.</param>
        /// <returns><c>true</c> if the claim was removed successfully, <c>false</c> otherwise.</returns>
        public bool RemoveClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot remove a claim from an entity with status " + Status);
            }

            if (_claims.Contains(claim))
            {
                if (claim.status == Claim.ClaimStatus.New)
                {
                    _claims.Remove(claim);
                }
                claim.Delete();
                Touch();
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void Delete()
        {            
            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot delete an entity with status "+Status);
            }

            if (Status == EntityStatus.New)
            {
                Status = EntityStatus.Deleted;
            } else
            {
                Status = EntityStatus.ToBeDeleted;
            }

            Clear();

        }

        private bool SaveClaims()
        {
            Claim[] claimsToProcess = _claims.ToArray();

            bool touched = false;

            foreach (Claim c in claimsToProcess)
            {
                switch (c.status)
                {
                    case Claim.ClaimStatus.Deleted:
                        c.Save("");
                        _claims.Remove(c);
                        touched = true;
                        break;
                    case Claim.ClaimStatus.Modified:
                    case Claim.ClaimStatus.New:
                        c.RefreshId();                     
                        c.Save("");
                        touched = true;
                        break;
                }
            }

            return touched;
        }

        /// <summary>
        /// Save all changes.
        /// </summary>
        /// <param name="summary">The edit summary.</param>
        public virtual void Save(String summary)
        {
            if (summary == null)
            {
                summary = "";
            }

            EntityStatus currentStatus = Status;
            switch (currentStatus)
            {
                case EntityStatus.ToBeDeleted:
                    Api.DeleteEntity(this.GetEntityType() + ":" + Id.PrefixedId, LastRevisionId, summary);
                    Status = EntityStatus.Deleted;
                    break;
                case EntityStatus.Changed:
                case EntityStatus.New:

                    if (dirtyLabels.Count > 0)
                    {
                        if (this.changes.get("labels") == null)
                        {
                            this.changes.set("labels", new JsonObject());
                        }

                        foreach (string lang in dirtyLabels)
                        {
                            string labelValue = "";

                            // If there is label the text is the label itself, if not is a removed label (i.e. empty)
                            if (_labels.ContainsKey(lang))
                            {
                                labelValue = _labels[lang];
                            }

                            this.changes.get("labels").asObject().set(
                                lang,
                                new JsonObject()
                                    .add("language", lang)
                                    .add("value", labelValue)
                            );
                        }
                    }

                    if (dirtyDescriptions.Count > 0)
                    {
                        if (this.changes.get("descriptions") == null)
                        {
                            this.changes.set("descriptions", new JsonObject());
                        }

                        foreach (string lang in dirtyDescriptions)
                        {
                            string descriptionValue = "";

                            // If there is description the text is the label itself, if not is a removed label (i.e. empty)
                            if (_descriptions.ContainsKey(lang))
                            {
                                descriptionValue = _descriptions[lang];
                            }

                            this.changes.get("descriptions").asObject().set(
                                lang,
                                new JsonObject()
                                    .add("language", lang)
                                    .add("value", descriptionValue)
                            );
                        }
                    }


                    // Process aliases changes
                    IEnumerable<EntityAlias> aliasesToSave = from a in _aliases
                                                                where a.Status == AliasStatus.New || a.Status == AliasStatus.Removed
                                                                select a;

                    if (this.changes.get("aliases") == null && aliasesToSave.Any())
                    {
                        this.changes.set("aliases", new JsonArray());
                    }
                    foreach (EntityAlias a in aliasesToSave)
                    {
                        JsonObject jsonAlias = new JsonObject()
                            .add("language", a.Language)
                            .add("value", a.Label)
                            .add(a.Status == AliasStatus.New ? "add" : "remove", true);

                        this.changes.get("aliases").asArray().add(jsonAlias);
                    }


                    JsonObject result;

                    if (this.Id == null)
                    {
                        result = this.Api.CreateEntity(this.GetEntityType(), this.changes, this.LastRevisionId, summary);

                        if (result.get("entity") != null)
                        {
                            JsonObject data = result.get("entity").asObject();
                            if (data.get("id") != null)
                            {
                                this.Id = new EntityId(data.get("id").asString());
                            }
                        }
                    }
                    else
                    {
                        result = this.Api.EditEntity(this.Id.PrefixedId, this.changes, this.LastRevisionId, summary);
                    }


                    if (SaveClaims())
                    {
                        JsonObject entity = this.Api.GetEntityJsonFromId(this.Id);
                        this.FillData(entity);
                        Status = EntityStatus.Loaded;
                    }
                    else
                    {
                        if (result.get("entity") != null)
                        {
                            this.FillData(result.get("entity").asObject());
                            Status = EntityStatus.Loaded;
                        }
                    }


                    this.UpdateLastRevisionIdFromResult(result);
                    this.changes = new JsonObject();

                    break;
            }
        }

        internal void UpdateLastRevisionIdFromResult(JsonObject result)
        {
            if (result.get("pageinfo") != null && result.get("pageinfo").asObject().get("lastrevid") != null)
            {
                this.LastRevisionId = result.get("pageinfo").asObject().get("lastrevid").asInt();
            }
        }

        public Statement AddStatement(Snak snak, Rank rank)
        {

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot add a statement to an entity with status " + Status);
            }

            Statement s = new Statement(this, snak, rank);
            _claims.Add(s);
            Touch();
            return s;
        }

        /// <summary>
        /// Gets the type identifier of the type at server side.
        /// </summary>
        /// <returns>The type identifier.</returns>
        protected abstract String GetEntityType();


        protected bool IsTouchable()
        {
            return (Status != EntityStatus.Deleted && Status != EntityStatus.ToBeDeleted);
        }


        internal void Touch()
        {
            EntityStatus currentStatus = Status;
            switch (currentStatus)
            {
                case EntityStatus.Changed:
                    break;
                case EntityStatus.New:
                    break;
                case EntityStatus.Loaded:
                    Status = EntityStatus.Changed;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        protected virtual void Clear()
        {
            _aliases.Clear();
            _claims.Clear();
            _descriptions.Clear();
            _labels.Clear();
            _aliases.Clear();
            dirtyDescriptions.Clear();
            dirtyLabels.Clear();
        }

    }
}