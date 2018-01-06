using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Wikibase
{
    /// <summary>
    /// Class for getting entities from various requests.
    /// </summary>
    public class EntityProvider
    {
        private WikibaseApi _api;

        /// <summary>
        /// Creates a new <see cref="EntityProvider"/>.
        /// </summary>
        /// <param name="api">The api.</param>
        public EntityProvider(WikibaseApi api)
        {
            _api = api;
        }

        /// <summary>
        /// Get the entities from the given entity ids.
        /// </summary>
        /// <param name="ids">The entity ids.</param>
        /// <returns>The entities.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ids"/> is <c>null</c>.</exception>
        public Entity[] GetEntitiesFromIds(EntityId[] ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            return this.GetEntitiesFromIds(ids, null);
        }

        /// <summary>
        /// Get the entities from the given entity ids with data in the languages provided.
        /// </summary>
        /// <param name="ids">The entity ids.</param>
        /// <param name="languages">The languages. <c>null</c> to get all languages.</param>
        /// <returns>The entities.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ids"/> is <c>null</c>.</exception>
        public Entity[] GetEntitiesFromIds(EntityId[] ids, string[] languages)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            string[] prefixedIds = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                prefixedIds[i] = ids[i].PrefixedId;
            }
            return _api.GetEntitiesFromIds(prefixedIds, languages);
        }

        /// <summary>
        /// Get the entity from the given entity id.
        /// </summary>
        /// <param name="id">The entity id</param>
        /// <returns>The entity</returns>
        public Entity GetEntityFromId(EntityId id)
        {
            return this.GetEntityFromId(id, null);
        }

        /// <summary>
        /// Get the entity from the given entity id with data in the languages provided.
        /// </summary>
        /// <param name="id">The entity id</param>
        /// <param name="languages">The languages</param>
        /// <returns>The entity</returns>
        public Entity GetEntityFromId(EntityId id, string[] languages)
        {
            Entity[] entities = this.GetEntitiesFromIds(new EntityId[] { id }, languages);
            foreach (Entity entity in entities)
            {
                return entity;
            }
            return null;
        }

        /// <summary>
        /// Get the entities from the given sites and titles.
        /// </summary>
        /// <param name="sites">The sites</param>
        /// <param name="titles">The titles</param>
        /// <returns>The entities</returns>
        public Entity[] GetEntitiesFromSitelinks(string[] sites, string[] titles)
        {
            return this.GetEntitiesFromSitelinks(sites, titles, null);
        }

        /// <summary>
        /// Get the entities from the given sites and titles with data in the languages provided.
        /// </summary>
        /// <param name="sites">The sites</param>
        /// <param name="titles">The titles</param>
        /// <param name="languages">The languages</param>
        /// <returns>The entities</returns>
        public Entity[] GetEntitiesFromSitelinks(string[] sites, string[] titles, string[] languages)
        {
            return _api.GetEntitesFromSitelinks(sites, titles, languages);
        }

        /// <summary>
        /// Get the entity from the given site and title.
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="title">The title</param>
        /// <returns>The entity</returns>
        public Entity GetEntityFromSitelink(string site, string title)
        {
            return this.GetEntityFromSitelink(site, title, null);
        }

        /// <summary>
        /// Get the entity from the given site and title with data in the languages provided.
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="title">The title</param>
        /// <param name="languages">The languages</param>
        /// <returns>The entity</returns>
        public Entity GetEntityFromSitelink(string site, string title, string[] languages)
        {
            Entity[] entities = this.GetEntitiesFromSitelinks(new string[] { site }, new string[] { title }, languages);
            return entities.FirstOrDefault();
        }
    }
}