import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/API/signal_r_client.dart';
import 'package:app_frontend/Repositories/repositories.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Views/Family/family_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';

/// Entry point for the family screen.
/// Creates [FamilyController] and makes it available to the widget subtree.
class FamilyView extends StatelessWidget {
  const FamilyView({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (ctx) => FamilyController(
        repository: ctx.read<Repositories>().families,
        sessionManager: ctx.read<SessionManager>(),
        signalRClient: ctx.read<SignalRClient>(),
      ),
      child: const _FamilyBody(),
    );
  }
}

/// Scaffold with AppBar and family content.
/// Delegates all logic to [FamilyController].
class _FamilyBody extends StatelessWidget {
  const _FamilyBody();

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<FamilyController>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Familia'),
      ),
      body: controller.isLoading
          ? const Center(child: CircularProgressIndicator())
          : controller.errorMessage != null
              ? _buildError(context, controller)
              : _buildContent(context, controller),
    );
  }

  Widget _buildError(BuildContext context, FamilyController controller) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.error_outline,
              size: 64,
              color: Colors.grey,
            ),
            const SizedBox(height: 16),
            Text(
              controller.errorMessage!,
              style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                    color: Theme.of(context).colorScheme.error,
                  ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildContent(BuildContext context, FamilyController controller) {
    final familyData = controller.familyData;

    if (familyData == null) {
      return const Center(
        child: Text('No se encontraron datos de la familia'),
      );
    }

    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const SizedBox(height: 20),
            
            // Family icon
            const Icon(
              Icons.family_restroom,
              size: 100,
              color: Colors.grey,
            ),
            const SizedBox(height: 32),

            // Family name section
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Nombre de la familia',
                      style: Theme.of(context).textTheme.labelLarge?.copyWith(
                            color: Theme.of(context).colorScheme.primary,
                          ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      familyData.name,
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Invite code section
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Código de invitación',
                      style: Theme.of(context).textTheme.labelLarge?.copyWith(
                            color: Theme.of(context).colorScheme.primary,
                          ),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Expanded(
                          child: Text(
                            familyData.inviteCode,
                            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                  fontFamily: 'monospace',
                                ),
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.copy),
                          onPressed: () => _copyInviteCode(context, familyData.inviteCode),
                          tooltip: 'Copiar código',
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 48),

            // Leave family button
            FilledButton.icon(
              onPressed: controller.isLeavingFamily
                  ? null
                  : () => _showLeaveConfirmation(context),
              icon: const Icon(Icons.exit_to_app),
              label: controller.isLeavingFamily
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Abandonar familia'),
              style: FilledButton.styleFrom(
                backgroundColor: Theme.of(context).colorScheme.error,
              ),
            ),

            // Error message when leaving fails
            if (controller.errorMessage != null && !controller.isLoading) ...[
              const SizedBox(height: 16),
              Text(
                controller.errorMessage!,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Theme.of(context).colorScheme.error,
                    ),
                textAlign: TextAlign.center,
              ),
            ],
          ],
        ),
      ),
    );
  }

  void _copyInviteCode(BuildContext context, String inviteCode) {
    Clipboard.setData(ClipboardData(text: inviteCode));
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Código copiado al portapapeles'),
        duration: Duration(seconds: 2),
      ),
    );
  }

  void _showLeaveConfirmation(BuildContext context) {
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('¿Abandonar familia?'),
        content: const Text(
          'Si abandonas la familia, necesitarás el código de invitación para volver a unirte. ¿Estás seguro?',
        ),
        actions: [
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.secondary,
            ),
            child: const Text('Cancelar'),
          ),
          SizedBox(height: 10,),
          FilledButton(
            onPressed: () {
              Navigator.of(dialogContext).pop();
              _onLeaveFamily(context);
            },
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            child: const Text('Abandonar'),
          ),
        ],
      ),
    );
  }

  Future<void> _onLeaveFamily(BuildContext context) async {
    final controller = context.read<FamilyController>();
    final success = await controller.leaveFamily();

    if (!success || !context.mounted) return;

    // Navigate to onboarding and remove all previous routes
    Navigator.of(context).pushNamedAndRemoveUntil(
      Routes.onboarding,
      (route) => false,
    );
  }
}
