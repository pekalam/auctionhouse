using System;
using System.IO;
using System.Xml;
using Core.Common.Domain.Categories;

namespace Infrastructure.Services
{
    public class CategoryNameServiceSettings
    {
        public string CategoriesFilePath { get; set; }
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
            using (var file = File.OpenRead(_categoryNameServiceSettings.CategoriesFilePath))
            {
                var xmlReader = XmlReader.Create(file);
                xmlReader.Read();
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
