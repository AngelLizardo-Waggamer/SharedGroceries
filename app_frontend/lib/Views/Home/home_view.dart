import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/repositories.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Views/Home/home_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:provider/provider.dart';

/// Entry point for the home screen.
/// Creates [HomeController] and makes it available to the widget subtree.
class HomeView extends StatelessWidget {
  const HomeView({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (ctx) => HomeController(
        repository: ctx.read<Repositories>().shoppingLists,
        sessionManager: ctx.read<SessionManager>(),
      ),
      child: const _HomeBody(),
    );
  }
}

/// Main home screen with app bar and shopping lists.
class _HomeBody extends StatefulWidget {
  const _HomeBody();

  @override
  State<_HomeBody> createState() => _HomeBodyState();
}

class _HomeBodyState extends State<_HomeBody> {
  @override
  void initState() {
    super.initState();

    // Check if user has a family and fetch lists after first frame
    SchedulerBinding.instance.addPostFrameCallback((_) {
      _initializeScreen();
    });
  }

  Future<void> _initializeScreen() async {
    final controller = context.read<HomeController>();

    // First check if user has a family
    await controller.checkFamilyId();

    if (!mounted) return;

    // If user needs onboarding, redirect them
    if (controller.needsOnboarding) {
      Navigator.of(context).pushReplacementNamed(Routes.onboarding);
      return;
    }

    // Show welcome message
    _showSnackBar(context, '¡Bienvenido a Shared Groceries!');

    // Fetch shopping lists
    await controller.fetchLists();
  }

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<HomeController>();

    // Show loading indicator while checking family status
    if (!controller.hasCheckedFamily) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis listas'),
        centerTitle: true,
        leading: const Icon(Icons.local_grocery_store),
        actions: [
          PopupMenuButton<String>(
            itemBuilder: _buildPopupMenuItems,
            onSelected: (value) => _handleMenuSelection(context, value),
          ),
        ],
      ),
      body: _buildBody(controller),
      floatingActionButton: controller.shoppingLists.isNotEmpty
          ? FloatingActionButton(
              onPressed: () => _showCreateListModal(context),
              child: const Icon(Icons.add),
            )
          : null,
    );
  }

  Widget _buildBody(HomeController controller) {
    // Show loading indicator
    if (controller.isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    // Show error message
    if (controller.errorMessage != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              'Error al cargar las listas',
              style: TextStyle(color: Theme.of(context).colorScheme.error),
            ),
            const SizedBox(height: 16),
            Text(
              controller.errorMessage!,
              style: Theme.of(context).textTheme.bodySmall,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => controller.fetchLists(),
              child: const Text('Reintentar'),
            ),
          ],
        ),
      );
    }

    // Show empty state
    if (controller.shoppingLists.isEmpty) {
      return _buildEmptyState();
    }

    // Show list of shopping lists
    return ListView.builder(
      itemCount: controller.shoppingLists.length,
      itemBuilder: (context, index) {
        final list = controller.shoppingLists[index];
        return ListTile(
          title: Text(list.name),
          subtitle: Text(
            'Creada: ${_formatDate(list.createdAt)}',
            style: Theme.of(context).textTheme.bodySmall,
          ),
          trailing: const Icon(Icons.arrow_forward_ios),
          onTap: () {
            // TODO: Navigate to list details
          },
        );
      },
    );
  }

  Widget _buildEmptyState() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 40),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Text(
            'No hay listas disponibles',
            style: Theme.of(context).textTheme.headlineMedium,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 40),
          ElevatedButton(
            onPressed: () => _showCreateListModal(context),
            child: const Text('Crear nueva lista'),
          ),
        ],
      ),
    );
  }

  List<PopupMenuEntry<String>> _buildPopupMenuItems(BuildContext context) {
    return const [
      PopupMenuItem(value: 'family', child: Text('Familia')),
      PopupMenuItem(value: 'profile', child: Text('Perfil')),
    ];
  }

  void _handleMenuSelection(BuildContext context, String value) {
    switch (value) {
      case 'family':
        Navigator.of(context).pushNamed(Routes.family);
        break;
      case 'profile':
        Navigator.of(context).pushNamed(Routes.profile);
        break;
    }
  }

  /// Shows a modal bottom sheet for creating a new list.
  void _showCreateListModal(BuildContext context) {
    final controller = context.read<HomeController>();
    controller.clearError();

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (modalContext) => ChangeNotifierProvider.value(
        value: controller,
        child: const _CreateListModal(),
      ),
    );
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }

  void _showSnackBar(BuildContext context, String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        duration: const Duration(seconds: 1, milliseconds: 500),
      ),
    );
  }
}

/// Modal for creating a new shopping list.
class _CreateListModal extends StatelessWidget {
  const _CreateListModal();

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<HomeController>();

    return SingleChildScrollView(
      child: Padding(
        padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom +
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
              'Nombre de la lista:',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 10),
            TextField(
              controller: controller.listNameController,
              enabled: !controller.isCreatingList,
              autofocus: true,
              textCapitalization: TextCapitalization.sentences,
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
              onPressed: controller.isCreatingList
                  ? null
                  : () => _onCreateList(context),
              child: controller.isCreatingList
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Crear lista'),
            ),
            const SizedBox(height: 10),
          ],
        ),
      ),
    );
  }

  Future<void> _onCreateList(BuildContext context) async {
    final controller = context.read<HomeController>();
    final success = await controller.createList();

    if (!success || !context.mounted) return;

    // Close the modal
    Navigator.of(context).pop();

    // Show success message
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Lista creada exitosamente'),
        duration: Duration(seconds: 1, milliseconds: 500),
      ),
    );
  }
}
