
continue https://confluence.kingland.com/display/DHTD/360+Master+File+Specification

https://www.bing.com/search?q=java+%E5%8F%8D%E5%B0%84%E6%9C%BA%E5%88%B6&qs=AS&pq=java+%E5%8F%8D%E5%B0%84&sk=AS2&sc=8-7&cvid=AD39EC46C3F14E18BE369B399131932D&FORM=CHRDEF&sp=3


var app = angular.module('myApp', []);
app.controller('myCtrl', function ($scope, $http) {
    $http.get("test.ask").success(function (response) {

        $scope.myWelcome = response;

    });
});

@ResponseBody
    @RequestMapping(value = "test", method = RequestMethod.GET)
    public String test() {


        return "55555";
    }


AngularJs中$http发送post或者get请求,SpringMVC后台接收不到参数值的解决办法
1.问题原因

默认情况下，jQuery传输数据使用Content-Type: x-www-form-urlencodedand和类似于"name=zhangsan&age=18"的序列，然而AngularJS，传输数据使用Content-Type: application/json和{ "name": "zhangsan", "age": "18" }这样的json序列。

2.解决办法

A.服务端进行修改，在Controller接收参数的方法中，对象前加 @RequestBody (注：要用对象的方式来接参数)
B.客户端修改


$http.post(url,{},{params:{"name": "zhangsan", "age": "18"}}).success(function(data){

    });
    
$http.get(url,{params:{"name": "zhangsan", "age": "18"}}).success(function(data){

    });

