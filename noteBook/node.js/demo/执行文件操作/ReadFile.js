const fs =require('fs');
console.log(__dirname);
console.log(__filename)
fs.readFile(__dirname+"\\"+"helloWorld",function(err,data){
    if(err){
        throw err;

    }
    else{
        //data参数的数据类型是一个Buffer对象,里面报讯的是一个一个的自己(理解为自己数组)
        console.log(data.toString('utf8'))
    }

})

fs.readFile(__dirname+"\\"+"helloWorld","utf8",function(err,data){
    if(err){
        throw err;

    }
    else{
        //如果读文件的时候传入了字符编码就自动解析成对应的编码格式
        console.log(data);
    }

})