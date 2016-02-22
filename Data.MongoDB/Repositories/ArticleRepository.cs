using System;
using System.Threading.Tasks;

using Data.MongoDB.Models;

using MongoDB.Driver;

namespace Data.MongoDB.Repositories
{
    public class ArticleRepository : BaseRepository<Article, Guid>, IArticleRepository
    {
        public async Task UpdateArticle(Article article)
        {
            var filter = Builders<Article>.Filter.Eq(article1 => article1.Id, article.Id);

            var update =
                Builders<Article>.Update.Set(article1 => article1.Title, article.Title)
                    .Set(article1 => article1.Body, article.Body)
                    .Set(article1 => article1.Author, article.Author)
                    .Set(article1 => article1.PublishDate, article.PublishDate);

            await Collection.UpdateOneAsync(filter, update);            
        }
    }
}