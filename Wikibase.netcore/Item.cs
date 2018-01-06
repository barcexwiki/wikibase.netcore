using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase
{
    /// <summary>
    /// An item.
    /// </summary>
    public class Item : Entity
    {
        private Dictionary<string, string> _sitelinks = new Dictionary<string, string>();

        /// <summary>
        /// List of site codes whose sitelinks have changed
        /// </summary>
        protected HashSet<string> dirtySitelinks = new HashSet<string>();


        #region Json names

        /// <summary>
        /// The name of the <see cref="_sitelinks"/> property in the serialized json object.
        /// </summary>
        private const string SiteLinksJsonName = "sitelinks";

        /// <summary>
        /// The name of the site property of a sitelink in the serialized json object.
        /// </summary>
        private const string SiteLinksSiteJsonName = "site";

        /// <summary>
        /// The name of the title property of a sitelink in the serialized json object.
        /// </summary>
        private const string SiteLinksTitleJsonName = "title";

        /// <summary>
        /// The name of the bagdes property of a sitelink in the serialized json object.
        /// </summary>
        private const string SiteLinksBadgesJsonName = "badges";

        #endregion Json names

        /// <summary>
        /// Creates a new instance of <see cref="Item"/>.
        /// </summary>
        /// <param name="api">The api.</param>
        public Item(WikibaseApi api)
            : base(api)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Item"/> and fill it with <paramref name="data"/>.
        /// </summary>
        /// <param name="api">The api.</param>
        /// <param name="data">Json object to be parsed and added.</param>
        internal Item(WikibaseApi api, JToken data)
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
            if (data[SiteLinksJsonName] != null)
            {
                _sitelinks.Clear();
                JToken jsonSiteLinks = data[SiteLinksJsonName];
                if (jsonSiteLinks != null && jsonSiteLinks.Type == JTokenType.Object )
                {
                    foreach (JProperty member in jsonSiteLinks)
                    {                        
                        _sitelinks.Add((string)member.Value[SiteLinksSiteJsonName], (string)member.Value[SiteLinksTitleJsonName]);
                        // ToDo: parse badges
                    }
                }
            }
        }

        /// <summary>
        /// Get all sitelinks.
        /// </summary>
        /// <returns>The sitelinks.</returns>
        /// <remarks>Key is the project name, value the page name. To modify the sitelinks, don't modify this dictionary, but use
        /// <see cref="SetSitelink"/> and <see cref="RemoveSitelink"/>.</remarks>
        public Dictionary<string, string> GetSitelinks()
        {
            return new Dictionary<string, string>(_sitelinks);
        }

        /// <summary>
        /// Get the sitelink for the given site.
        /// </summary>
        /// <param name="site">The site</param>
        /// <returns></returns>
        public string GetSitelink(string site)
        {
            return _sitelinks[site];
        }

        /// <summary>
        /// Set the sitelink for the given site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="title">The sitelink.</param>
        public void SetSitelink(string site, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("empty title");
            if (string.IsNullOrWhiteSpace(site))
                throw new ArgumentException("empty site");

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot remove alias from an entity with status " + Status);
            }

            _sitelinks[site] = title;
            this.dirtySitelinks.Add(site);
            Touch();
        }

        /// <summary>
        /// Remove the sitelink for the given site.
        /// </summary>
        /// <param name="site">The site</param>
        /// <returns><c>true</c> if the sitelink was removed successfully, <c>false</c> otherwise.</returns>
        public bool RemoveSitelink(string site)
        {

            if (!IsTouchable())
            {
                throw new InvalidOperationException("Cannot remove a site link from an entity with status " + Status);
            }

            if (_sitelinks.Remove(site))
            {
                this.dirtySitelinks.Add(site);
                Touch();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the type identifier of the type at server side.
        /// </summary>
        /// <returns>The type identifier.</returns>
        protected override string GetEntityType()
        {
            return "item";
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
                case EntityStatus.Changed:
                case EntityStatus.New:
                    if (dirtySitelinks.Count > 0)
                    {
                        if (this.changes[SiteLinksJsonName] == null)
                        {
                            this.changes[SiteLinksJsonName] = new JObject();
                        }

                        foreach (string site in dirtySitelinks)
                        {
                            string sitelinkValue = "";

                            // If there is label the text is the label itself, if not is a removed label (i.e. empty)
                            if (_sitelinks.ContainsKey(site))
                            {
                                sitelinkValue = _sitelinks[site];
                            }

                            this.changes[SiteLinksJsonName][site] = new JObject
                                {
                                    {SiteLinksSiteJsonName, site},
                                    {SiteLinksTitleJsonName, sitelinkValue}
                                };
                                
                        }
                    }
                    break;
            }

            base.Save(summary);

            // Clears the dirty sets
            dirtySitelinks.Clear();
        }

        protected override void Clear()
        {
            _sitelinks.Clear();
            dirtySitelinks.Clear();
            base.Clear();
        }
    }
}