using System.Text.Json.Serialization;

namespace ModernAPI.Models
{
    public class EditAssetModel
    {
        [JsonPropertyName("assetID")] public int AssetID { get; set; }
        [JsonPropertyName("assetName")] public string? AssetName { get; set; }
        [JsonPropertyName("manufactureSN")] public string? ManufactureSN { get; set; }
        [JsonPropertyName("weight")] public double Weight { get; set; }
        [JsonPropertyName("price")] public double Price { get; set; }
        [JsonPropertyName("classRefID")] public int ClassRefID { get; set; }
        [JsonPropertyName("classRefName")] public string? ClassRefName { get; set; }
        [JsonPropertyName("parentClassRefID")] public int ParentClassRefID { get; set; }
        [JsonPropertyName("parentClassRefName")] public string? ParentClassRefName { get; set; }
        [JsonPropertyName("locationRefID")] public int LocationRefID { get; set; }
        [JsonPropertyName("locationRefName")] public string? LocationRefName { get; set; }
        [JsonPropertyName("parentLocationRefID")] public int ParentLocationRefID { get; set; }
        [JsonPropertyName("parentLocationRefName")] public string? ParentLocationRefName { get; set; }
        [JsonPropertyName("assetStateID")] public int AssetStateID { get; set; }
        [JsonPropertyName("weightUnitID")] public int WeightUnitID { get; set; }
        [JsonPropertyName("assetUnitID")] public int AssetUnitID { get; set; }
        [JsonPropertyName("gurdianShipID")] public int? GuardianShipID { get; set; }
        [JsonPropertyName("quantity")] public double Quantity { get; set; }
        [JsonPropertyName("isContainer")] public bool IsContainer { get; set; }
        [JsonPropertyName("assetCode")] public int AssetCode { get; set; }
        [JsonPropertyName("assetGroupCode")] public string? AssetGroupCode { get; set; }
        [JsonPropertyName("originalPrice")] public double? OriginalPrice { get; set; }
        [JsonPropertyName("costPrice")] public double? CostPrice { get; set; }
        [JsonPropertyName("salePrice")] public double? SalePrice { get; set; }
        [JsonPropertyName("extraUnitCost")] public double? ExtraCost { get; set; }
        [JsonPropertyName("profitUnitAmount")] public double? Profit { get; set; }
        [JsonPropertyName("assetPhoto")] public string? AssetPhoto { get; set; }
        [JsonPropertyName("tagList")] public List<AssetTag>? TagList { get; set; }
        [JsonPropertyName("isRecieved")] public bool IsReceived { get; set; }
        [JsonPropertyName("isPennding")] public bool IsPending { get; set; }
    }

    public class AssetTag
    {
        [JsonPropertyName("tagID")] public string? TagID { get; set; }
        [JsonPropertyName("tagTypeKey")] public string? TagTypeKey { get; set; }
    }
}





