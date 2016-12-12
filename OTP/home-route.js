/**
 * Created by Celery on 12/12/2016.
 */
'use strict';  
(function() {
	/* Setup Rounting For All Menu Pages */
	var myApp = angular.module("homeApp", ['ui.router']);
	myApp.config(function($stateProvider, $urlRouterProvider) {
		$urlRouterProvider.when("", "/userhome");
		$stateProvider
			.state("userhome", {
				url: "/userhome",
				templateUrl: "userhome.html"
			})
		$stateProvider
			.state("userhome.review-process", {
				url: "/review-process",
				templateUrl: "menu-pages/review-process.html"
			})
			.state("userhome.interview-process", {
				url: "/interview-process",
				templateUrl: "menu-pages/interview-process.html"
			})
			.state("userhome.application-management", {
				url: "/application-management",
				templateUrl: "menu-pages/application-management.html"
			})
			.state("userhome.ksc-user-management", {
				url: "/ksc-user-management",
				templateUrl: "menu-pages/ksc-user-management.html"
			})
			.state("userhome.question-maintenance", {
				url: "/question-maintenance",
				templateUrl: "menu-pages/question-maintenance.html"
			})
			.state("userhome.import", {
				url: "/import",
				templateUrl: "menu-pages/import.html"
			})
			.state("userhome.export", {
				url: "/export",
				templateUrl: "menu-pages/export.html"
			})
			.state("userhome.configuration", {
				url: "/configuration",
				templateUrl: "menu-pages/configuration.html"
			})
			.state("userhome.report", {
				url: "/report",
				templateUrl: "menu-pages/report.html"
			})

	});
}).call(this);