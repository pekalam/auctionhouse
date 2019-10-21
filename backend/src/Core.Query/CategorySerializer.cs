using System.Collections.Generic;
using System.Linq;
using Core.Common.Domain.Categories;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Core.Query
{
    class CategorySerializer : SerializerBase<Category>
    {
        private readonly CategoryBuilder _categoryBuilder;

        public CategorySerializer(CategoryBuilder categoryBuilder)
        {
            _categoryBuilder = categoryBuilder;
        }

        public override Category Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            reader.ReadStartDocument();
            while (reader.ReadName() != "CategoryName")
            {
                reader.ReadInt32();
            }
            var categoryStr = reader.ReadString();
            List<string> categoryNames = categoryStr.Split(';').ToList();
            reader.ReadEndDocument();
            return _categoryBuilder.FromCategoryNamesList(categoryNames);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Category value)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();
            var str = "";
            int i = 0;
            Category next = value;
            while (next != null)
            {
                if (str.Length > 0)
                {
                    str += ';';
                }
                str += next.Name;
                writer.WriteInt32($"Category{i++}Id", value.CategoryId);
                next = next.SubCategory;
            }
            writer.WriteString("CategoryName", str);
            writer.WriteEndDocument();
        }
    }
}