using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Data.MongoDB.Models;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Data.MongoDB
{
    public abstract class BaseRepository<T, TId> : IBaseRepository<T, TId> where T: class
    {

        private const string DefaultConnectionstringName = "MongoServerSettings";
        private const string DatabaseName = "pressford";

        private readonly MongoClient client;

        public BaseRepository()
        {
            client = new MongoClient(ConfigurationManager.ConnectionStrings[DefaultConnectionstringName].ConnectionString);                       
        }

        public IMongoCollection<T> Collection
        {
            get
            {
                return client.GetDatabase(DatabaseName).GetCollection<T>(typeof(T).Name.ToLower());
            }
        }

        public async Task<T> GetById(TId id)
        {
            return await Collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task<T> Add(T entity)
        {
            await Collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<T> Update(T entity)
        {
            var mongoEntity = entity as IMongoEntity;
            var filter = Builders<T>.Filter.Eq("_id", mongoEntity.Id);
            
            await Collection.ReplaceOneAsync(filter, entity);
            
            return entity;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await Collection.Find(arg => true).ToListAsync();
        }        

        public async Task Delete(TId id)
        {
            await this.Collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
        }
    }
}