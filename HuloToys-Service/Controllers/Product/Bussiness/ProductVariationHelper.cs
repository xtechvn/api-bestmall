using Entities.ViewModels.Products;
using System.Text;

namespace HuloToys_Service.Controllers.Product.Bussiness
{
    public static class ProductVariationHelper
    {
        public static string RenderVariationDetail(List<ProductAttributeMongoDbModel> attribute, List<ProductAttributeMongoDbModelItem> attributeDetail,
         List<ProductDetailVariationAttributesMongoDbModel> variation_detail)
        {
            if (attribute == null || attributeDetail == null || attribute.Count == 0 || attributeDetail.Count == 0)
            {
                return "Mặc định";
            }

            StringBuilder variationValueBuilder = new StringBuilder();
            if (variation_detail != null && variation_detail.Count > 0)
            {
                foreach (var variationItem in variation_detail)
                {
                    var selected_attribute = attribute.FirstOrDefault(attr => attr._id == variationItem.id);
                    var selected_attributeDetail = attributeDetail.FirstOrDefault(detail => detail.name == variationItem.name);
                    if (selected_attribute != null && selected_attributeDetail != null)
                    {
                        variationValueBuilder.Append($"{selected_attribute.name}:{selected_attributeDetail.name}");
                        if (variation_detail.IndexOf(variationItem) < (variation_detail.Count - 1))
                        {
                            variationValueBuilder.Append(", ");
                        }
                    }
                }
            }
            else
            {
                variationValueBuilder.Append("Mặc định");
            }

            return variationValueBuilder.ToString();
        }
    }
}
