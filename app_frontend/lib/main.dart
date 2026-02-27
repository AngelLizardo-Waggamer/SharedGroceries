import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Theme/app_theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

void main() async {

	WidgetsFlutterBinding.ensureInitialized();
	await SystemChrome.setPreferredOrientations([
		DeviceOrientation.portraitUp,
	]);

	runApp(const MainApp());
}

// Validación inicial para saber si tiene una sesión activa o no.
bool _isAuthenticated() {
	// TODO: verificar token/sesión almacenada
	return false;
}

class MainApp extends StatelessWidget {
	const MainApp({super.key});

	@override
	Widget build(BuildContext context) {
	
	return MaterialApp(
		title: 'Shared Groceries',
		themeMode: .light,
		theme: AppTheme.light,
		initialRoute: _isAuthenticated() ? '/home' : '/login',
		routes: Routes.routes
	);
	}
}
