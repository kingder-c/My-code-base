const fs=require('fs');

// fs.writeFile('./xxx/yyy.txt','大家早上好','utf-8',function(err){
//     if(err){
//         console.log(err)
//         throw err;
//     }
//     console.log('ok')
// })


try {
    fs.writeFile("./xxx/yyy.txt","大家早上好",'utf-8',function(error){
        console.log("start");
        console.log(error)
    })
    console.log("end")
}
catch(e){
    console.log(e);
}


