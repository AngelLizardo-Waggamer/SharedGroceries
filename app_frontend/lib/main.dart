import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/repositories.dart';
import 'package:app_frontend/Database/app_db.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Theme/app_theme.dart';
import 'package:app_frontend/config.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';

void main() async {
	WidgetsFlutterBinding.ensureInitialized();

	await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);

	final session = SessionManager();
	final apiClient = ApiClient(baseUrl: Config.apiURL, apiVersion: Config.apiVersion, sessionManager: session);
	final repositories = Repositories(apiClient: apiClient, sessionManager: session);
	final database = AppDatabase();

	final isAuthenticated = await session.getAuthToken() != null;

	runApp(MainApp(
		session: session,
		apiClient: apiClient,
		repositories: repositories,
		database: database,
		isAuthenticated: isAuthenticated,
	));
}

class MainApp extends StatelessWidget {
	const MainApp({
		super.key,
		required this.session,
		required this.apiClient,
		required this.repositories,
		required this.database,
		required this.isAuthenticated,
	});

	final SessionManager session;
	final ApiClient apiClient;
	final Repositories repositories;
	final AppDatabase database;
	final bool isAuthenticated;

	@override
	Widget build(BuildContext context) {
		return MultiProvider(
			providers: [
				Provider<SessionManager>.value(value: session),
				Provider<ApiClient>.value(value: apiClient),
				Provider<Repositories>.value(value: repositories),
				Provider<AppDatabase>.value(value: database),
			],
			child: MaterialApp(
                debugShowCheckedModeBanner: false,
				title: 'Shared Groceries',
				themeMode: .light,
				theme: AppTheme.light,
				initialRoute: isAuthenticated ? Routes.home : Routes.login,
				routes: Routes.routes,
			),
		);
	}
}
