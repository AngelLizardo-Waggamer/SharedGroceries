import 'package:app_frontend/Auth/session_manager.dart';
import 'package:app_frontend/Repositories/repositories.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:app_frontend/Views/Home/home_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:intl/intl.dart';
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
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(
          controller.isSelectionMode
              ? '${controller.selectedListIds.length} seleccionado(s)'
              : 'Mis listas',
        ),
        centerTitle: true,
        leading: controller.isSelectionMode
            ? IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => controller.clearSelection(),
              )
            : const Icon(Icons.local_grocery_store),
        actions: [
          if (!controller.isSelectionMode)
            PopupMenuButton<String>(
              itemBuilder: _buildPopupMenuItems,
              onSelected: (value) => _handleMenuSelection(context, value),
            ),
        ],
      ),
      body: _buildBody(controller),
      floatingActionButton: controller.shoppingLists.isNotEmpty
          ? FloatingActionButton(
              onPressed: controller.isSelectionMode
                  ? () => _confirmDeleteLists(context)
                  : () => _showCreateListModal(context),
              backgroundColor: controller.isSelectionMode
                  ? Theme.of(context).colorScheme.error
                  : null,
              child: Icon(
                controller.isSelectionMode ? Icons.delete : Icons.add,
                color: controller.isSelectionMode
                    ? Theme.of(context).colorScheme.onError
                    : null,
              ),
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
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
      itemCount: controller.shoppingLists.length,
      itemBuilder: (context, index) {
        final list = controller.shoppingLists[index];
        final isSelected = controller.isListSelected(list.id);

        return Padding(
          padding: const EdgeInsets.symmetric(vertical: 8),
          child: Card(
            elevation: isSelected ? 3 : 1,
            color: isSelected
                ? Theme.of(
                    context,
                  ).colorScheme.primaryContainer.withValues(alpha: 0.35)
                : null,
            child: InkWell(
              borderRadius: BorderRadius.circular(12),
              onTap: () {
                if (controller.isSelectionMode) {
                  controller.toggleSelection(list.id);
                } else {
                  Navigator.of(context).pushNamed(
                    Routes.shoppingList,
                    arguments: {
                      'listId': list.id,
                      'listName': list.name,
                    },
                  );
                }
              },
              onLongPress: () {
                if (!controller.isSelectionMode) {
                  controller.toggleSelection(list.id);
                }
              },
              child: ConstrainedBox(
                constraints: const BoxConstraints(minHeight: 90),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 14,
                  ),
                  child: Row(
                    children: [
                      Icon(
                        Icons.shopping_bag_outlined,
                        size: 30,
                        color: Theme.of(context).colorScheme.primary,
                      ),
                      const SizedBox(width: 14),
                      Expanded(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              list.name,
                              style: Theme.of(
                                context,
                              ).textTheme.titleMedium?.copyWith(fontSize: 18),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            const SizedBox(height: 6),
                            Text(
                              'Creada el ${_formatDate(list.createdAt)}',
                              style: Theme.of(
                                context,
                              ).textTheme.bodyMedium?.copyWith(fontSize: 15),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                      if (controller.isSelectionMode)
                        Icon(
                          isSelected
                              ? Icons.check_circle
                              : Icons.radio_button_unchecked,
                          size: 24,
                          color: isSelected
                              ? Theme.of(context).colorScheme.primary
                              : Theme.of(
                                  context,
                                ).colorScheme.onSurface.withValues(alpha: 0.45),
                        )
                      else
                        const Icon(Icons.arrow_forward_ios, size: 22),
                    ],
                  ),
                ),
              ),
            ),
          ),
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
    return DateFormat('dd/MM/yyyy').format(date);
  }

  /// Shows a confirmation dialog before deleting selected lists.
  void _confirmDeleteLists(BuildContext context) {
    final controller = context.read<HomeController>();
    final count = controller.selectedListIds.length;

    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Eliminar listas'),
        content: Text(
          count == 1
              ? '¿Estás seguro de que deseas eliminar esta lista?'
              : '¿Estás seguro de que deseas eliminar $count listas?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () async {
              Navigator.of(dialogContext).pop();
              await _deleteSelectedLists(context);
            },
            child: const Text('Eliminar'),
          ),
        ],
      ),
    );
  }

  /// Deletes the selected lists and shows a result message.
  Future<void> _deleteSelectedLists(BuildContext context) async {
    final controller = context.read<HomeController>();
    final count = controller.selectedListIds.length;
    final success = await controller.deleteSelectedLists();

    if (!context.mounted) return;

    if (success) {
      _showSnackBar(
        context,
        count == 1
            ? 'Lista eliminada exitosamente'
            : '$count listas eliminadas exitosamente',
      );
    } else {
      _showSnackBar(
        context,
        controller.errorMessage ?? 'Error al eliminar las listas',
      );
    }
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
