using Core.Common.Domain.Categories;
using System.Xml;
using System.Xml.Schema;

namespace XmlCategoryTreeStore
{
    public class XmlCategoryNameStoreSettings
    {
        public string CategoriesFilePath { get; set; }
        public string SchemaFilePath { get; set; }
    }

    internal class XmlCategoryTreeStore : ICategoryTreeStore
    {
        private readonly XmlCategoryNameStoreSettings _categoryNameServiceSettings;
        private readonly CategoryTreeNode _root = new CategoryTreeNode(null, 0, null);

        public XmlCategoryTreeStore(XmlCategoryNameStoreSettings categoryNameServiceSettings)
        {
            var cwd = Directory.GetCurrentDirectory();
            _categoryNameServiceSettings = categoryNameServiceSettings;
        }

        private CategoryTreeNode ReadCategory(XmlReader xmlReader)
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
            var sc = new XmlSchemaSet();
            sc.Add("categories-ns", _categoryNameServiceSettings.SchemaFilePath);

            var settings = new XmlReaderSettings();
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

                var i = 0;
                while (xmlReader.ReadToFollowing("category"))
                {
                    ReadCategory(xmlReader);
                }
            }

        }

        public CategoryTreeNode GetCategoriesTree() => _root;
    }
}