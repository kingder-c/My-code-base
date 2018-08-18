
  new Vue({
    el: '#app',
    methods:{
        add:function(){
            console.log("asdf")
            this.tableData.push({id:this.id,name:this.name,ctime:new Date().toString()})

        },
        del(row){
            // this.tableData.some((item,i)=>{

            //     if(item==row){
            //         this.tableData.splice(i,1);
            //         return true;

            //     }
            // })

            this.tableData.splice(this.tableData.findIndex(item=>{
                if(item==row){
                    return true;
                }
            }),1);
            
        },
        search(){
            console.log(this.keyword);
            console.log(this.tableData);
            console.log(this.tableData.filter(item=>{
                item.name.indexOf(this.keyword)!=-1
            }))
            return this.tableData.filter(item=>{
                return item.name.indexOf(this.keyword)!=-1
            })
        }
    },
    data: {
        id:"",
        name:"",
        keyword:"",
          tableData: [{
            id: '02',
            name: '宝马',
            ctime: new Date().toString(),
            del:'<a href="#" @click.prevent="del(this.id)">删除</a>'
          }, {
            id: '04',
            name: '奔馳',
            ctime: new Date().toString()
          }, {
            id: '01',
            name: '自行车',
            ctime: new Date().toString()
          }, {
            id: '20',
            name: '垃圾车',
            ctime: new Date().toString()
          }]
        }
  })