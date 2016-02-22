using System;
using System.Collections.Generic;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Data.MongoDB.Models
{
    public class Article : IMongoEntity
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        public string Title { get; set; }
 
        public string Body { get; set; }

        public DateTime? PublishDate { get; set; }

        public string Author { get; set; }

        public IList<Comment> Comments { get; set; }
    }
}