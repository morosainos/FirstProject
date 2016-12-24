/**
 * Author:Celery celery.mingyu.qin@kingland.com
 * Date:2016-12-21
 * Description:Controller for multiple choice template
 */

'use strict';

angular.module(window.otp.appName, ['multiple']).controller('app-controller', function($scope, $http, $interval) {

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

	$scope.putQuestion = function(name) {
		//alert(name);
		$http({
			method: 'GET',
			url: 'data.json'
		}).success(function(data) {
			//	console.log(data[0].name);
			//	return "data[0].name";
		}).error(function(data) {
			console.log("error");
		});
		return "<b>如果发现一个好产品或者一个大市场，就应该立即跟进，在跟进中发现现有产品的缺陷，然后通过创新弥补缺陷，超越对手，" +
			"实现后来居上。当然，创新的目标是改造，而不是简单模仿。因此，创造性模仿者需要通过对他人创意的了解， 重新组合、" +
			"改良而产生具有不同功能与价值的新东西。事实上，所有的产品，除了第一代是原创的， 以后的进步都是通过创造性模仿来实现的。这段文字旨在说明：<b>";
	};

	$scope.putAnswers = function() {

		return [{
			number: 4
		}, {
			choice: ["创造性模仿是改良产品的主要途径", 
			"<b>通过不断的改善就能得到新的产品</b>", 
			"创新来源于对现有产品缺陷的弥补", 
			"大部分产品都是通过模仿来完成的"]
		}];
	}
	$scope.putUser = function() {
		return "ksc";
	}

});