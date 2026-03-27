import 'package:app_frontend/Views/ShoppingList/shopping_list_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter_slidable/flutter_slidable.dart';
import 'package:provider/provider.dart';

/// Entrada de la pantalla de detalle de lista.
/// Inyecta su controller y delega el render al body.
class ShoppingListView extends StatelessWidget {
  final String listId;
  final String listName;

  const ShoppingListView({
    super.key,
    required this.listId,
    required this.listName,
  });

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (_) => ShoppingListController(listId: listId, listName: listName),
      child: const _ShoppingListBody(),
    );
  }
}

class _ShoppingListBody extends StatelessWidget {
  const _ShoppingListBody();

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<ShoppingListController>();
    final colorScheme = Theme.of(context).colorScheme;

    return DefaultTabController(
      length: 3,
      initialIndex: controller.currentTab.index,
      child: Scaffold(
        appBar: AppBar(
          title: Text(controller.listName),
          bottom: TabBar(
            labelColor: colorScheme.onPrimary,
            unselectedLabelColor: colorScheme.onPrimary.withValues(alpha: 0.72),
            indicatorColor: colorScheme.onPrimary,
            onTap: controller.setTabByIndex,
            tabs: const [
              Tab(text: 'Pendiente'),
              Tab(text: 'En carrito'),
              Tab(text: 'Pagado'),
            ],
          ),
        ),
        body: controller.isLoading
            ? const Center(child: CircularProgressIndicator())
            : controller.errorMessage != null
            ? _buildError(context, controller.errorMessage!)
            : _buildItems(context, controller),
        floatingActionButton: FloatingActionButton(
          onPressed: () {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Próximamente podrás agregar ítems.'),
                duration: Duration(seconds: 2),
              ),
            );
          },
          child: const Icon(Icons.add),
        ),
      ),
    );
  }

  Widget _buildError(BuildContext context, String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Text(
          message,
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
            color: Theme.of(context).colorScheme.error,
          ),
          textAlign: TextAlign.center,
        ),
      ),
    );
  }

  Widget _buildItems(BuildContext context, ShoppingListController controller) {
    final items = controller.visibleItems;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(12, 16, 12, 8),
          child: Text(
            _sectionTitle(controller),
            style: Theme.of(context).textTheme.labelLarge,
          ),
        ),
        if (items.isEmpty)
          Expanded(
            child: Center(
              child: Text(
                'No hay ítems para mostrar',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ),
          )
        else
          Expanded(
            child: ListView.separated(
              padding: const EdgeInsets.fromLTRB(12, 8, 12, 24),
              itemCount: items.length,
              separatorBuilder: (_, _) => const SizedBox(height: 10),
              itemBuilder: (context, index) {
                final item = items[index];
                return _buildSlidableItem(context, controller, item);
              },
            ),
          ),
      ],
    );
  }

  Widget _buildSlidableItem(
    BuildContext context,
    ShoppingListController controller,
    ShoppingListItemVM item,
  ) {
    final theme = Theme.of(context);
    final isPendingTab = controller.currentTab == ShoppingListTab.pending;
    final isInCartTab = controller.currentTab == ShoppingListTab.inCart;
    final isPaidTab = controller.currentTab == ShoppingListTab.paid;

    // Construye el tile con esquinas derechas dinámicas para integrarlo con Slidable.
    Widget buildTile({required bool flattenRightCorners}) {
      final radius = BorderRadius.only(
        topLeft: const Radius.circular(10),
        bottomLeft: const Radius.circular(10),
        topRight: Radius.circular(flattenRightCorners ? 0 : 10),
        bottomRight: Radius.circular(flattenRightCorners ? 0 : 10),
      );

      return Card(
        margin: EdgeInsets.zero,
        elevation: 2,
        shadowColor: theme.colorScheme.outline.withValues(alpha: 0.25),
        shape: RoundedRectangleBorder(
          borderRadius: radius,
          side: BorderSide(color: theme.colorScheme.outlineVariant, width: 1),
        ),
        clipBehavior: Clip.antiAlias,
        child: ListTile(
          minTileHeight: 74,
          leading: Icon(
            isPaidTab ? Icons.check_circle : Icons.radio_button_unchecked,
            color: theme.colorScheme.primary,
          ),
          title: Text(item.name, style: theme.textTheme.titleMedium),
          trailing: Icon(
            Icons.drag_indicator,
            color: theme.colorScheme.outline,
          ),
        ),
      );
    }

    if (isPaidTab) {
      // En pagados no hay acciones: solo visualización.
      return buildTile(flattenRightCorners: false);
    }

    return Slidable(
      key: ValueKey(item.id),
      closeOnScroll: true,
      endActionPane: ActionPane(
        motion: const DrawerMotion(),
        // En carrito se muestran dos acciones; en pendientes solo una.
        extentRatio: isInCartTab ? 0.6 : 0.3,
        children: [
          if (isInCartTab)
            SlidableAction(
              onPressed: (_) => _onMoveToPending(context, item.id),
              backgroundColor: theme.colorScheme.secondary,
              foregroundColor: theme.colorScheme.onSecondary,
              icon: Icons.undo,
              label: 'Pendiente',
            ),
          if (isPendingTab || isInCartTab)
            SlidableAction(
              onPressed: (_) => _onDeleteItem(context, item.id),
              backgroundColor: theme.colorScheme.error,
              foregroundColor: theme.colorScheme.onError,
              borderRadius: BorderRadius.only(
                topRight: Radius.circular(10),
                bottomRight: Radius.circular(10),
              ),
              icon: Icons.delete,
              label: 'Eliminar',
            ),
        ],
      ),
      child: Builder(
        builder: (slidableContext) {
          final slidableController = Slidable.of(slidableContext);

          if (slidableController == null) {
            return buildTile(flattenRightCorners: false);
          }

          return AnimatedBuilder(
            animation: slidableController.animation,
            builder: (_, _) {
              // Mientras se abre hacia la izquierda, el borde derecho se aplana.
              final flattenRightCorners = slidableController.ratio < -0.001;
              return buildTile(flattenRightCorners: flattenRightCorners);
            },
          );
        },
      ),
    );
  }

  String _sectionTitle(ShoppingListController controller) {
    // Texto resumen por estado para mantener contexto visible al usuario.
    switch (controller.currentTab) {
      case ShoppingListTab.pending:
        final count = controller.pendingCount;
        return '$count ÍTEM${count == 1 ? '' : 'S'} PENDIENTE${count == 1 ? '' : 'S'}';
      case ShoppingListTab.inCart:
        final count = controller.inCartCount;
        return '$count ÍTEM${count == 1 ? '' : 'S'} EN CARRITO';
      case ShoppingListTab.paid:
        final count = controller.paidCount;
        return '$count ÍTEM${count == 1 ? '' : 'S'} PAGADO${count == 1 ? '' : 'S'}';
    }
  }

  Future<void> _onDeleteItem(BuildContext context, String itemId) async {
    final controller = context.read<ShoppingListController>();
    final success = await controller.deleteItem(itemId);

    if (!context.mounted) return;

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          success
              ? 'Ítem eliminado'
              : controller.errorMessage ?? 'No se pudo eliminar el ítem',
        ),
        duration: const Duration(seconds: 2),
      ),
    );
  }

  Future<void> _onMoveToPending(BuildContext context, String itemId) async {
    final controller = context.read<ShoppingListController>();
    final success = await controller.moveItemToPending(itemId);

    if (!context.mounted) return;

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          success
              ? 'Ítem movido a pendientes'
              : controller.errorMessage ?? 'No se pudo mover el ítem',
        ),
        duration: const Duration(seconds: 2),
      ),
    );
  }
}
