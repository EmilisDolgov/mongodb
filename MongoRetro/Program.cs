using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace MongoRetro
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var mongoDb = client.GetDatabase("Retro");
            var collection = mongoDb.GetCollection<Review>("Reviews");
            //Embedded docs
            var propositions = collection
                .Aggregate()
                .Unwind(x => x.Propositions)
                .Group((new BsonDocument("_id", "$Propositions")))
                .ToList();
            //Aggregate to get average team rating for each sprint
            var sprintAvgRatings = collection
                .Aggregate()
                .Group(new BsonDocument { { "_id", "$SprintId" }, {"AverageTeamRating", new BsonDocument("$avg", "$TeamRating") } })
                .ToList();
            //Map-reduce to get average team rating for each sprint
            string map = @"function() {
                emit(this.SprintId, this.TeamRating)
            }";
            string reduce = @"function(sprintId, TeamRating) {
                return Array.avg(TeamRating);
            }";
            var options = new MapReduceOptions<Review, BsonDocument>
            {
                OutputOptions = MapReduceOutputOptions.Inline
            };
            var sprintAvgRatingsMapReduce = collection.MapReduce(map, reduce, options).ToList();
        }
    }
    public class Worker
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class Review
    {
        public ObjectId _id { get; set; }
        public string UserId { get; set; }
        public int SprintId { get; set;}
        public int MyRating { get; set; }
        public int TeamRating { get; set; }
        public List<Proposition> Propositions { get; set; }

    }

    public class Proposition
    {
        public string Text { get; set; }
        public string Type { get; set; }
    }
}
