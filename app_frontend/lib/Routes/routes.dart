import 'package:app_frontend/Views/views.dart';
import 'package:flutter/material.dart';

class Routes {
	Routes._();

	static Map<String, WidgetBuilder> get routes => {
		'/home': (context) => const HomeView(),
		'/login': (context) => const LoginView(),
		'/register': (context) => const RegisterView(),
		'/onboarding': (context) => OnboardingView(),
	};
}