using ModernAPI.Models;
using ModernAPI.Pages;

namespace ModernAPI.Data
{
    

    public class APIFunctions 
    {
        public static APIFunctionModel GetEndPoint(EndPoints _EndPoint, string _Parameters)
        {
            switch (_EndPoint)
            {
                case EndPoints.assets:
                    return EndPoint_Assets(_Parameters);

                case EndPoints.customFValue:
                    return EndPoint_CustomFValue(_Parameters);

                case EndPoints.assetDetail:
                    return EndPoints_AssetDetail(_Parameters);

                case EndPoints.assetEdit:
                    return EndPoints_AssetEdit();

                default:
                    return null;
            }
        }

        private static APIFunctionModel EndPoints_AssetEdit()
        {
            

            APIFunctionModel model = new APIFunctionModel();
            model.BaseUrl = "https://localhost/API";
            model.Endpoint = "/api/AssetOperation/CreateOrEditAsset";
            //model.APIParameters = _Parameters;                                        // No Parameters in Edit or Add Asset. payload is AssetEditModel
            model.APIUrl = $"{model.BaseUrl}{model.Endpoint}";

            return model;
        }

        private static APIFunctionModel EndPoints_AssetDetail(string _Parameters)
        {
            //https://localhost/API/api/Asset/GetAssetDetails?AssetId=1

            APIFunctionModel model = new APIFunctionModel();
            model.BaseUrl = "https://localhost/API";
            model.Endpoint = "/api/Asset/GetAssetDetails";
            model.APIParameters = $"AssetId={_Parameters}";                                // Parameters are only Asset ID
            model.APIUrl = $"{model.BaseUrl}{model.Endpoint}?{model.APIParameters}";

            return model;
        }

        #region Custom Fields Value and Definition

        private static APIFunctionModel EndPoint_CustomFValue(string _Parameters)
        {
            APIFunctionModel model = new APIFunctionModel();
            model.BaseUrl = "https://localhost/API";
            model.Endpoint = "/api/CustomFields/GetCustomFields";
            model.APIParameters = _Parameters;
            model.APIUrl = $"{model.BaseUrl}{model.Endpoint}?{model.APIParameters}";

            return model;

        }
        #endregion


        #region Asset
        public static APIFunctionModel EndPoint_Assets(string _Parameters)
        {
            APIFunctionModel model = new APIFunctionModel();
            model.BaseUrl = "https://localhost/API";
            model.Endpoint = "/api/Asset/GetPagedAssets";
            model.APIParameters = _Parameters;
            model.APIUrl = $"{model.BaseUrl}{model.Endpoint}?{model.APIParameters}";

            return model;
        }
        #endregion

    }


    public class APIFunctionModel
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string APIParameters { get; set; } = string.Empty;
        public string APIUrl { get; set; } = string.Empty;
        
    }

    public enum EndPoints
    {
        assets,
        assetDetail,
        assetEdit,
        customFDefinition,
        customFValue
    }
        

}
