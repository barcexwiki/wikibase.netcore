using System;
using System.Collections.Generic;
using System.Text;
using MinimalJson;

namespace Wikibase
{
    /// <summary>
    /// An item.
    /// </summary>
    public class Item : Entity
    {
        private Dictionary<String, String> _sitelinks = new Dictionary<String, String>();

        /// <summary>
        /// List of site codes whose sitelinks have changed
        /// </summary>
        protected HashSet<string> dirtySitelinks = new HashSet<string>();


        #region Json names

        /// <summary>
        /// The name of the <see cref="_sitelinks"/> property in the serialized json object.
        /// </summary>
        private const String SiteLinksJsonName = "sitelinks";

        /// <summary>
        /// The name of the site property of a sitelink in the serialized json object.
        /// </summary>
        private const String SiteLinksSiteJsonName = "site";

        /// <summary>
        /// The name of the title property of a sitelink in the serialized json object.
        /// </summary>
        private const String SiteLinksTitleJsonName = "title";

        /// <summary>
        /// The name of the bagdes property of a sitelink in the serialized json object.
        /// </summary>
        private const String SiteLinksBadgesJsonName = "badges";

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
        internal Item(WikibaseApi api, JsonObject data)
            : base(api, data)
        {
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JsonObject"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected override void FillData(JsonObject data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            base.FillData(data);
            if (data.get(SiteLinksJsonName) != null)
            {
                _sitelinks.Clear();
                var jasonSiteLinks = data.get(SiteLinksJsonName);
                if (jasonSiteLinks != null && jasonSiteLinks.isObject())
                {
                    foreach (JsonObject.Member member in jasonSiteLinks.asObject())
                    {
                        JsonObject obj = member.value.asObject();
                        _sitelinks.Add(obj.get(SiteLinksSiteJsonName).asString(), obj.get(SiteLinksTitleJsonName).asString());
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
        public Dictionary<String, String> GetSitelinks()
        {
            return new Dictionary<String, String>(_sitelinks);
        }

        /// <summary>
        /// Get the sitelink for the given site.
        /// </summary>
        /// <param name="site">The site</param>
        /// <returns></returns>
        public String GetSitelink(String site)
        {
            return _sitelinks[site];
        }

        /// <summary>
        /// Set the sitelink for the given site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="title">The sitelink.</param>
        public void SetSitelink(String site, String title)
        {
            if (String.IsNullOrWhiteSpace(title))
                throw new ArgumentException("empty title");
            if (String.IsNullOrWhiteSpace(site))
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
        public Boolean RemoveSitelink(String site)
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
        protected override String GetEntityType()
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
                        if (this.changes.get(SiteLinksJsonName) == null)
                        {
                            this.changes.set(SiteLinksJsonName, new JsonObject());
                        }

                        foreach (string site in dirtySitelinks)
                        {
                            string sitelinkValue = "";

                            // If there is label the text is the label itself, if not is a removed label (i.e. empty)
                            if (_sitelinks.ContainsKey(site))
                            {
                                sitelinkValue = _sitelinks[site];
                            }

                            this.changes.get(SiteLinksJsonName).asObject().set(
                                site,
                                new JsonObject()
                                    .add(SiteLinksSiteJsonName, site)
                                    .add(SiteLinksTitleJsonName, sitelinkValue)
                            );
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