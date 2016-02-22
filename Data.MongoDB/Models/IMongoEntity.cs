using System;

namespace Data.MongoDB.Models
{
    public interface IMongoEntity
    {
        Guid Id { get; set; } 
    }
}