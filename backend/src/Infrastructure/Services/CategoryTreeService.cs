using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Core.Common.Domain.Categories;

namespace Infrastructure.Services
{
    public class CategoryNameServiceSettings
    {
        public string CategoriesFilePath { get; set; }
        public string SchemaFilePath { get; set; }
    }

    public class CategoryTreeService : ICategoryTreeService
    {
        private readonly CategoryNameServiceSettings _categoryNameServiceSettings;
        private readonly CategoryTreeNode _root = new CategoryTreeNode(null, 0, null);

        public CategoryTreeService(CategoryNameServiceSettings categoryNameServiceSettings)
        {
            _categoryNameServiceSettings = categoryNameServiceSettings;
        }

        private CategoryTreeNode ReadCategory(XmlReader xmlReader, ref int i)
        {
            var categoryName = xmlReader.GetAttribute("name");
            var categoryId = int.Parse(xmlReader.GetAttribute("id"));
            var catNode = new CategoryTreeNode(categoryName, categoryId, _root);
            xmlReader.ReadToDescendant("sub-category");
            do
            {
                categoryName = xmlReader.GetAttribute("name");
                categoryId = int.Parse(xmlReader.GetAttribute("id"));
                var subCat = new CategoryTreeNode(categoryName, categoryId, catNode);
                xmlReader.ReadToDescendant("sub-sub-category");
                do
                {
                    categoryName = xmlReader.GetAttribute("name");
                    categoryId = int.Parse(xmlReader.GetAttribute("id"));
                    new CategoryTreeNode(categoryName, categoryId, subCat);
                } while (xmlReader.ReadToNextSibling("sub-sub-category"));
            } while (xmlReader.ReadToNextSibling("sub-category"));

            return catNode;
        }

        public void Init()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("categories-ns", _categoryNameServiceSettings.SchemaFilePath);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(sc);

            using (var file = File.OpenRead(_categoryNameServiceSettings.CategoriesFilePath))
            {
                var xmlReader = XmlReader.Create(file, settings);

                xmlReader.ReadToFollowing("categories");
                if (xmlReader.Name != "categories")
                {
                    throw new Exception("Invalid root node name");
                }

                int i = 0;
                while (xmlReader.ReadToFollowing("category"))
                {
                    ReadCategory(xmlReader, ref i);
                }
            }

        }

        public CategoryTreeNode GetCategoriesTree() => _root;
    }
}
