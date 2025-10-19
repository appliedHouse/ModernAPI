using ModernAPI.Data;
using System.Text;
using static ModernAPI.Data.APIData;
using static ModernAPI.Models.CustomFieldsModel;

namespace ModernAPI.Pages
{
    public partial class CustomFields
    {
        public CustomFValuePageModel MyModal { get; set; }
        public string MyMessage { get; set; } = string.Empty;

        public List<CustomFieldsDefinitionViewModel> ListCFV { get; set; }


        public async void GetAPIData()
        {
            //var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //var logger = loggerFactory.CreateLogger<APIData>();

            APIData APIService = new APIData(HttpClientFactory, TokenService);
            var result = await APIService.GetData<ApiResponse<CustomFieldsViewModel>>(EndPoints.customFValue, GetParameters());
            MyModal.ListCFV = result?.GetResults() ?? [];

            await InvokeAsync(StateHasChanged);
        }

        public async void PutAPIData()
        {

        }


        public string GetParameters()
        {

            //https://localhost/API/api/CustomFields/GetCustomFields?AssetId=1&serialNumber=123&tagId=1
            //https://localhost/API/api//CustomFields/GetCustomFields?AssetId=1


            var _Text = new StringBuilder();

            if (MyModal.AssetID > 0)
            {
                _Text.Append($"AssetId={MyModal.AssetID}");
            }

            if (MyModal.SerialNum.Length > 0)
            {
                if (_Text.Length > 0) { _Text.Append('&'); }

                _Text.Append($"serialNumber={MyModal.SerialNum}");
            }

            if (MyModal.TagID.Length > 0)
            {
                if (_Text.Length > 0) { _Text.Append('&'); }

                _Text.Append($"tagId={MyModal.TagID}");
            }

            return _Text.ToString();

        }



        public class CustomFValuePageModel
        {
            public int AssetID { get; set; }
            public string SerialNum { get; set; } = string.Empty;   
            public string TagID { get; set; } = string.Empty;
            public List<CustomFieldsViewModel> ListCFV { get; set; }

        }




    }
}
