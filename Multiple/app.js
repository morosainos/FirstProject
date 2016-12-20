var appModule = angular.module('myApp', ['angular-simditor']);


appModule.run(function($templateCache) {
	//    $templateCache.put('new_file.html','new_file.html')
	$templateCache.put('new_file.html', '<h1>fff</h1>')
})
//.config(['simditorConfig',function(simditorConfig) {
//	simditorConfig.placeholder = '这里输入文字...';
//}])
//.controller('TestCtrl', ['$scope', function($scope){
//	$scope.test = 'test content';
//}]);
appModule.directive('hello', function() {
	return {
		restrict: 'E',
		templateUrl: 'new_file.html',
		replace: true
	};
});
appModule.controller('myCtrl', function($scope) {
	$scope.aa = "aaa"
})