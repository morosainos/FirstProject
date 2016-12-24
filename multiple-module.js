/**
 * Author:Celery celery.mingyu.qin@kingland.com 
 * Date:2016-12-21
 * Description:Controller for multiple choice template
 */
(function() {
	"use strict";
	(function() {
		var multipleTemplate = angular.module('multiple', ['angular-simditor', 'ngSanitize']);
		multipleTemplate.directive('multipleTemplate', function() {
			return {
				restrict: 'AE',
				templateUrl: "/resources/templates/multiple-choice-question/multiple-module.html",

				scope: {
					loadQuestion: '&',
					loadAnswer: '&',
					checkUser: '&'
				},

				link: function(scope, element) {

					var userType = scope.checkUser();
					//接收用户id判断用户是KSC还是Applicant
					if(userType === "ksc") {
						scope.isEditable = true;
					} else {
						scope.isEditable = false;
					}
					
					scope.question = scope.loadQuestion();
					var answers = scope.loadAnswer();
					var number = answers[0].number;
					
					scope.info = [];
					// 假设这是数据来源
					var simp = new Array(number);
					for(var i = 0; i < number; i++) {
						simp[i] = answers[1].choice[i];
					}
					angular.forEach(simp, function(data, index) {
						scope.info.push({
							key: index,
							value: data
						});
					});
					// 增加
					scope.add = function() {
						scope.info.splice(number, 0, {
							key: number,
							value: ""
						});
					};
					// 删除
					scope.delete = function(num) {
						scope.info.splice(num, 1);
					};

					//按下save将题和答案的信息发送
					scope.save = function() {
						console.log(scope.question);
						console.log(scope.checkChoice1);
					};

					//初始化
					function init() {
						scope.checkChoice1 = false;
						scope.checkChoice2 = false;
						scope.checkChoice3 = false;
						scope.checkChoice4 = false;
					}

					init();
					scope.click = function() {
						scope.isEditable = !scope.isEditable;
					};

				}
			}
		});
	})();
})();