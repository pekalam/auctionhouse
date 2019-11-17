using Core.Query.ReadModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.Views
{
    public class CommonTags
    {
        [BsonId]
        public string Tag { get; set; }
        public TagOcccurrence[] WithTags { get; set; }
    }
}
