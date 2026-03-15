import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Views/Profile/profile_controller.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

/// Entry point for the profile screen.
/// Creates [ProfileController] and makes it available to the widget subtree.
class ProfileView extends StatelessWidget {
  const ProfileView({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (ctx) => ProfileController(
        sessionManager: ctx.read<SessionManager>(),
        signalRClient: ctx.read<SignalRClient>(),
      ),
      child: const _ProfileBody(),
    );
  }
}

/// Scaffold with AppBar and profile content.
/// Delegates all logic to [ProfileController].
class _ProfileBody extends StatelessWidget {
  const _ProfileBody();

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<ProfileController>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Perfil'),
        // AppBar automatically shows back button when it can pop
      ),
      body: controller.isLoading
          ? const Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(24.0),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.start,
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  // User icon
                  const Icon(
                    Icons.account_circle,
                    size: 120,
                    color: Colors.grey,
                  ),
                  const SizedBox(height: 32),
                  
                  // Username display
                  if (controller.username != null)
                    Text(
                      controller.username!,
                      style: Theme.of(context).textTheme.headlineMedium,
                    ),
                  
                  // Error message
                  if (controller.errorMessage != null) ...[
                    const SizedBox(height: 16),
                    Text(
                      controller.errorMessage!,
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                            color: Theme.of(context).colorScheme.error,
                          ),
                    ),
                  ],
                  
                  const SizedBox(height: 48),
                  
                  // Logout button
                  SizedBox(
                    width: double.infinity,
                    child: FilledButton.icon(
                      onPressed: controller.isLoading
                          ? null
                          : () => _onLogout(context),
                      icon: const Icon(Icons.logout),
                      label: const Text('Cerrar sesión'),
                    ),
                  ),
                ],
              ),
            ),
    );
  }

  Future<void> _onLogout(BuildContext context) async {
    final controller = context.read<ProfileController>();
    final success = await controller.logout();

    if (!success || !context.mounted) return;

    // Navigate to login and remove all previous routes from the stack
    Navigator.of(context).pushNamedAndRemoveUntil(
      Routes.login,
      (route) => false,
    );
  }
}
