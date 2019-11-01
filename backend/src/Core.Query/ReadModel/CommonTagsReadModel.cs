using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.ReadModel
{
    public class TagOcccurrence
    {
        public string Tag { get; set; }
        public int Times { get; set; }
    }

    public class CommonTagsReadModel
    {
        [BsonId]
        public string Tag { get; set; }
        public TagOcccurrence[] WithTags { get; set; }
    }
}
