namespace ModernAPI.Models
{
    public class CustomFieldsModel
    {
        public class CustomFieldsViewModel
        {
            public int FieldValueID { get; set; }
            public int FieldID { get; set; }
            public int AssetID { get; set; }
            public string TitleAR { get; set; }
            public string TitleEN { get; set; }
            public string FieldValue { get; set; }
            public DateTime LastUpdateTime { get; set; }

        }

        public class CustomFieldsDefinitionViewModel
        {
            public int FieldID { get; set; }
            public int ClassID { get; set; }
            public int TypeID { get; set; }
            public string TitleAR { get; set; }
            public string TitleEN { get; set; }
            public bool? IsRequired { get; set; }
            public bool? IsActive { get; set; }
            public bool? IsDeleted { get; set; }


        }
    }
}
