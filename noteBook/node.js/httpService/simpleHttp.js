//加载Http模块 
const http = require('http');

var server = http.createServer();

server.on('request',function(req,res){
    //解决乱码的方式是服务器通过设置响应报文头告诉浏览器相应的编码来解析网页
    res.setHeader('Content-Type','text/html; charset=utf-8')
    res.write("123<h1>你好</h1>世界");
    res.end();
})

server.listen(8080,function(){
    console.log("server start~")
})