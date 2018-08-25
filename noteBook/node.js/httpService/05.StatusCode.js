const http = require('http');

http.createServer(function(req,res){
    res.statusCode = 404;
    res.statusMessage = "fafafafa";
    res.end("hello world 你好世界");
    
}).listen(8080,function(){
    console.log('server Running');
})