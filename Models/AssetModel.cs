using System.Text.Json.Serialization;

namespace ModernAPI.Models
{
    public class AssetModel
    {
        public class Asset
        {
            [JsonPropertyName("assetID")]
            public int AssetID { get; set; }

            [JsonPropertyName("assetName")]
            public string? AssetName { get; set; }

            [JsonPropertyName("weight")]
            public double? Weight { get; set; }

            [JsonPropertyName("price")]
            public float? Price { get; set; }

            [JsonPropertyName("formID")]
            public int? FormID { get; set; }

            [JsonPropertyName("classID")]
            public int? ClassID { get; set; }

            [JsonPropertyName("locationID")]
            public int? LocationID { get; set; }

            [JsonPropertyName("manufactureSN")]
            public string? ManufactureSN { get; set; }

            [JsonPropertyName("assetStateID")]
            public int? AssetStateID { get; set; }

            [JsonPropertyName("weightUnitID")]
            public int? WeightUnitID { get; set; }

            [JsonPropertyName("assetUnitID")]
            public int? AssetUnitID { get; set; }

            [JsonPropertyName("gurdianShipID")]
            public int? GurdianShipID { get; set; }

            [JsonPropertyName("quantity")]
            public int? Quantity { get; set; }

            [JsonPropertyName("isContainer")]
            public bool? IsContainer { get; set; }

            [JsonPropertyName("assetCode")]
            public int? AssetCode { get; set; }  

            [JsonPropertyName("assetGroupCode")]
            public string? AssetGroupCode { get; set; }

            [JsonPropertyName("originalPrice")]
            public double? OriginalPrice { get; set; }

            [JsonPropertyName("costPrice")]
            public double? CostPrice { get; set; }

            [JsonPropertyName("salePrice")]
            public double? SalePrice { get; set; }

            [JsonPropertyName("extraUnitCost")]
            public double? ExtraUnitCost { get; set; }

            [JsonPropertyName("profitUnitAmount")]
            public double? ProfitUnitAmount { get; set; }

            [JsonPropertyName("assetPhoto")]
            public string? AssetPhoto { get; set; }

            [JsonPropertyName("tagList")]
            public List<Tag>? TagList { get; set; }

            [JsonPropertyName("isRecieved")]
            public bool IsRecieved { get; set; }

            [JsonPropertyName("isPennding")]
            public bool? IsPennding { get; set; }

            [JsonPropertyName("location")]
            public Location? Location { get; set; }

            [JsonPropertyName("classification")]
            public Classification? Classification { get; set; }

            [JsonPropertyName("assetGuardian")]
            public AssetGuardian? AssetGuardian { get; set; }
        }

        public class Tag
        {
            [JsonPropertyName("tagID")]
            public string TagID { get; set; } = string.Empty;

            [JsonPropertyName("showTagID")]
            public string ShowTagID { get; set; } = string.Empty;

            [JsonPropertyName("tagType")]
            public string TagType { get; set; } = string.Empty;

            [JsonPropertyName("tagTypeKey")]
            public string TagTypeKey { get; set; } = string.Empty;
        }

        public class Location
        {
            [JsonPropertyName("locationID")]
            public int LocationID { get; set; }

            [JsonPropertyName("locationRefID")]
            public string? LocationRefID { get; set; }

            [JsonPropertyName("parentLocationID")]
            public int? ParentLocationID { get; set; }

            [JsonPropertyName("locationName")]
            public string? LocationName { get; set; }

            [JsonPropertyName("locationNameEn")]
            public string? LocationNameEn { get; set; }

            [JsonPropertyName("locationFullName")]
            public string? LocationFullName { get; set; }

            [JsonPropertyName("locationParentPath")]
            public string? LocationParentPath { get; set; }

            [JsonPropertyName("locationCode")]
            public string? LocationCode { get; set; }

            [JsonPropertyName("levelNodeCode")]
            public string? LevelNodeCode { get; set; }

            [JsonPropertyName("locationTypeID")]
            public int? LocationTypeID { get; set; }

            [JsonPropertyName("hasChiled")]
            public bool? HasChiled { get; set; }

            [JsonPropertyName("isfav")]
            public bool? Isfav { get; set; }

            [JsonPropertyName("isAuthrize")]
            public bool? IsAuthrize { get; set; }
        }

        public class Classification
        {
            [JsonPropertyName("classID")]
            public int ClassID { get; set; }

            [JsonPropertyName("classRefId")]
            public int? ClassRefId { get; set; }

            [JsonPropertyName("classParentID")]
            public int? ClassParentID { get; set; }

            [JsonPropertyName("className")]
            public string? ClassName { get; set; }

            [JsonPropertyName("classNameEn")]
            public string? ClassNameEn { get; set; }

            [JsonPropertyName("parentClassPath")]
            public string? ParentClassPath { get; set; }

            [JsonPropertyName("classCode")]
            public string? ClassCode { get; set; }

            [JsonPropertyName("classTypeID")]
            public int ClassTypeID { get; set; }

            [JsonPropertyName("classImageName")]
            public string? ClassImageName { get; set; }

            [JsonPropertyName("defualtPrice")]
            public double DefualtPrice { get; set; }

            [JsonPropertyName("defualtUnitPrice")]
            public double? DefualtUnitPrice { get; set; }

            [JsonPropertyName("defualtExtraUnitCost")]
            public double? DefualtExtraUnitCost { get; set; }

            [JsonPropertyName("defaultSalePrice")]
            public double? DefaultSalePrice { get; set; }

            [JsonPropertyName("defaultProfitUnitAmount")]
            public double? DefaultProfitUnitAmount { get; set; }

            [JsonPropertyName("vat")]
            public double? Vat { get; set; }

            [JsonPropertyName("defualtUnit")]
            public int? DefualtUnit { get; set; }

            [JsonPropertyName("weightDefualtUnit")]
            public int? WeightDefualtUnit { get; set; }

            [JsonPropertyName("hasChiled")]
            public bool? HasChiled { get; set; }

            [JsonPropertyName("isfav")]
            public bool? Isfav { get; set; }

            [JsonPropertyName("containerModeID")]
            public int? ContainerModeID { get; set; }

            [JsonPropertyName("containerHaveFixedExtraCost")]
            public bool? ContainerHaveFixedExtraCost { get; set; }
        }

        public class AssetGuardian
        {
            [JsonPropertyName("employeeID")]
            public int EmployeeID { get; set; }

            [JsonPropertyName("employeeName")]
            public string? EmployeeName { get; set; }

            [JsonPropertyName("employeeSocialID")]
            public string? EmployeeSocialID { get; set; }

            [JsonPropertyName("departmentID")]
            public int DepartmentID { get; set; }

            [JsonPropertyName("departmentName")]
            public string? DepartmentName { get; set; }
        }
    }
}