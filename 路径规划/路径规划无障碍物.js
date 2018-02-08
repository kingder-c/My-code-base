/**
 * @constructor 名称：Route
 * @description 作用：路径规划及网络分析
 * @author 张健 
 * @date 2018/2/8
 */
Robin.Portal.Route = {
    routePlan: function ( x1, y1, x2, y2,  callBack ) {
        require( [
                   "esri/symbols/SimpleLineSymbol", "esri/Color",
                   "esri/tasks/RouteTask",
                    "esri/tasks/FeatureSet",
                   "esri/tasks/RouteParameters"
        ], function () {
            var routeServiceUrl = Robin.Setting.MapAnalyse.route.url;
            var routeGraphicLayer = Robin.Map.GetGraphicLayer( top.Robin.Map.Map2DControl, Robin.Setting.MapAnalyse.route.routeLayerName );
            if ( routeGraphicLayer != null ) {
                routeGraphicLayer.clear();
            }
            var map = top.Robin.Map.Map2DControl;
            var wkid = Robin.Setting.GlobalSetting.wkid;
            //起点、终点
            var ptStart = new esri.geometry.Point( parseFloat( x1 ), parseFloat( y1 ), new esri.SpatialReference( { wkid: wkid } ) );
            var ptEnd = new esri.geometry.Point( parseFloat( x2 ), parseFloat( y2 ), new esri.SpatialReference( { wkid: wkid } ) );
            var startPtGra = new esri.Graphic( ptStart );
            var endPtGra = new esri.Graphic( ptEnd );

            //设置路径样式
            var routeSymbol = new esri.symbol.SimpleLineSymbol( esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color( [82, 152, 255, 1] ), 5 );
            //起点、终点尾部线
            var routeSymbolDef = new esri.symbol.SimpleLineSymbol( esri.symbol.SimpleLineSymbol.STYLE_SOLID, new dojo.Color( [77, 220, 38, 1] ), 5 );

            var routeTask = new esri.tasks.RouteTask( routeServiceUrl );
            var routeParams = new esri.tasks.RouteParameters();
            //设置参数
            routeParams.outSpatialReference = { wkid: wkid };
            routeParams.returnRoutes = true;
            routeParams.returnDirections = true;
            routeParams.directionsLengthUnits = esri.Units.METERS;
            routeParams.stops = new esri.tasks.FeatureSet();
            routeParams.stops.features.push( startPtGra );
            routeParams.stops.features.push( endPtGra );
            if ( routeParams.stops.features.length == 0 ) {
                noty( { text: "路径规划输入参数不全，无法分析", type: "warning", layout: "topCenter", timeout: 2000 } );
                return;
            }

            routeTask.solve( routeParams, function ( solveResult ) {
                if ( callBack ) {
                    callBack( solveResult );
                }

                var routeResults = solveResult.routeResults;
                var res = routeResults.length;
                if ( res > 0 ) {
                    for ( var i = 0; i < res; i++ ) {
                        var graphicroute = routeResults[i];
                        var graphic = graphicroute.route;
                        graphic.setSymbol( routeSymbol );
                        routeGraphicLayer.add( graphic );

                        //连接路线起点、终点与对应定位起点、终点
                        var paths = graphic.geometry.paths[0];
                        var routeStr = paths[0];
                        var routeEnd = paths[paths.length - 1];

                        var routeStrline = new esri.geometry.Polyline( [[x1, y1], routeStr], new esri.SpatialReference( { wkid: wkid } ) );
                        var routeEndline = new esri.geometry.Polyline( [[x2, y2], routeEnd], new esri.SpatialReference( { wkid: wkid } ) );
                        var strlinegraphic = new esri.Graphic( routeStrline, routeSymbolDef );
                        var endlinegraphic = new esri.Graphic( routeEndline, routeSymbolDef );

                        routeGraphicLayer.add( strlinegraphic );
                        routeGraphicLayer.add( endlinegraphic );
                    }
                }
                else {
                    //alert("路径规划返回结果空");
                }
            }, function ( error ) {
                noty( { text: "没有找到合适的路线", type: "warning", layout: "topCenter", timeout: 2000 } );
            } );
        } );
    }
}
