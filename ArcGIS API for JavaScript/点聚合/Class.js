/**
 * @constructor 名称：ClusterTool
 * @description 作用：用来创建聚合点图层
 * @author 张健 
 * @date 2018/2/9
 */
Robin.Portal.ClusterTool = {
    Point: [],
    id:"",
    Creat: function ( id ) {
        id=id+(new Date().getSeconds().toString())
        this.id = id;
        require( ["../js/plugin/ClusterHeatmap/ClusterLayer.js", ], function ( ClusterLayer ) {
            clusterLayer = new ClusterLayer( {
                "data": Robin.Portal.ClusterTool.Point,
                "distance": 60,
                "id": id,
                "labelColor": "#fff",
                "labelOffset": 10,
                "resolution": Robin.Map.Map2DControl.extent.getWidth() / Robin.Map.Map2DControl.width,
                "singleColor": "#888",
            } );
            var defaultSym = new esri.symbol.PictureMarkerSymbol( "../images/hydrant/bz-normal.png", 24, 36 ).setOffset( 0, 15 );
            var renderer = new esri.renderer.ClassBreaksRenderer( defaultSym, "clusterCount" );
            var picBaseUrl = "../images/hydrant/polymerize/";
            var less = new esri.symbol.PictureMarkerSymbol( picBaseUrl + "cluster-lower.png", 36, 36 ).setOffset( 0, 15 );
            var normal = new esri.symbol.PictureMarkerSymbol( picBaseUrl + "cluster-middle.png", 48, 48 ).setOffset( 0, 15 );
            var more = new esri.symbol.PictureMarkerSymbol( picBaseUrl + "cluster-high.png", 60, 60 ).setOffset( 0, 15 );
            renderer.addBreak( 1, 5, less );
            renderer.addBreak( 5, 50, normal );
            renderer.addBreak( 50, 1001, more );
            clusterLayer.setRenderer( renderer );
            Robin.Map.Map2DControl.addLayer( clusterLayer );
            dojo.connect( clusterLayer, 'onClick', function ( evt ) {
                var level;
                //如果有聚合,放大一级
                if ( evt.graphic.attributes.clusterCount > 1 ) {
                    level = Robin.Map.Map2DControl.getZoom() + 1;
                }
                else {
                    level = Robin.Setting.GlobalSetting.maxZoomLevel;
                }
                Robin.Map.Map2DControl.setZoom( level );
                Robin.Map.Map2DControl.centerAt( Robin.Map.GetPoint( evt.graphic.geometry.x, evt.graphic.geometry.y ) );
            } )
        } );
        Robin.Portal.ClusterTool.open();
    },
    open: function () {
        Robin.Map.Map2DControl.on( "zoom-end", function ( zoom ) {
            level = zoom.level;
            Robin.Portal.ClusterTool.ZoomEndEvent( level );
        } )
    },
    close: function () {
        Robin.Map.Map2DControl.on( "zoom-end", function ( zoom ) {
        } );
        Robin.Map.ClearLayer( Robin.Map.Map2DControl,this.id );
    },
    ZoomEndEvent: function (level) {
        if ( level < 5 ) {
            var graphicLayer = Robin.Map.GetGraphicLayer( Robin.Map.Map2DControl, this.id );
            if ( graphicLayer )
                graphicLayer.show();
            var graphicLayer = Robin.Map.GetGraphicLayer( Robin.Map.Map2DControl, Robin.Setting.MapAnalyse.hydrant.hydrantLayerName );
            if ( graphicLayer )
                graphicLayer.hide();
        }
        else if ( level >= 5 ) {//当缩放到第六级时候，隐藏聚合图层，显示普通图层
            var graphicLayer = Robin.Map.GetGraphicLayer( Robin.Map.Map2DControl, this.id );
            if ( graphicLayer )
                graphicLayer.hide();
            var graphicLayer = Robin.Map.GetGraphicLayer( Robin.Map.Map2DControl, Robin.Setting.MapAnalyse.hydrant.hydrantLayerName );
            if ( graphicLayer )
                graphicLayer.show();
        }
    }
}




//添加数据
Robin.Portal.ClusterTool.Point.push( { x: parseFloat( x ), y: parseFloat( y ), attr: {} } );
//创建聚合图层
Robin.Portal.ClusterTool.Creat("HydrantCluster")