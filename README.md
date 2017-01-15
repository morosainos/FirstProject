file:///D:/DSRO_FILE/360 Master File Specification - DSR Hub Technical Documents - Draft Space - Confluence.html
Figure 16 – Fund Monitoring: Security Subscription Example
Organization Monitoring (OM)
The organization monitoring product provides data on subscribed organizations, its direct parent, and ultimate parent. The data elements received for the subscribed record is based on the consumers' data element specification for the organization monitoring product defined in the contract. The consumer will receive the direct parent control relationship for the organization. These records are shell records with only the following data attributes.
Hub IdKINS
Legal name
Physical Country
If the parent and/or ultimate parent also happen to be enrolled into a different product, the full record containing subscribed data elements is supplied (subscription supersedes shell record). 
Additionally, this product includes joint venture (exactly 50/50) relationships if the ultimate parent of the family tree is the child in a joint venture. The parents in the joint venture are provided in the consumer's master file as an auto-enrolled shell record. 




# html
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

http://www.html-js.com/article/Front-end-source-code-analysis-original-uirouter-source-code-analysis
http://www.myexception.cn/javascript/2035214.html
