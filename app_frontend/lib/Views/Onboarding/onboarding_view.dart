import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Repositories/repositories.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Views/Onboarding/onboarding_controller.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

/// Entry point for the onboarding screen.
/// Creates [OnboardingController] and makes it available to the widget subtree.
class OnboardingView extends StatelessWidget {
  const OnboardingView({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (ctx) => OnboardingController(
        repository: ctx.read<Repositories>().families,
        sessionManager: ctx.read<SessionManager>(),
        signalRClient: ctx.read<SignalRClient>(),
      ),
      child: const _OnboardingBody(),
    );
  }
}

/// Main onboarding screen with welcome message and action buttons.
class _OnboardingBody extends StatelessWidget {
  const _OnboardingBody();

  @override
  Widget build(BuildContext context) {
    final double deviceWidth = MediaQuery.of(context).size.width;
    final double deviceHeight = MediaQuery.of(context).size.height;

    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.start,
          crossAxisAlignment: CrossAxisAlignment.center,
          mainAxisSize: MainAxisSize.max,
          children: [
            SizedBox(height: deviceHeight * 0.1),
            Icon(Icons.local_grocery_store, size: deviceWidth * 0.25),
            SizedBox(height: deviceHeight * 0.025),
            Text(
              '¡Bienvenid@ a Shared Groceries!',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            SizedBox(height: deviceHeight * 0.025),
            Padding(
              padding: EdgeInsets.symmetric(horizontal: deviceWidth * 0.075),
              child: Text(
                'Para comenzar crea o únete a una familia. De esa forma podrás compartir tus listas de compras y colaborar con tus familiares.',
                style: Theme.of(context).textTheme.bodyMedium,
                textAlign: TextAlign.center,
              ),
            ),
            SizedBox(height: deviceHeight * 0.025),
            SizedBox(
              width: deviceWidth * 0.85,
              child: ElevatedButton(
                onPressed: () => _showCreateFamilyModal(context),
                child: const Text('Crear familia'),
              ),
            ),
            SizedBox(height: deviceHeight * 0.025),
            SizedBox(
              width: deviceWidth * 0.85,
              child: ElevatedButton(
                onPressed: () => _showJoinFamilyModal(context),
                child: const Text('Unirse a una familia'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  /// Shows a modal bottom sheet for creating a new family.
  void _showCreateFamilyModal(BuildContext context) {
    final controller = context.read<OnboardingController>();
    // Clear any previous error when opening a new modal
    controller.clearError();

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (modalContext) => ChangeNotifierProvider.value(
        value: controller,
        child: const _CreateFamilyModal(),
      ),
    );
  }

  /// Shows a modal bottom sheet for joining an existing family.
  void _showJoinFamilyModal(BuildContext context) {
    final controller = context.read<OnboardingController>();
    // Clear any previous error when opening a new modal
    controller.clearError();

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (modalContext) => ChangeNotifierProvider.value(
        value: controller,
        child: const _JoinFamilyModal(),
      ),
    );
  }
}

/// Modal for creating a new family.
class _CreateFamilyModal extends StatelessWidget {
  const _CreateFamilyModal();

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<OnboardingController>();

    return SingleChildScrollView(
      child: Padding(
        padding: EdgeInsets.only(
          bottom:
              MediaQuery.of(context).viewInsets.bottom +
              MediaQuery.of(context).size.height * 0.02,
          left: 20,
          right: 20,
          top: 20,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'Nombre de la familia:',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 10),
            TextField(
              controller: controller.familyNameController,
              enabled: !controller.isCreating,
            ),
            // Error message — only rendered when non-null.
            if (controller.errorMessage != null) ...[
              const SizedBox(height: 12),
              Text(
                controller.errorMessage!,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: Theme.of(context).colorScheme.error,
                ),
              ),
            ],
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: controller.isCreating
                  ? null
                  : () => _onCreateFamily(context),
              child: controller.isCreating
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Crear familia'),
            ),
            const SizedBox(height: 10),
          ],
        ),
      ),
    );
  }

  Future<void> _onCreateFamily(BuildContext context) async {
    final controller = context.read<OnboardingController>();
    final success = await controller.createFamily();

    if (!success || !context.mounted) return;

    // Close the modal
    Navigator.of(context).pop();

    // Navigate to home
    Navigator.of(context).pushReplacementNamed(Routes.home);
  }
}

/// Modal for joining an existing family.
class _JoinFamilyModal extends StatelessWidget {
  const _JoinFamilyModal();

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<OnboardingController>();

    return SingleChildScrollView(
      child: Padding(
        padding: EdgeInsets.only(
          bottom:
              MediaQuery.of(context).viewInsets.bottom +
              MediaQuery.of(context).size.height * 0.02,
          left: 20,
          right: 20,
          top: 20,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'Código de invitación:',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 10),
            TextField(
              controller: controller.inviteCodeController,
              enabled: !controller.isJoining,
              maxLength: 6,
            ),
            // Error message — only rendered when non-null.
            if (controller.errorMessage != null) ...[
              const SizedBox(height: 12),
              Text(
                controller.errorMessage!,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: Theme.of(context).colorScheme.error,
                ),
              ),
            ],
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: controller.isJoining
                  ? null
                  : () => _onJoinFamily(context),
              child: controller.isJoining
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Unirse'),
            ),
            const SizedBox(height: 10),
          ],
        ),
      ),
    );
  }

  Future<void> _onJoinFamily(BuildContext context) async {
    final controller = context.read<OnboardingController>();
    final success = await controller.joinFamily();

    if (!success || !context.mounted) return;

    // Close the modal
    Navigator.of(context).pop();

    // Navigate to home
    Navigator.of(context).pushReplacementNamed(Routes.home);
  }
}
