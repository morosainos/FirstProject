Need another step in Sentry to process all requires_sentry=H. Gathers entire tree and calls PersistConsumptionHierarchy. Checks Rel DABPs,
and ensures all records already exist in consumption. If any record in tree is not in consumption, entire tree is logged as skipped
and will be tried the next day. Multi-threading this could be tricky if entities can be in more than one tree. Possibly need database enforced
constraints on consumption relationship table to ensure duplicate relationships don't get added.

1 https://confluence.kingland.com/display/DHTD/Feature+Design+-+Updates+to+relationship+logic+in+Sentry+when+records+are+in+DABPs
2 https://confluence.kingland.com/display/DHTD/Registry+and+Sentry+Redesign
continue https://confluence.kingland.com/display/DHTD/360+Master+File+Specification
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

