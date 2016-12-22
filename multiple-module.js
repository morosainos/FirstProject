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
				templateUrl: "/static/otpexternal/login/multiple-choice-template/multiple-module.html",

				scope: {
					// Step 2: greet方法绑定到onGreet属性（对应Html中的on-greet），并将greet的输入参数传给onGreet
					'greet': '&ongreet',
					getTitle: '&',
					getContent: '&',
					show: '&' //有参情况
				},
				controller: function($scope) {
					if($scope.color > 2)
						console.log("duile");
					$scope.ss = function() {
						console.log("yes");
					}
					$scope.title = $scope.getTitle(); //调用无参函数  
					$scope.contents = $scope.getContent(); //调用无参函数 
				},
				link: function(scope, http) {
					//接收用户id判断用户是KSC还是Applicant
					scope.checkUserType = function() {
						var id = 1;
						if(id == 0)
							scope.isEditable = true;
						else {
							scope.isEditable = false;
						}
					};
					//按下save将题和答案的信息发送
					scope.save = function() {
						console.log(scope.question);
						console.log(scope.checkChoice1);
					};
					//通过题ID加载试题和选项信息
					scope.loadQuestion = function() {

						}
						//添加选项
					scope.addQuestion = function() {

						}
						//删除选项
					scope.deleteQuestion = function() {
						console.log("aa");
					}

					//初始化
					scope.init = function() {
						scope.checkChoice1 = false;
						scope.checkChoice2 = false;
						scope.checkChoice3 = false;
						scope.checkChoice4 = false;
						scope.checkUserType();
						scope.loadQuestion();
					}

					scope.init();
					scope.choice1 = "创造性模仿是改良产品的主要途径";
					scope.choice2 = "通过不断的改善就能得到新的产品";
					scope.choice3 = "创新来源于对现有产品缺陷的弥补";
					scope.choice4 = "大部分产品都是通过模仿来完成的";
					scope.question = '如果发现一个好产品或者一个大市场，就应该立即跟进，在跟进中发现现有产品的缺陷，然后通过创新弥补缺陷，超越对手，' +
						'实现后来居上。当然，创新的目标是改造，而不是简单模仿。因此，创造性模仿者需要通过对他人创意的了解， 重新组合、' +
						'改良而产生具有不同功能与价值的新东西。事实上，所有的产品，除了第一代是原创的， 以后的进步都是通过创造性模仿来实现的。这段文字旨在说明：';
				}
			}
		});
		multipleTemplate.controller('multipleCtrl', function($scope) {
			$scope.greet = function() {
				console.log("greet");
				return {
					message: 'Hello',
					name: 'Tom'
				};
			}
		});

	})();
}).call(this);