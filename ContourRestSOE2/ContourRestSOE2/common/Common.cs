using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.SpatialAnalystTools;



namespace ContourRestSOE2.common
{
    /// <summary>
    /// 用来组织上个重分类的分类项
    /// </summary>
    public class ReclassifyValue
    {
        /// <summary>
        /// 栅格重分类的子类标示
        /// </summary>
        public int IndexFlag
        {
            get;
            set;
        }

        /// <summary>
        /// 栅格重分类的下限值
        /// </summary>
        public double MinValue
        {
            get;
            set;
        }

        /// <summary>
        /// 栅格重分类的上线值
        /// </summary>
        public double MaxValue
        {
            get;
            set;
        }

        /// <summary>
        /// 中间值
        /// </summary>
        public double Median
        {
            get
            {
                return this.MinValue + (this.MaxValue - this.MinValue) / 2;
            }

        }

    }


    /// <summary>
    /// <see cref="IUserManager"/>接口定义了<see cref="User"/>实体类的增、删、改、
    /// （CRUD）方法，该接口的实现类将负责用户对象的简单管理。
    /// <remarks>增加使用例子
    /// <see cref="IUserManager"/>接口仅定义<see cref="User"/>实体对象的CRUD操作
    /// 并没有定义关联用户对象与群组、组织机构等的操作，这些业务逻辑操作的方法定义
    /// 专门的关联操作接口<see cref="IUserGroupAssociator"/>及<see cref="IUserOrgAssociator"/>中。
    /// </remarks>
    //Title: IUserManager
    //Copyright:  ***** Software LTD.co Copyright (c) 2006
    //Company: 正元智慧城市建设公司智慧市政事业部
    //Designer: ***
    //Coder: ***
    //Reviewer: 
    //Tester: ***
    //Version: 1.0
    //History:
    //2017-11-09  张久君 [创建]
    //2016-11-xx  程序员姓名 [编码]
    //2016-11-xx  程序员姓名 [修改] xxxxx
    /// </summary>
    public class Common
    {


        /// <summary>
        /// 实现由Ring生成Polygon。
        /// </summary>
        /// <param name="pRing"></param>
        /// <returns></returns>
        private static IPolygon CreatePolygonFromRing(IGeometry pRing)
        {
            ISegmentCollection segCol_out = pRing as ISegmentCollection;
            IPolyline pPolyline_out = new PolylineClass();
            ISegmentCollection newSegCol_out = pPolyline_out as ISegmentCollection;
            newSegCol_out.AddSegmentCollection(segCol_out);
            pPolyline_out.SpatialReference = pRing.SpatialReference;
            IPolyline pOutLineResult = DefinePolylineSmooth(pPolyline_out);

            //使用外环生成面
            IGeometryCollection geometryCollection = pOutLineResult as IGeometryCollection;
            ISegmentCollection segmentCollection;
            IGeometryCollection polygon = new PolygonClass();
            object o = Type.Missing;
            for (int i = 0; i < geometryCollection.GeometryCount; i++)
            {
                segmentCollection = new RingClass();
                segmentCollection.AddSegmentCollection(geometryCollection.get_Geometry(i) as ISegmentCollection);
                IRing ring = segmentCollection as IRing;
                polygon.AddGeometry(ring as IGeometry, ref o, ref o);
            }
            IPolygon polygonSpation = polygon as IPolygon;
            polygonSpation.SpatialReference = pRing.SpatialReference;
            return polygonSpation;
        }


        /// <summary>
        /// 实现polygon的平滑处理，解决曲线不能转换成json的问题
        /// </summary>
        /// <param name="polygonParameter"></param>
        /// <returns></returns>
        public static IPolygon DefinePolygonSmooth(IPolygon4 polygonParameter)
        {
            //polygonParameter.Smooth(2);
            //ITopologicalOperator pTopologicalOperator01 = polygonParameter as ITopologicalOperator;
            //IGeometry pPolygonBound = pTopologicalOperator01.Boundary;
            //IPolygon pOutPolygon01 = CreatePolygonFromRing(pPolygonBound);
            //return pOutPolygon01;

            IGeometryBag pOutGeometryBag = polygonParameter.ExteriorRingBag;//获取外部环
            IGeometryCollection pOutGmtyCollection = pOutGeometryBag as IGeometryCollection;
            IGeometry pOutRing = pOutGmtyCollection.get_Geometry(0);//外部环

            IPolygon pOutPolygon = CreatePolygonFromRing(pOutRing);

            //获取内部环
            IGeometryBag pInteriotGeometryBag = polygonParameter.get_InteriorRingBag(pOutRing as IRing);//获取内部环
            IGeometryCollection pInteriorGeometryCollection = pInteriotGeometryBag as IGeometryCollection;
            if (pInteriorGeometryCollection.GeometryCount == 0)
            {
                return pOutPolygon;
            }

            ITopologicalOperator pTopologicalOperator = pOutPolygon as ITopologicalOperator;

            IGeometry pGeometry = null;
            
            for (int j = 0; j < pInteriorGeometryCollection.GeometryCount; j++)
            {
                IGeometry pInteriorGeometry = pInteriorGeometryCollection.get_Geometry(j);
                IPolygon pInnerPolygon = CreatePolygonFromRing(pInteriorGeometry);
                pGeometry = pTopologicalOperator.Difference(pInnerPolygon);
                pTopologicalOperator = pGeometry as ITopologicalOperator;
            }

            pOutPolygon = pGeometry as IPolygon;
            pOutPolygon.SpatialReference = polygonParameter.SpatialReference;
            return pOutPolygon;

        }


        /// <summary>
        /// 2018.02.26 张久君 添加。实现对polyline的平滑处理。（解决转化结果不能转换为json的问题）
        /// </summary>
        /// <param name="polylineParameter"></param>
        /// <returns></returns>
        public static IPolyline DefinePolylineSmooth2(IPolyline polylineParameter)
        {
            //polylineParameter.Smooth(4);//对线进行平滑处理。采用该方法进行平滑处理后，在对平滑化之后的结果转化为Json时会报错，提示不能将曲线转换为json。
            //对经过平滑处理的曲线进行截取，获取点集

            IGeometryCollection pGeometryCollection = new PolylineClass();
            IPointCollection pPointCollection = new PathClass();

            //将平滑处理后的等值线进行按照距离获取点，再用获取的点连成相对平滑的线，避开曲线不能转换成json的问题。
            int pLineLength = (int)polylineParameter.Length;
            for (int i = 0; i < pLineLength; i++)
            {
                IPoint pPoint = new PointClass();
                polylineParameter.QueryPoint(esriSegmentExtension.esriExtendAtFrom, i, false, pPoint);
                pPointCollection.AddPoint(pPoint);
            }
            pPointCollection.AddPoint(polylineParameter.ToPoint);

            object pMissing = Type.Missing;
            pGeometryCollection.AddGeometry(pPointCollection as IGeometry, ref pMissing, ref pMissing);
            return pGeometryCollection as IPolyline;
        }

        /// <summary>
        /// 2018.02.26 张久君 添加。实现对polyline的平滑处理。（解决转化结果不能转换为json的问题）
        /// </summary>
        /// <param name="polylineParameter"></param>
        /// <returns></returns>
        public static IPolyline DefinePolylineSmooth(IPolyline polylineParameter)
        {
            polylineParameter.Smooth(1);//对线进行平滑处理。采用该方法进行平滑处理后，在对平滑化之后的结果转化为Json时会报错，提示不能将曲线转换为json。
            //对经过平滑处理的曲线进行截取，获取点集

            IGeometryCollection pGeometryCollection = new PolylineClass();
            IPointCollection pPointCollection = new PathClass();

            //将平滑处理后的等值线进行按照距离获取点，再用获取的点连成相对平滑的线，避开曲线不能转换成json的问题。
            int pLineLength = (int)polylineParameter.Length;
            for (int i = 0; i < pLineLength; i++)
            {
                IPoint pPoint = new PointClass();
                polylineParameter.QueryPoint(esriSegmentExtension.esriExtendAtFrom, i, false, pPoint);
                pPointCollection.AddPoint(pPoint);
            }
            pPointCollection.AddPoint(polylineParameter.ToPoint);

            object pMissing = Type.Missing;
            pGeometryCollection.AddGeometry(pPointCollection as IGeometry, ref pMissing, ref pMissing);
            return pGeometryCollection as IPolyline;
        }


        public static void StorePolygon(IPolygon pPolygon)
        {
            IWorkspaceFactory pWSF = new ShapefileWorkspaceFactory();
            IWorkspace pWS = pWSF.OpenFromFile(@"D:\code\1市政课题\测试数据\shape", 0);
            IFeatureWorkspace pFWS = pWS as IFeatureWorkspace;
            IFeatureClass pFeatureClass = pFWS.OpenFeatureClass("New_Shapefile");
            IFeature pFeature = pFeatureClass.CreateFeature();
            pFeature.Shape = pPolygon;
            pFeature.Store();

        }

        /// <summary>
        /// 张久君 2017-11-20 (解决polygon中包含岛的问题)实现用Polygon截取raster数据功能，并将截取结果以IGeoDataset方式返回
        /// </summary>
        /// <param name="pPolygon"></param>
        /// <param name="pDEM_GeoDataSet"></param>
        /// <returns></returns>
        public static IGeoDataset PolygonExtractionRaster2(IPolygon4 pMaskPolygon, IGeoDataset pDEM_GeoDataSet)
        {
            IExtractionOp2 pExtractionOp = new RasterExtractionOpClass();
            IGeoDataset pResultGeoDataSet = null;
            try
            {
                IGeometryBag pOutGeometryBag = pMaskPolygon.ExteriorRingBag;//获取外部环
                IGeometryCollection pOutGmtyCollection = pOutGeometryBag as IGeometryCollection;
                if(pOutGmtyCollection.GeometryCount == 1)
                {
                    //List<IPolygon> pInteriorGeometryList = new List<IPolygon>();//存放内部环
                    IGeometry pOutRing = pOutGmtyCollection.get_Geometry(0);//外部环
                    IGeometryBag pInteriotGeometryBag = pMaskPolygon.get_InteriorRingBag(pOutRing as IRing);//获取内部环
                    IGeometryCollection pInteriorGeometryCollection = pInteriotGeometryBag as IGeometryCollection;
                    //若Polygon内部不含岛，则直接进行raster剪切
                    if (pInteriorGeometryCollection.GeometryCount == 0)
                    {
                        IPolygon pPolygon = pMaskPolygon as IPolygon;
                        pResultGeoDataSet = pExtractionOp.Polygon(pDEM_GeoDataSet, pPolygon,true);
                        return pResultGeoDataSet;
                    }

                    //若Polygon内部含岛，则先用输入的Polygon的外环生成一个不含到的polygon，用该Polygon进行raster的剪切，选择polygon内部的raster。
                    //根据外环生成内部无岛的Polygon

                    ISegmentCollection segCol_out = pOutRing as ISegmentCollection;
                    IPolygon pPolygon_out = new PolygonClass();
                    ISegmentCollection newSegCol_out = pPolygon_out as ISegmentCollection;
                    newSegCol_out.AddSegmentCollection(segCol_out);
                    pPolygon_out.SpatialReference = pMaskPolygon.SpatialReference;
                    pResultGeoDataSet = pExtractionOp.Polygon(pDEM_GeoDataSet,pPolygon_out, true);


                    IPolygon pPolygon_InterriorAll = new PolygonClass();
                    ITopologicalOperator pTopologicalOperator = pPolygon_InterriorAll as ITopologicalOperator;

                    //循环用外环生成的polygon数据剪切的raster数据，用内环进行循环截取，去内环外部的raster。
                    for(int j=0;j<pInteriorGeometryCollection.GeometryCount;j++)
                    {
                        IGeometry pInteriorGeometry = pInteriorGeometryCollection.get_Geometry(j);
                        IRing pRing = pInteriorGeometry as IRing;
                        ISegmentCollection segCol = pRing as ISegmentCollection;
                        IPolygon pPolygon_Interrior = new PolygonClass();
                        ISegmentCollection newSegCol = pPolygon_Interrior as ISegmentCollection;
                        newSegCol.AddSegmentCollection(segCol);
                        pPolygon_Interrior.SpatialReference = pMaskPolygon.SpatialReference;
                        pResultGeoDataSet = pExtractionOp.Polygon(pResultGeoDataSet, pPolygon_Interrior, false);
                    }
                    return pResultGeoDataSet;
                    
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// 将polygon存储至内存类型的featureclass对象中
        /// </summary>
        /// <param name="pPolygon"></param>
        /// <returns></returns>
        public static IFeatureClass StorePolygonToImmerFeatureClass(IPolygon pPolygon)
        {
            try
            {
                IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactory();
                string WorkspaceName = Guid.NewGuid().ToString().Replace("-", "");
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", WorkspaceName, null, 0);
                ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
                ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();

                IFields oFields = new FieldsClass();
                IFieldsEdit oFieldsEdit = oFields as IFieldsEdit; ;


                IField oField = new FieldClass();
                IFieldEdit oFieldEdit = oField as IFieldEdit;
                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.AvgNumPoints_2 = 5;
                geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
                geometryDefEdit.GridCount_2 = 1;
                geometryDefEdit.HasM_2 = false;
                geometryDefEdit.HasZ_2 = false;
                geometryDefEdit.SpatialReference_2 = pPolygon.SpatialReference;
                oFieldEdit.Name_2 = "SHAPE";
                oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                oFieldEdit.GeometryDef_2 = geometryDef;
                oFieldEdit.IsNullable_2 = true;
                oFieldEdit.Required_2 = true;
                oFieldsEdit.AddField(oField);

                //添加存储降雨量监测值的字段
                IField pFieldValue = new FieldClass();
                IFieldEdit pFieldEditValue = pFieldValue as IFieldEdit;
                pFieldEditValue.Type_2 = esriFieldType.esriFieldTypeDouble;
                pFieldEditValue.Name_2 = "DoubleValue";
                pFieldEditValue.IsNullable_2 = true;
                oFieldsEdit.AddField(pFieldValue);

                string pFeatureClassName = Guid.NewGuid().ToString().Replace("-", "");
                IFeatureClass oFeatureClass = (inmemWor as IFeatureWorkspace).CreateFeatureClass(pFeatureClassName, oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

                //获取降雨量监测值字段的序号
                //IFields pFields2 = oFeatureClass.Fields;
                //int pRainfallFieldIndex = pFields2.FindField("DoubleValue");

                IFeature pFeature = oFeatureClass.CreateFeature();
                IGeometry pGeometry = pPolygon as IGeometry;
                pFeature.Shape = pGeometry;
                pFeature.Store();
                return oFeatureClass;
            }
            catch (Exception ex)
            {
                return null;
            }

        }



        /// <summary>
        /// 2017-12-27 实现矢量数据转栅格
        /// </summary>
        /// <param name="feaureClass"></param>
        /// <param name="string_RasterWorkspace"></param>
        /// <param name="int32_NumberOfCells"></param>
        /// <returns></returns>
        public static ESRI.ArcGIS.Geodatabase.IGeoDataset PolygonToRaster2(IPolygon pPolygon)
        {
            try
            {
                IFeatureClass pFeatureClass = StorePolygonToImmerFeatureClass(pPolygon);

                ESRI.ArcGIS.Geodatabase.IGeoDataset geoDataset = (ESRI.ArcGIS.Geodatabase.IGeoDataset)pFeatureClass; // Explicit Cast

                ESRI.ArcGIS.Geometry.ISpatialReference spatialReference = geoDataset.SpatialReference;

                // Create a RasterMaker operator
                ESRI.ArcGIS.GeoAnalyst.IConversionOp conversionOp = new ESRI.ArcGIS.GeoAnalyst.RasterConversionOpClass();

                string pTempWorkspacePath = string.Empty;
                //获取计算机盘符
                System.IO.DriveInfo[] allDirves = System.IO.DriveInfo.GetDrives();
                for (int i = 0; i < allDirves.Length; i++)
                {
                    if (allDirves[i].IsReady == true)
                    {
                        pTempWorkspacePath = allDirves[i].Name + @"RasterTempWorkspace";
                        break;
                    }
                }

                if (System.IO.Directory.Exists(pTempWorkspacePath) == false)
                {
                    System.IO.Directory.CreateDirectory(pTempWorkspacePath);
                }
                else
                {
                    string[] DirectoryArray = System.IO.Directory.GetDirectories(pTempWorkspacePath);
                    for (int i = 0; i < DirectoryArray.Length; i++)
                    {
                        try
                        {
                            System.IO.DirectoryInfo pDirectoryInfo = new System.IO.DirectoryInfo(DirectoryArray[i]);
                            DateTime pCreateTime = pDirectoryInfo.CreationTime;
                            if (pCreateTime < DateTime.Now.AddMinutes(-10))
                            {
                                pDirectoryInfo.Delete(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                }

                //创建一个内存工作空间
                ESRI.ArcGIS.Geodatabase.IWorkspaceFactory workspaceFactoryRaster = new ESRI.ArcGIS.DataSourcesRaster.RasterWorkspaceFactoryClass();
                string WorkspaceNameRaster = Guid.NewGuid().ToString().Replace("-", "");
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceNameRaster = workspaceFactoryRaster.Create(pTempWorkspacePath, WorkspaceNameRaster, null, 0);
                ESRI.ArcGIS.esriSystem.IName nameRaster = (IName)workspaceNameRaster;
                ESRI.ArcGIS.Geodatabase.IWorkspace inmemWorRaster = (IWorkspace)nameRaster.Open();


                // Create analysis environment
                ESRI.ArcGIS.GeoAnalyst.IRasterAnalysisEnvironment rasterAnalysisEnvironment = (ESRI.ArcGIS.GeoAnalyst.IRasterAnalysisEnvironment)conversionOp; // Explicit Cast
                //rasterAnalysisEnvironment.OutWorkspace = inmemWorRaster;

                ESRI.ArcGIS.Geometry.IEnvelope envelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
                envelope = geoDataset.Extent;

                // Set cell size
                System.Double double_xMin = envelope.XMin;
                System.Double double_xMax = envelope.XMax;
                System.Double double_difference = double_xMax - double_xMin;
                System.Double double_cellSize = 2;//double_difference / int32_NumberOfCells;
                object object_cellSize = (System.Object)double_cellSize;
                rasterAnalysisEnvironment.SetCellSize(ESRI.ArcGIS.GeoAnalyst.esriRasterEnvSettingEnum.esriRasterEnvValue, ref object_cellSize);

                // Set output extent
                object object_Envelope = (System.Object)envelope; // Explict Cast
                object object_Missing = System.Type.Missing;
                rasterAnalysisEnvironment.SetExtent(ESRI.ArcGIS.GeoAnalyst.esriRasterEnvSettingEnum.esriRasterEnvValue, ref object_Envelope, ref object_Missing);

                // Set output spatial reference
                rasterAnalysisEnvironment.OutSpatialReference = spatialReference;

                // Perform spatial operation
                ESRI.ArcGIS.Geodatabase.IRasterDataset rasterDataset = new ESRI.ArcGIS.DataSourcesRaster.RasterDatasetClass();

                // Create the new raster name that meets the coverage naming convention
                System.String string_RasterName = pFeatureClass.AliasName;
                if (string_RasterName.Length > 8)
                {
                    string_RasterName = string_RasterName.Substring(0, 8);
                }
                string_RasterName = string_RasterName.Replace(" ", "_");
                rasterDataset = conversionOp.ToRasterDataset(geoDataset, "TIFF", inmemWorRaster, string_RasterName + @".tif");
                ESRI.ArcGIS.Geodatabase.IGeoDataset geoDataset_output = (ESRI.ArcGIS.Geodatabase.IGeoDataset)rasterDataset;
                return geoDataset_output;
            }
            catch (Exception ex)
            {
                return null;
            }
            

        }




        /// <summary>
        /// 张久君 2017-11-16 实现用Polygon截取raster数据功能，并将截取结果以IGeoDataset方式返回.(该方法支持含有岛的polygon进行切割)
        /// </summary>
        /// <param name="pPolygon"></param>
        /// <param name="pDEM_GeoDataSet"></param>
        /// <returns></returns>
        public static IGeoDataset PolygonExtractionRaster(IPolygon pPolygon, IGeoDataset pDEM_GeoDataSet)
        {
            
            try
            {
                //IExtractionOp2 pExtractionOp = new RasterExtractionOpClass();
                //IGeoDataset pGeoDataSet_ExtractionResult = pExtractionOp.Polygon(pDEM_GeoDataSet,pPolygon,true);
                //return pGeoDataSet_ExtractionResult;
                string TempRasterPath = string.Empty;
                IGeoDataset pPolygonRaster = PolygonToRaster2(pPolygon);
                IExtractionOp2 pExtractionOp = new RasterExtractionOpClass();
                IGeoDataset pGeoDataSet = pExtractionOp.Raster(pDEM_GeoDataSet, pPolygonRaster);
                return pGeoDataSet;
            }
            catch (Exception ex)
            {
                return null;
            }
            

        }

        /// <summary>
        /// 张久君 2017-11-16 实现通过点选择polygon要素的功能
        /// </summary>
        /// <param name="pPoint"></param>
        /// <param name="pFeatureClass"></param>
        /// <returns></returns>
        public static IPolygon PointSelectPolygon(IPoint pPoint,IFeatureClass pFeatureClass)
        {
            try
            {
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = pPoint as IGeometry;
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                IFeatureCursor pFeatureCursor = pFeatureClass.Search(pSpatialFilter, false);
                IFeature pFeature = pFeatureCursor.NextFeature();
                if (pFeature != null)
                {
                    IPolygon pPolygon = pFeature.Shape as IPolygon;
                    return pPolygon;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 张久君 2017-11-16 实现Raster数据转Polygon，并以包含Polygon要素的IFeatureClass类型的对象作为返回格式
        /// </summary>
        /// <param name="pRasterGeodataset">raster数据集</param>
        /// <param name="pSpatialReference">输出结果的空间坐标系</param>
        /// <returns></returns>
        public static IFeatureClass RasterToPolygonFeatureClass(IGeoDataset pRasterGeodataset,ISpatialReference pSpatialReference)
        {
            try
            {
                //创建一个内存工作空间
                IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactory();
                string WorkspaceName = Guid.NewGuid().ToString().Replace("-", "");
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", WorkspaceName, null, 0);
                ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
                ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();

                IConversionOp pConversionOp = new RasterConversionOpClass();
                
                //create Raster Analysis Enviroment
                IRasterAnalysisEnvironment env = (IRasterAnalysisEnvironment)pConversionOp;
                env.OutSpatialReference = pSpatialReference;
                
                //object o = (System.Object)0.1;
                //env.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue,ref o);
                string pFeatureClassName = Guid.NewGuid().ToString().Replace("-","");
                
                IGeoDataset pGeoDatasetResult = pConversionOp.RasterDataToPolygonFeatureData(pRasterGeodataset, inmemWor, pFeatureClassName,true);
                IFeatureClass pFeatureClassResult = pGeoDatasetResult as IFeatureClass;
                return pFeatureClassResult;
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// 张久君 2017-11-16 实现对ranster数据的筛选，将ranster的值小于输入参数“pWaterHeight”的cells筛选出来并生成一个新的ranster数据。
        /// </summary>
        /// <param name="pGeoDataSet"></param>
        /// <param name="pWaterHeight"></param>
        /// <returns></returns>
        public static IGeoDataset RasterCalculator(IGeoDataset pGeoDataSet, string pExecuteString)
        {
            if (pGeoDataSet == null)
            {
                return null;
            }
            try
            {
                IMapAlgebraOp pMapAlgebraOp = new RasterMapAlgebraOpClass();
                pMapAlgebraOp.BindRaster(pGeoDataSet,"1");
                IGeoDataset pResultGeoDataset = pMapAlgebraOp.Execute(pExecuteString);
                return pResultGeoDataset;
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// 张久君 2018-03-15 创建
        /// 从SOE附加的MapServer地图服务中获取Raster图层
        /// </summary>
        /// <param name="serverObjectHelper"></param>
        /// <returns></returns>
        public static IPolygon GetRangeFromMapServer(IServerObjectHelper serverObjectHelper)
        {
            try
            {
                //张久君 2017.11.07 获取soe所附加的MapServer的坐标信息
                IMapServer3 mapServer = (IMapServer3)serverObjectHelper.ServerObject;
                string mapName = mapServer.DefaultMapName;
                IMapLayerInfo layerInfor;
                IMapServerInfo mapServerInfor = mapServer.GetServerInfo(mapName);
                IMapLayerInfos layerInfos = mapServerInfor.MapLayerInfos;
                int c = layerInfos.Count;
                IMapServerDataAccess dataAccess = (IMapServerDataAccess)mapServer;
                for (int i = 0; i < c; i++)
                {
                    layerInfor = layerInfos.get_Element(i);
                    if (layerInfor.IsFeatureLayer == true)
                    {
                        IFeatureClass pFeatureclass = (IFeatureClass)dataAccess.GetDataSource(mapName, i);
                        if (pFeatureclass.ShapeType == esriGeometryType.esriGeometryPolygon)
                        {
                            IFeatureCursor pFeatureCursor = pFeatureclass.Search(null,false);
                            IPolygon pPolygon = pFeatureCursor.NextFeature().Shape as IPolygon;
                            return pPolygon;
                        }
                    }
                }
                return null;
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 张久君 2017-11-16 创建
        /// 从SOE附加的MapServer地图服务中获取Raster图层
        /// </summary>
        /// <param name="serverObjectHelper"></param>
        /// <returns></returns>
        public static IGeoDataset GetGeoDataSetFromMapServer(IServerObjectHelper serverObjectHelper)
        {
            try
            {
                //张久君 2017.11.07 获取soe所附加的MapServer的坐标信息
                IMapServer3 mapServer = (IMapServer3)serverObjectHelper.ServerObject;
                string mapName = mapServer.DefaultMapName;
                IMapLayerInfo layerInfor;
                IMapServerInfo mapServerInfor = mapServer.GetServerInfo(mapName);
                IMapLayerInfos layerInfos = mapServerInfor.MapLayerInfos;
                int c = layerInfos.Count;
                int layerIndex = -1;
                for (int i = 0; i < c; i++)
                {
                    layerInfor = layerInfos.get_Element(i);
                    if (layerInfor.IsFeatureLayer == false)
                    {
                        layerIndex = i;
                        break;
                    }
                }
                IMapServerDataAccess dataAccess = (IMapServerDataAccess)mapServer;
                if (layerIndex != -1)
                {
                    IRaster pRaster = (IRaster)dataAccess.GetDataSource(mapName, layerIndex);
                    IGeoDataset pGeoDataset = (IGeoDataset)pRaster;
                    return pGeoDataset;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 张久君 2017-11-21 指定水面高程值，计算积水的积水量。（单位：立方米）
        /// </summary>
        /// <param name="pDEM_GeoDataset">DEM数据</param>
        /// <param name="PlaneHeight">积水水面高程</param>
        /// <returns></returns>
        public static double ComputeDEM_Volume(IGeoDataset pDEM_GeoDataset,double PlaneHeight)
        {
            try
            {
                IRaster pRaster = pDEM_GeoDataset as IRaster;
                ISpatialReference pSpatialReference = pDEM_GeoDataset.SpatialReference;
                IRasterSurface pRasterSurface = new RasterSurfaceClass();
                pRasterSurface.PutRaster(pRaster, 0);
                ISurface surface = pRasterSurface as ISurface;
                double pVolume = surface.GetVolume(PlaneHeight, esriPlaneReferenceType.esriPlaneReferenceBelow);
                return pVolume;
            }
            catch (Exception ex)
            {
                return double.MinValue;
            }
        }



        /// <summary>
        /// 张久君 2017-11-16 创建。
        /// 根据给定的坐标获取该坐标对应点的高程
        /// </summary>
        /// <param name="x">x坐标值</param>
        /// <param name="y">y坐标值</param>
        /// <param name="pDEM_GeoDataset">DEM数据集</param>
        /// <returns>返回值，指定点的DEM高程值</returns>
        public static double GetHeight(IPoint pPoint,IGeoDataset pDEM_GeoDataset)
        {
            try
            {
                IRaster pRaster = pDEM_GeoDataset as IRaster;
                ISpatialReference pSpatialReference = pDEM_GeoDataset.SpatialReference;
                IRasterSurface pRasterSurface = new RasterSurfaceClass();
                pRasterSurface.PutRaster(pRaster,0);
                ISurface surface = pRasterSurface as ISurface;
                double pHeight;
                if (pPoint.SpatialReference == null)
                {
                    pPoint.Project(pSpatialReference);
                    pHeight = surface.GetElevation(pPoint);
                }
                else
                {
                    pHeight = surface.GetElevation(pPoint);
                }
                pHeight = Math.Round(pHeight,2);
                return pHeight;

            }
            catch (Exception ex)
            {
                return -9999999;
            }
        }


        /// <summary>
        /// 获取MapServer中的FeatureLayer图层的坐标系信息，若不存在FeatureLayer图层，则获取栅格图层的坐标系信息
        /// </summary>
        /// <param name="serverObjectHelper"></param>
        /// <returns></returns>
        public static ISpatialReference GetSpatialReferenceFromMapServer(IServerObjectHelper serverObjectHelper)
        {
            //张久君 2017.11.07 获取soe所附加的MapServer的坐标信息
            IMapServer3 mapServer = (IMapServer3)serverObjectHelper.ServerObject;
            string mapName = mapServer.DefaultMapName;
            IMapLayerInfo layerInfor;
            IMapServerInfo mapServerInfor = mapServer.GetServerInfo(mapName);
            IMapLayerInfos layerInfos = mapServerInfor.MapLayerInfos;
            int c = layerInfos.Count;
            int layerIndex = -1;
            for (int i = 0; i < c; i++)
            {
                layerInfor = layerInfos.get_Element(i);
                if (layerInfor.IsFeatureLayer == true)
                {
                    layerIndex = i;
                    break;
                }
            }
            IMapServerDataAccess dataAccess = (IMapServerDataAccess)mapServer;
            ISpatialReference pSpatialReference = null;
            if (layerIndex == -1)
            {
                layerIndex = 0;
                IRaster pRaster = (IRaster)dataAccess.GetDataSource(mapName, layerIndex);
                IGeoDataset pGeoDataset = (IGeoDataset)pRaster;
                pSpatialReference = pGeoDataset.SpatialReference;
            }
            else
            {
                IFeatureClass pFeatCls = (IFeatureClass)dataAccess.GetDataSource(mapName, layerIndex);
                IDataset pDS = pFeatCls as IDataset;
                IGeoDataset pGeoDataset = (IGeoDataset)pFeatCls;
                pSpatialReference = pGeoDataset.SpatialReference;
            }
            return pSpatialReference;
        }

        /// <summary>
        /// 根据给定的坐标系创建一个存储在内存工作空间中的FeatureClass对象
        /// </summary>
        /// <param name="pSpatialReference">坐标系信息</param>
        /// <returns></returns>
        public static IFeatureClass CtrateMemoryPointFeatureClass(ISpatialReference pSpatialReference, List<DefinePoint> pDefinePointList)
        {
            try
            {
                //ESRI.ArcGIS.GeoAnalyst.IInterpolationOp3 pInterpolation = new ESRI.ArcGIS.GeoAnalyst.RasterInterpolationOp() as ESRI.ArcGIS.GeoAnalyst.IInterpolationOp3;

                IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactory();
                string WorkspaceName = Guid.NewGuid().ToString().Replace("-", "");
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", WorkspaceName, null, 0);
                ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
                ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();

                IFields oFields = new FieldsClass();
                IFieldsEdit oFieldsEdit = oFields as IFieldsEdit; ;


                IField oField = new FieldClass();
                IFieldEdit oFieldEdit = oField as IFieldEdit;
                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.AvgNumPoints_2 = 5;
                geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
                geometryDefEdit.GridCount_2 = 1;
                geometryDefEdit.HasM_2 = false;
                geometryDefEdit.HasZ_2 = false;
                geometryDefEdit.SpatialReference_2 = pSpatialReference;
                oFieldEdit.Name_2 = "SHAPE";
                oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                oFieldEdit.GeometryDef_2 = geometryDef;
                oFieldEdit.IsNullable_2 = true;
                oFieldEdit.Required_2 = true;
                oFieldsEdit.AddField(oField);

                //添加存储降雨量监测值的字段
                IField pFieldValue = new FieldClass();
                IFieldEdit pFieldEditValue = pFieldValue as IFieldEdit;
                pFieldEditValue.Type_2 = esriFieldType.esriFieldTypeDouble;
                pFieldEditValue.Name_2 = "DoubleValue";
                pFieldEditValue.IsNullable_2 = true;
                oFieldsEdit.AddField(pFieldValue);

                string pFeatureClassName = Guid.NewGuid().ToString().Replace("-", "");
                IFeatureClass oFeatureClass = (inmemWor as IFeatureWorkspace).CreateFeatureClass(pFeatureClassName, oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

                //获取降雨量监测值字段的序号
                IFields pFields2 = oFeatureClass.Fields;
                int pRainfallFieldIndex = pFields2.FindField("DoubleValue");

                for (int i = 0; i < pDefinePointList.Count; i++)
                {
                    IFeature pFeature = oFeatureClass.CreateFeature();
                    IGeometry pGeometry = pDefinePointList[i].StationPoint as IGeometry;
                    pFeature.Shape = pGeometry;
                    pFeature.Value[pRainfallFieldIndex] = pDefinePointList[i].value;
                    pFeature.Store();
                }

                //以下为测试代码，测试创建IFeatureClass是否成功

                //IFeatureCursor pFeatureCursor = oFeatureClass.Search(null, false);
                //IFeature pFeat2 = pFeatureCursor.NextFeature();
                //StringBuilder pStringBuilder = new StringBuilder();
                //while (pFeat2 != null)
                //{
                //    IPoint pp = pFeat2.Shape as IPoint;
                //    pStringBuilder.AppendLine("ID:" + pFeat2.OID.ToString() + "，X:" + pp.X.ToString() + ",Y:" + pp.Y.ToString() + ",V:" + pFeat2.Value[1].ToString() + "\n");
                //    pFeat2 = pFeatureCursor.NextFeature();
                //}

                return oFeatureClass;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 张久君 2017.11.06 将Point类型的FeatureClass进行IDW插值，并返回插值结果
        /// </summary>
        /// <param name="pointFeatureClass">point类型的FeatureClass对象</param>
        /// <param name="FieldName">参与插值的字段名称</param>
        /// <returns></returns>
        public static IGeoDataset PointFeatureClass_IDW(IFeatureClass pointFeatureClass, string FieldName, ISpatialReference pSpatialReference,IEnvelope OutPutEnvelop)
        {
            try
            {
                IFeatureClassDescriptor pFCDescriptor = new FeatureClassDescriptorClass();
                pFCDescriptor.Create(pointFeatureClass, null, FieldName);
                IRasterRadius pRadius = new RasterRadiusClass();//设置搜索半径

                object objectMaxDistance = null;
                object objectbarrier = null;
                object missing = Type.Missing;

                pRadius.SetVariable(12, ref missing);//这里设置不同的值，出图效果会发生变化

                //执行插值，生成结果（栅格数据集）
                ESRI.ArcGIS.GeoAnalyst.IInterpolationOp3 pInterpolation = new ESRI.ArcGIS.GeoAnalyst.RasterInterpolationOp() as ESRI.ArcGIS.GeoAnalyst.IInterpolationOp3;
                object dCellSize = 2;   //设置栅格精度（输出栅格像元大小）
                object snapRasterData = Type.Missing;
                IRasterAnalysisEnvironment pEnv = pInterpolation as IRasterAnalysisEnvironment;
                pEnv.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref dCellSize);
                pEnv.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, OutPutEnvelop, Type.Missing);
                pEnv.OutSpatialReference = pSpatialReference;


                IGeoDataset pOutGeoDataset = pInterpolation.IDW((IGeoDataset)pFCDescriptor, 2, pRadius, ref missing);//设置默认权重值为2

                //控制周围点对于内插值的重要性的距离指数。幂值越高，远数据点的影响越小
                //IRaster pOutRaster = pOutGeoDataset as IRaster;
                return pOutGeoDataset;
            }
            catch (Exception ex)
            {
                return null;
            }
            

        }



        /// <summary>
        /// 张久君 2017-11-06 等值线的提取
        /// </summary>
        /// <param name="pGeoDataSet">进行等值线提取的raster数据</param>
        /// <param name="pInterval">等值线的间距</param>
        /// <returns></returns>
        public static JsonObject ContourLine(IGeoDataset pGeoDataSet,double pInterval)
        {
            try
            {
                //IGeoDataset pGeoDataSet = pInputRaster as IGeoDataset;
                //创建一个内存工作空间
                IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactory();
                string WorkspaceName = Guid.NewGuid().ToString().Replace("-", "");
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", WorkspaceName, null, 0);
                ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
                ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();

                object Missing = Type.Missing;
                ISurfaceOp2 pSurfaceOp2 = new RasterSurfaceOpClass();
                IRasterAnalysisEnvironment pRasterAnalysisEnvironment = pSurfaceOp2 as IRasterAnalysisEnvironment;
                pRasterAnalysisEnvironment.Reset();
                object cellSizeProvider = 6;
                pRasterAnalysisEnvironment.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref cellSizeProvider);
                //pRasterAnalysisEnvironment.OutWorkspace = inmemWor;


                IGeoDataset pOutputDataSet = pSurfaceOp2.Contour(pGeoDataSet,pInterval, ref Missing, ref Missing);
                IFeatureClass pFeatureClass = pOutputDataSet as IFeatureClass;
                int pCountourValueFieldIndex = pFeatureClass.Fields.FindField("Contour");
                IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
                List<JsonObject> pJsonObjectList = new List<JsonObject>();
                IFeature pFeature = pFeatureCursor.NextFeature();
                while (pFeature != null)
                {
                    IGeometry pGeometry = pFeature.Shape;

                    //2018-02-22 张久君 添加线的平滑处理
                    //IPolyline pSmoothResult = DefinePolylineSmooth(pGeometry as IPolyline);

                    JsonObject pFeatureJsonObject = new JsonObject();
                    pFeatureJsonObject.AddLong("OID",pFeature.OID);
                    pFeatureJsonObject.AddDouble("Contour",Convert.ToDouble(pFeature.Value[pCountourValueFieldIndex].ToString()));
                    JsonObject pGeometryJsonObject = ESRI.ArcGIS.SOESupport.Conversion.ToJsonObject(pGeometry, true);

                    pFeatureJsonObject.AddJsonObject("GeometryLine",pGeometryJsonObject);
                    pJsonObjectList.Add(pFeatureJsonObject);
                    pFeature = pFeatureCursor.NextFeature();
                }
                JsonObject pJsonObjectResult = new JsonObject();
                pJsonObjectResult.AddString("state", "success");
                pJsonObjectResult.AddArray("ContourLine", pJsonObjectList.ToArray());
                return pJsonObjectResult;
            }
            catch (Exception ex)
            {
                JsonObject pJSONObjectResult = new JsonObject();
                pJSONObjectResult.AddString("state", "fail");
                pJSONObjectResult.AddString("exception",ex.Message.ToString());
                return pJSONObjectResult;
            }

        }

        /// <summary>
        /// 张久君 2017-11-21 修改 实现栅格数据的重分类,并以输出参数的方式输出栅格重分类的分类项
        /// </summary>
        /// <param name="pInputIGeoDataset"></param>
        /// <returns></returns>
        public static IGeoDataset RasterReclassify(IGeoDataset pInputIGeoDataset, out Dictionary<int, ReclassifyValue> pOutDictionaryRasterReclassify)
        {
            //用来组织栅格重分类的分类标准
            Dictionary<int, ReclassifyValue> pDictionaryRasterReclassify = new Dictionary<int, ReclassifyValue>();

            try
            {
                IRaster pInputRaster = pInputIGeoDataset as IRaster;

                IRasterDescriptor pRD = new RasterDescriptorClass();
                pRD.Create(pInputRaster, new QueryFilter(), "Value");
                IReclassOp pReclassOp = new RasterReclassOpClass();
               
                IRasterAnalysisEnvironment pRasterAnalysisEnvironment = pReclassOp as IRasterAnalysisEnvironment;
                object objSnap = null;
                object objExtent = pInputIGeoDataset.Extent;
                pRasterAnalysisEnvironment.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, ref objExtent, ref objSnap);
                pRasterAnalysisEnvironment.OutSpatialReference = pInputIGeoDataset.SpatialReference;

                IRasterBandCollection pRsBandCol = pInputIGeoDataset as IRasterBandCollection;
                IRasterBand pRasterBand = pRsBandCol.Item(0);
                pRasterBand.ComputeStatsAndHist();
                IRasterStatistics pRasterStatistic = pRasterBand.Statistics;
                double pMaxValue = pRasterStatistic.Maximum;
                double pMinValue = pRasterStatistic.Minimum;
                INumberRemap pNumRemap = new NumberRemapClass();

                //根据输入raster数据的最大值与最小值计算分割单位
                int pMinValue_int = Convert.ToInt16(pMinValue);
                int pMaxValiue_int = Convert.ToInt16(pMaxValue) + 1;

                double pAverage = Math.Round((pMaxValue - pMinValue) / 10,2);//进行重分类时分10类
                //if (pAverage == 0)
                //{
                //    pAverage = 1;
                //}
                double pCurrentRempValue = pMinValue_int;
                int i = 0;
                while (pCurrentRempValue <= pMaxValiue_int)
                {
                    ReclassifyValue pReclassifyValue = new ReclassifyValue();
                    pReclassifyValue.IndexFlag = i;
                    pReclassifyValue.MinValue = pCurrentRempValue;
                    pReclassifyValue.MaxValue = pCurrentRempValue + pAverage;
                    pDictionaryRasterReclassify.Add(i, pReclassifyValue);
                    
                    pNumRemap.MapRange(pCurrentRempValue, pCurrentRempValue + pAverage,i);
                    pCurrentRempValue = pCurrentRempValue + pAverage;
                    i++;
                }

                IRemap pRemap = pNumRemap as IRemap;
                IGeoDataset pOutGeoDataSet = pReclassOp.ReclassByRemap(pInputIGeoDataset, pRemap, false);
                pOutDictionaryRasterReclassify = pDictionaryRasterReclassify;
                return pOutGeoDataSet;

            }
            catch (Exception ex)
            {
                pOutDictionaryRasterReclassify = null;
                return null;
            }
        }

        /// <summary>
        /// 张久君 2017-11-21 实现重分类后的数据转换为Polygon，并以JsonObject的格式返回
        /// </summary>
        /// <param name="pRasterGeodataset"></param>
        /// <returns></returns>
        public static JsonObject RasterToPolygonJsonObject(IGeoDataset pRasterGeodataset, ISpatialReference pSpatialReference,Dictionary<int,ReclassifyValue> pRasterReclassifyDictionary)
        {
            try
            {
                //创建一个内存工作空间
                IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactory();
                string WorkspaceName = Guid.NewGuid().ToString().Replace("-", "");
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", WorkspaceName, null, 0);
                ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
                ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();

                IConversionOp pConversionOp = new RasterConversionOpClass();
                //create Raster Analysis Enviroment
                IRasterAnalysisEnvironment env = (IRasterAnalysisEnvironment)pConversionOp;
                env.OutSpatialReference = pSpatialReference;

                IGeoDataset pGeoDatasetResult = pConversionOp.RasterDataToPolygonFeatureData(pRasterGeodataset, inmemWor, "ff", true);
                IFeatureClass pFeatureClassResult = pGeoDatasetResult as IFeatureClass;
                IFeatureCursor pFeatureCursor = pFeatureClassResult.Search(null,false);
                IFeature pFeature = pFeatureCursor.NextFeature();
                List<JsonObject> pJsonObjectList = new List<JsonObject>();

                int pCountourValueFieldIndex = pFeatureClassResult.FindField(@"gridcode");


                while (pFeature != null)
                {
                    IGeometry pGeometry = pFeature.Shape;
                    JsonObject pFeatureJsonObject = new JsonObject();
                    pFeatureJsonObject.AddLong("OID", pFeature.OID);

                    int gridcode = Convert.ToInt32(pFeature.Value[pCountourValueFieldIndex].ToString());
                    //通过获取的“Contour”值得到对应的实际值区间
                    ReclassifyValue pReclassifyValue = null;
                    bool GetState = pRasterReclassifyDictionary.TryGetValue(gridcode, out pReclassifyValue);

                    pFeatureJsonObject.AddDouble("ReclassifyMinValue", pReclassifyValue.MinValue);
                    pFeatureJsonObject.AddDouble("ReclassifyMaxValue", pReclassifyValue.MaxValue);
                    pFeatureJsonObject.AddDouble("ReclassifyMedian", pReclassifyValue.Median);

                    //获取面积
                    IArea pArea = pGeometry as IArea;
                    pFeatureJsonObject.AddDouble("Area", pArea.Area);

                    JsonObject pGeometryJsonObject = ESRI.ArcGIS.SOESupport.Conversion.ToJsonObject(pGeometry, true);
                    pFeatureJsonObject.AddJsonObject("GeometryPolygon", pGeometryJsonObject);
                    pJsonObjectList.Add(pFeatureJsonObject);
                    pFeature = pFeatureCursor.NextFeature();

                }

                JsonObject pResultJsonObject = new JsonObject();
                pResultJsonObject.AddString("state","success");
                pResultJsonObject.AddArray("ContourPlane", pJsonObjectList.ToArray());
                return pResultJsonObject;

            }
            catch (Exception ex)
            {
                JsonObject pResultJsonObject = new JsonObject();
                pResultJsonObject.AddString("state", "fail");
                return pResultJsonObject;
            }


        }


        //public static void InsertPointToFeatureClass(IFeatureClass pFeatureClass, List<DefinePoint> pDefinePointList, string FieldName)
        //{
        //    //获取降雨量监测值字段的序号
        //    IFields pFields2 = pFeatureClass.Fields;
        //    int pRainfallFieldIndex = pFields2.FindField(FieldName);

        //    for (int i = 0; i < pDefinePointList.Count; i++)
        //    {
        //        IPoint pPoint = new Point();
        //        pPoint.X = pDefinePointList[i].X;
        //        pPoint.Y = pDefinePointList[i].Y;
        //        IFeature pFeature = pFeatureClass.CreateFeature();
        //        pFeature.Shape = (IGeometry)pPoint;
        //        pFeature.Value[pRainfallFieldIndex] = pDefinePointList[i].value;
        //        pFeature.Store();
        //    }

        //}




        //////////////////////////////////////
    }
}
