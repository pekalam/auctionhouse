using MongoDB.Bson.Serialization.Attributes;

namespace ReadModel.Contracts.Views
{
    public class CommonTags
    {
        [BsonId]
        public string Tag { get; set; }
        public TagOcccurrence[] WithTags { get; set; }
    }
}
