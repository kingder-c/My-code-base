using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;

using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
using ContourRestSOE2.common;
using ContourRestSOE2;


//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace ContourRestSOE2
{
    [ComVisible(true)]
    [Guid("2fca3d0d-6e8b-405a-95e9-37a5446084ca")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",//use "MapServer" if SOE extends a Map service and "ImageServer" if it extends an Image service.
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "",
        DisplayName = "ContourRestSOE2",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class ContourRestSOE2 : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;

        //定义一个队列，用来存放IGeoDataSet对象，起到连接池的作用
        private Queue<IGeoDataset> m_DEM_GeoDataSet_Queue = new Queue<IGeoDataset>();
        private ISpatialReference mSpatialReference = null;



        public ContourRestSOE2()
        {
            soe_name = this.GetType().Name;
            logger = new ServerLogger();
            reqHandler = new SoeRestImpl(soe_name, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        public void Init(IServerObjectHelper pSOH)
        {
            serverObjectHelper = pSOH;
            mSpatialReference = Common.GetSpatialReferenceFromMapServer(serverObjectHelper);
            


        }

        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        public void Construct(IPropertySet props)
        {
            configProps = props;
        }

        #endregion

        #region IRESTRequestHandler Members

        public string GetSchema()
        {
            return reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource rootRes = new RestResource(soe_name, false, RootResHandler);

            RestOperation sampleOper = new RestOperation("sampleOperation",
                                                      new string[] { "parm1", "parm2", "parm3" },
                                                      new string[] { "json" },
                                                      SampleOperHandler);

            rootRes.operations.Add(sampleOper);

            //张久君 2017-11-13 降雨量等值线的绘制
            RestOperation RainfallContourLine = new RestOperation("RainfallContourLine",
                                                      new string[] { "rainfall" },
                                                      new string[] { "json" },
                                                      RainfallContourLineHandler);

            rootRes.operations.Add(RainfallContourLine);

            //张久君 2017-11-13 降雨量等值面的绘制
            RestOperation RainfallContourPlane = new RestOperation("RainfallContourPlane",
                                                      new string[] { "rainfall" },
                                                      new string[] { "json" },
                                                      RainfallContourPlaneHandler);

            rootRes.operations.Add(RainfallContourPlane);

            //张久君 2017-11-13 积水等值线生成
            RestOperation WaterDepthContourLine = new RestOperation("WaterDepthContourLine",
                                                      new string[] { "WaterDepth" },
                                                      new string[] { "json" },
                                                      WaterDepthContourLineHandler);

            rootRes.operations.Add(WaterDepthContourLine);

            //张久君 2017-11-21 积水等值面生成
            RestOperation WaterDepthContourPlane = new RestOperation("WaterDepthContourPlane",
                                                      new string[] { "WaterDepth" },
                                                      new string[] { "json" },
                                                      WaterDepthContourPlaneHandler);

            rootRes.operations.Add(WaterDepthContourPlane);


            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("功能描述", "实现根据降雨量监测站的空间位置和监测值生成降雨量等值线、面的生成；实现根据积水监测点空间位置及水深监测值生成积水深度等值线、面的生成；");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] SampleOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            string parm1Value;
            bool found = operationInput.TryGetString("parm1", out parm1Value);
            if (!found || string.IsNullOrEmpty(parm1Value))
                throw new ArgumentNullException("parm1");

            string parm2Value;
            found = operationInput.TryGetString("parm2", out parm2Value);
            if (!found || string.IsNullOrEmpty(parm2Value))
                throw new ArgumentNullException("parm2");

            string parm3Value;
            found = operationInput.TryGetString("parm3", out parm3Value);
            if (!found || string.IsNullOrEmpty(parm2Value))
                throw new ArgumentNullException("parm3");

            //获取本机IP地址
            string pComputerName = System.Net.Dns.GetHostName();



            JsonObject result = new JsonObject();
            result.AddString("parm1", parm1Value);
            result.AddString("parm2", parm2Value + Guid.NewGuid().ToString());
            result.AddString("parm3", parm3Value);
            result.AddString("ComputerName", pComputerName);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        ///////////////////////////////////////////////////

        //张久君 2017-11-21 实现积水量等值面的绘制
        private byte[] WaterDepthContourPlaneHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            object[] rainfallValueArray;
            bool found = operationInput.TryGetArray("WaterDepth", out rainfallValueArray);
            if (!found || rainfallValueArray == null)
                throw new ArgumentNullException("WaterDepth");


            //对Json格式上传的参数进行解析，获取监测站点的空间位置和监测值
            List<DefinePoint> pDefinePointList = new List<DefinePoint>();
            for (int i = 0; i < rainfallValueArray.Length; i++)
            {
                JsonObject point = new JsonObject();
                JsonObject pJsonObject = rainfallValueArray[i] as JsonObject;
                double? x;
                pJsonObject.TryGetAsDouble("x", out x);

                double? y;
                pJsonObject.TryGetAsDouble("y", out y);

                double? value;
                pJsonObject.TryGetAsDouble("value", out value);

                IPoint pPoint = new Point();
                pPoint.X = x.Value;
                pPoint.Y = y.Value;

                DefinePoint pDefinePoint = new DefinePoint();
                pDefinePoint.StationPoint = pPoint;
                pDefinePoint.value = value.Value;
                pDefinePointList.Add(pDefinePoint);

            }

            JsonObject pJsonObjectResult = WaterDepthContourHandlerBag.WaterDepthContourPlane(serverObjectHelper, pDefinePointList);

            return Encoding.UTF8.GetBytes(pJsonObjectResult.ToJson());
        }


        //张久君 2017-11-13 实现积水量等值线的绘制
        private byte[] WaterDepthContourLineHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            object[] rainfallValueArray;
            bool found = operationInput.TryGetArray("WaterDepth", out rainfallValueArray);
            if (!found || rainfallValueArray == null)
                throw new ArgumentNullException("WaterDepth");


            //对Json格式上传的参数进行解析，获取监测站点的空间位置和监测值
            List<DefinePoint> pDefinePointList = new List<DefinePoint>();
            for (int i = 0; i < rainfallValueArray.Length; i++)
            {
                JsonObject point = new JsonObject();
                JsonObject pJsonObject = rainfallValueArray[i] as JsonObject;
                double? x;
                pJsonObject.TryGetAsDouble("x", out x);

                double? y;
                pJsonObject.TryGetAsDouble("y", out y);

                double? value;
                pJsonObject.TryGetAsDouble("value", out value);

                IPoint pPoint = new Point();
                pPoint.X = x.Value;
                pPoint.Y = y.Value;

                DefinePoint pDefinePoint = new DefinePoint();
                pDefinePoint.StationPoint = pPoint;
                pDefinePoint.value = value.Value;
                pDefinePointList.Add(pDefinePoint);

            }

            //if (m_DEM_GeoDataSet_Queue.Count == 0)
            //{
            //    //网队列中添加30个IGeoDataSet对象
            //    for (int i = 0; i < 1; i++)
            //    {
            //        IGeoDataset pDemGeoDataSet = Common.GetGeoDataSetFromMapServer(serverObjectHelper);
            //        m_DEM_GeoDataSet_Queue.Enqueue(pDemGeoDataSet);
            //    }
            //}

            IGeoDataset pDemGeoDataSet = Common.GetGeoDataSetFromMapServer(serverObjectHelper);
            JsonObject pJsonObjectResult = WaterDepthContourHandlerBag.WaterDepthContourLine(mSpatialReference, pDemGeoDataSet, pDefinePointList);

            return Encoding.UTF8.GetBytes(pJsonObjectResult.ToJson());
        }



        //张久君 2017-11-13 实现降雨量等值线的绘制
        private byte[] RainfallContourLineHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            object[] rainfallValueArray;
            bool found = operationInput.TryGetArray("rainfall", out rainfallValueArray);
            if (!found || rainfallValueArray == null)
                throw new ArgumentNullException("rainfallValue");


            //对Json格式上传的参数进行解析，获取监测站点的空间位置和监测值
            List<DefinePoint> pDefinePointList = new List<DefinePoint>();
            for (int i = 0; i < rainfallValueArray.Length; i++)
            {
                JsonObject point = new JsonObject();
                JsonObject pJsonObject = rainfallValueArray[i] as JsonObject;
                double? x;
                pJsonObject.TryGetAsDouble("x", out x);

                double? y;
                pJsonObject.TryGetAsDouble("y", out y);

                double? value;
                pJsonObject.TryGetAsDouble("value", out value);

                IPoint pPoint = new Point();
                pPoint.X = x.Value;
                pPoint.Y = y.Value;

                DefinePoint pDefinePoint = new DefinePoint();
                pDefinePoint.StationPoint = pPoint;
                pDefinePoint.value = value.Value;
                pDefinePointList.Add(pDefinePoint);

            }

            //获取Soe所附加的MapServer所包含图层的坐标系信息。
            ISpatialReference pSpatialReference = common.Common.GetSpatialReferenceFromMapServer(serverObjectHelper);
            //IGeoDataset pGeoDatasetFromMapserver = common.Common.GetGeoDataSetFromMapServer(serverObjectHelper);
            IPolygon pMaskPolygon = Common.GetRangeFromMapServer(serverObjectHelper);
            //创建一个内存类型的FeatureClass对象，将解析的监测站空间点位信息和监测值写入创建的内存FeatureClass对象中
            IFeatureClass pMomeryFeatureClass = Common.CtrateMemoryPointFeatureClass(pSpatialReference, pDefinePointList);
            //以创建的内存类型的FeatureClass对象为参数，进行IDW插值
            IGeoDataset pGeoDataset = Common.PointFeatureClass_IDW(pMomeryFeatureClass, "DoubleValue", pSpatialReference, pMaskPolygon.Envelope);

            IGeoDataset pMaskGeoDataSet = common.Common.PolygonExtractionRaster(pMaskPolygon, pGeoDataset);

            JsonObject result = Common.ContourLine(pMaskGeoDataSet, 0.5);
            //IGeoDataset ps = Common.RasterReclassify(pGeoDataset);

            //JsonObject result = new JsonObject();

            return Encoding.UTF8.GetBytes(result.ToJson());
        }


        //张久君 2017-11-13 实现降雨量等值面的绘制
        private byte[] RainfallContourPlaneHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            object[] rainfallValueArray;
            bool found = operationInput.TryGetArray("rainfall", out rainfallValueArray);
            if (!found || rainfallValueArray == null)
                throw new ArgumentNullException("rainfallValue");


            //对Json格式上传的参数进行解析，获取监测站点的空间位置和监测值
            List<DefinePoint> pDefinePointList = new List<DefinePoint>();
            for (int i = 0; i < rainfallValueArray.Length; i++)
            {
                JsonObject point = new JsonObject();
                JsonObject pJsonObject = rainfallValueArray[i] as JsonObject;
                double? x;
                pJsonObject.TryGetAsDouble("x", out x);

                double? y;
                pJsonObject.TryGetAsDouble("y", out y);

                double? value;
                pJsonObject.TryGetAsDouble("value", out value);

                IPoint pPoint = new Point();
                pPoint.X = x.Value;
                pPoint.Y = y.Value;

                DefinePoint pDefinePoint = new DefinePoint();
                pDefinePoint.StationPoint = pPoint;
                pDefinePoint.value = value.Value;
                pDefinePointList.Add(pDefinePoint);

            }

            //获取Soe所附加的MapServer所包含图层的坐标系信息。
            ISpatialReference pSpatialReference = common.Common.GetSpatialReferenceFromMapServer(serverObjectHelper);
            //IGeoDataset pGeoDatasetFromMapserver = common.Common.GetGeoDataSetFromMapServer(serverObjectHelper);
            //创建一个内存类型的FeatureClass对象，将解析的监测站空间点位信息和监测值写入创建的内存FeatureClass对象中
            IFeatureClass pMomeryFeatureClass = Common.CtrateMemoryPointFeatureClass(pSpatialReference, pDefinePointList);

            IPolygon pMaskPolygon = Common.GetRangeFromMapServer(serverObjectHelper);

            //以创建的内存类型的FeatureClass对象为参数，进行IDW插值
            IGeoDataset pGeoDataset = Common.PointFeatureClass_IDW(pMomeryFeatureClass, "DoubleValue", pSpatialReference, pMaskPolygon.Envelope);
            //JsonObject result = Common.Contour(pGeoDataset);
            
            IGeoDataset pMaskGeoDataSet = common.Common.PolygonExtractionRaster(pMaskPolygon, pGeoDataset);

            //栅格数据重分类
            Dictionary<int, ReclassifyValue> pRasterReclassifyDictionary = null;
            IGeoDataset pRasterReclassifyResult = Common.RasterReclassify(pMaskGeoDataSet, out pRasterReclassifyDictionary);
            JsonObject result = Common.RasterToPolygonJsonObject(pRasterReclassifyResult, pSpatialReference, pRasterReclassifyDictionary);

            //JsonObject result = new JsonObject();

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        
        /////////////////
    }
}
