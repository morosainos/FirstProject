/**
 * Author:Celery celery.mingyu.qin@kingland.com
 * Date:2016-12-21
 * Description:Controller for multiple choice template
 */

'use strict';

angular.module(window.otp.appName, ['multiple']).controller('app-controller', function($scope, $http, $window, $location, $interval, $templateCache) {
	function init() {
		$scope.time = 1000;
		$scope.totalEpicQuestion = 6;
		$scope.point = 4;
		$scope.questionOrder = 1;
		$scope.currentQuestion = 1;
		$scope.totalQuestion = 24;
	}
	init();

	function tik() {
		$scope.time = $scope.time - 1;
	}
	var timer = $interval(function() {
		tik();
	}, 1000);
	$scope.find = function() {
		console.log("aa");
	}
	$scope.sayHello = function(message, name) {
		$scope.name = name;
		$scope.message = message;
		alert($scope.message + ',' + $scope.name);
	};

	$scope.title = "标题";
	$scope.contents = [{
		text: 1234
	}, {
		text: 5678
	}];
	$scope.showName = function(name) {
		alert(name);
	};
});