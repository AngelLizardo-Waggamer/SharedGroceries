import 'package:flutter/material.dart';

class HomeView extends StatelessWidget {
	const HomeView({super.key});

	@override
	Widget build(BuildContext context) {
    // TODO: Usar mejor medidas basadas en deviceWidth y Height
		// double deviceWidth = MediaQuery.of(context).size.width;
		// double deviceHeight = MediaQuery.of(context).size.height;

		// Mostrar un mensaje de bienvenida al cargar la pantalla justo después del primer frame
		WidgetsBinding.instance.addPostFrameCallback((_) {
			_showSnackBar(context, "¡Bienvenido a Shared Groceries!");
		});

		return Scaffold(
			appBar: AppBar(
				title: const Text('Mis listas'),
				centerTitle: true,
				leading: const Icon(Icons.local_grocery_store),
				actions: [PopupMenuButton(itemBuilder: buildPopupMenuItem)],
			),

			body: FutureBuilder(future: Future.delayed(Duration(seconds: 2)), builder: futureFetchingListsVisualization)
		);
	}

	Widget futureFetchingListsVisualization(BuildContext context, snapshot) {
				if (snapshot.connectionState == ConnectionState.waiting) {
					return const Center(child: CircularProgressIndicator());
				} else if (snapshot.hasError) {
					return Center(
						child: Text('Error al cargar las listas',
							style: TextStyle(color: Theme.of(context).colorScheme.onError),
						)
					);
				} else {
					if (snapshot.data == null || snapshot.data <= 0) {
						return Padding(
							padding: const EdgeInsets.only(left: 40, right: 40),
								child: Expanded(
									child: Column(
										mainAxisAlignment: .center,
										crossAxisAlignment: .center,
										mainAxisSize: .max,
										children: [
											Text("No hay listas disponibles", style: Theme.of(context).textTheme.headlineMedium,),
											SizedBox(height: 40,),
											ElevatedButton(onPressed: () {}, child: Text("Crear nueva lista"))
										],
									),
								),
						);
					}

					return ListView.builder(itemBuilder: listItemBuilder, itemCount: snapshot.data.length);
				}
			}

	// TODO: Reemplazar esto por la visualización de los datos que se obtengan de la API.
	Widget listItemBuilder(BuildContext context, int index) {
		return ListTile(
			title: Text("Lista $index"),
			subtitle: Text("Descripción de la lista $index"),
			trailing: Icon(Icons.arrow_forward_ios),
			onTap: () {},
		);
	}

	List<PopupMenuEntry<String>> buildPopupMenuItem(BuildContext context) => [
		const PopupMenuItem(value: 'family', child: Text('Familia')),
	];

	void _showSnackBar(BuildContext context, String message) {
		ScaffoldMessenger.of(context).showSnackBar(
			SnackBar(content: Text(message), duration: Duration(seconds: 1, milliseconds: 500),),
		);
	}

	
}