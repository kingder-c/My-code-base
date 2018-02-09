    //根据起点和终点进行缩放
    ZoomToTwoPoint: function ( map, x1, y1, x2, y2 ) {
        var extent;
        if ( x1 < x2 ) {
            var t;
            t = x1;
            x1 = x2;
            x2 = t;
        }
        if ( y1 < y2 ) {
            var t;
            t = y1;
            y1 = y2;
            y2 = t;
        }
        extent=new esri.geometry.Extent(x2,y2,x1,y1,map.spatialReference)
        if ( extent != null ) {
            var point = new esri.geometry.Point( extent.xmin + ( extent.xmax - extent.xmin ) / 2, extent.ymin + ( extent.ymax - extent.ymin ) / 2, map.spatialReference );
            var newExtent = new esri.geometry.Extent( point.x, point.y, point.x, point.y, point.spatialReference );
            //如果当前视图包含要缩放视图
            if ( Robin.Map.Extent1ContainExtent2( map.extent, extent ) ) {
                 extent = extent.expand(2);
                map.setExtent( extent );
            } else {
                var firstEx = Robin.Map.Union2Extent( newExtent, map.extent );
                map.setExtent( firstEx, true );
                setTimeout( function () {
                    map.centerAt( point )
                }, 700 );
                setTimeout( function () {
                    extent = extent.expand( 1.5 );
                    map.setExtent( extent );
                }, 1400 );
            }
        }
    }




    //根据要素缩放
    flayCirle: function (map, geometry) {
        var extent = geometry.getExtent();
        if (geometry.type == "point") {
            extent = new esri.geometry.Extent(geometry.x - 0.0000001, geometry.y - 0.0000001, geometry.x - 0 + 0.0000001, geometry.y - 0 + 0.0000001, map.spatialReference);
            extent = extent.expand(1.5);
        }
        if (extent != null) {
            var point = new esri.geometry.Point(extent.xmin + (extent.xmax - extent.xmin) / 2, extent.ymin + (extent.ymax - extent.ymin) / 2, map.spatialReference);
            var newExtent = new esri.geometry.Extent(point.x, point.y, point.x, point.y, point.spatialReference);
            //如果当前视图包含要缩放视图
            if (Robin.Map.Extent1ContainExtent2(map.extent, extent)) {
                // extent = extent.expand(2);
                map.setExtent(extent);
            } else {
                var firstEx = Robin.Map.Union2Extent(newExtent, map.extent);
                map.setExtent(firstEx, true);
                setTimeout(function () {
                    map.centerAt(point)
                }, 700);
                setTimeout(function () {
                    extent = extent.expand(1.5);
                    map.setExtent(extent);
                }, 1400);
            }
        }
    }	
	
	
	
	
	
	