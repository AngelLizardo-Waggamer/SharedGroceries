import 'package:app_frontend/Views/views.dart';
import 'package:flutter/material.dart';

class Routes {
	Routes._();

  static String get home => '/home';
  static String get login => '/login';
  static String get register => '/register';
  static String get onboarding => '/onboarding';
  static String get profile => '/profile';
  static String get family => '/family';

	static Map<String, WidgetBuilder> get routes => {
		home: (context) => const HomeView(),
		login: (context) => const LoginView(),
		register: (context) => const RegisterView(),
		onboarding: (context) => const OnboardingView(),
		profile: (context) => const ProfileView(),
		family: (context) => const FamilyView(),
	};
}