namespace ModernAPI.Data
{
    public class AssetListModel
    {

    }

    public class Tag
    {
        public string TagID { get; set; }
        public string ShowTagID { get; set; }
        public string TagType { get; set; }
        public string TagTypeKey { get; set; }
    }

    public class Asset
    {
        public int AssetID { get; set; }
        public string AssetName { get; set; }
        public double Weight { get; set; }
        public double Price { get; set; }
        public int FormID { get; set; }
        public int ClassID { get; set; }
        public int LocationID { get; set; }
        public string ManufactureSN { get; set; }
        public int AssetStateID { get; set; }
        public int WeightUnitID { get; set; }
        public int AssetUnitID { get; set; }
        public int? GurdianShipID { get; set; }
        public int Quantity { get; set; }
        public bool IsContainer { get; set; }
        public int AssetCode { get; set; }
        public int? AssetGroupCode { get; set; }
        public double OriginalPrice { get; set; }
        public double CostPrice { get; set; }
        public double SalePrice { get; set; }
        public double ExtraUnitCost { get; set; }
        public double ProfitUnitAmount { get; set; }
        public string AssetPhoto { get; set; }
        public List<Tag> TagList { get; set; }
        public bool IsRecieved { get; set; }
        public bool IsPennding { get; set; }

        public object Location { get; set; } // or a class, if you have one
        public object Classification { get; set; }
        public object AssetGuardian { get; set; }
    }
}
