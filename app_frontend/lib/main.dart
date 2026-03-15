import 'package:app_frontend/API/api_client.dart';
import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_expiration_recovery.dart';
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
	final signalRClient = SignalRClient(sessionManager: session, apiClient: apiClient);
	final navigatorKey = GlobalKey<NavigatorState>();
	final repositories = Repositories(apiClient: apiClient, sessionManager: session);
	final database = AppDatabase();

	// Global fallback when the session expires from any async layer.
	SessionExpirationRecovery.instance.configure(
		sessionManager: session,
		signalRClient: signalRClient,
		navigatorKey: navigatorKey,
	);

	final isAuthenticated = await session.getAuthToken() != null;
	final familyId = await session.getFamilyId();

	// Best-effort startup connect: failures are handled in-screen.
	if (isAuthenticated && familyId != null) {
		await signalRClient.connect();
	}

	runApp(MainApp(
		navigatorKey: navigatorKey,
		session: session,
		apiClient: apiClient,
		signalRClient: signalRClient,
		repositories: repositories,
		database: database,
		isAuthenticated: isAuthenticated,
	));
}

class MainApp extends StatelessWidget {
	const MainApp({
		super.key,
		required this.navigatorKey,
		required this.session,
		required this.apiClient,
		required this.signalRClient,
		required this.repositories,
		required this.database,
		required this.isAuthenticated,
	});

	final GlobalKey<NavigatorState> navigatorKey;
	final SessionManager session;
	final ApiClient apiClient;
	final SignalRClient signalRClient;
	final Repositories repositories;
	final AppDatabase database;
	final bool isAuthenticated;

	@override
	Widget build(BuildContext context) {
		return MultiProvider(
			providers: [
				Provider<SessionManager>.value(value: session),
				Provider<ApiClient>.value(value: apiClient),
				ChangeNotifierProvider<SignalRClient>.value(value: signalRClient),
				Provider<Repositories>.value(value: repositories),
				Provider<AppDatabase>.value(value: database),
			],
			child: MaterialApp(
                navigatorKey: navigatorKey,
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
