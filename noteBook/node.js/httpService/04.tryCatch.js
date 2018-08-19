const fs=require('fs');

fs.writeFile('./xxx/yyy.txt','大家早上好','utf-8',function(err){
    if(err){
        console.log(err)
        throw err;
    }
    console.log('ok')
})



