import 'package:flutter/material.dart';

class OnboardingView extends StatelessWidget {
	OnboardingView({super.key});

	final TextEditingController _familyNameController = TextEditingController();
	final TextEditingController _invitationCodeController = TextEditingController();

	@override
	Widget build(BuildContext context) {

		double deviceWidth = MediaQuery.of(context).size.width;
		double deviceHeight = MediaQuery.of(context).size.height;

		return Scaffold(
			body: Center(
				child: Column(
					mainAxisAlignment: .start,
					crossAxisAlignment: .center,
					mainAxisSize: .max,
				
					children: [
						SizedBox(height: deviceHeight * 0.1),
						Icon(Icons.local_grocery_store, size: deviceWidth * 0.25),
						SizedBox(height: deviceHeight * 0.025),
						Text("¡Bienvenid@ a Shared Groceries!", style: Theme.of(context).textTheme.headlineSmall,),
						SizedBox(height: deviceHeight * 0.025),
						Text("Para comenzar crea o únete a una familia. De esa forma podrás compartir tus listas de compras y colaborar con tus familiares.", style: Theme.of(context).textTheme.bodyMedium, textAlign: .center,),
						SizedBox(height: deviceHeight * 0.025),
						SizedBox(
							width: deviceWidth * 0.85,
							child: ElevatedButton(onPressed: () => _createFamily(context), child: Text("Crear familia"))
						),
						SizedBox(height: deviceHeight * 0.025),
						SizedBox(
							width: deviceWidth * 0.85,
							child: ElevatedButton(onPressed: () => _joinFamily(context), child: Text("Unirse a una familia"))
						),
					],
				),
			),
		);
	}

	void _createFamily(BuildContext context) {

		void onCreateBtnPressed() {
			Navigator.of(context).pop();

			if (_familyNameController.text.isEmpty) {
				ScaffoldMessenger.of(context).showSnackBar(
					SnackBar(
						content: Text("El nombre de la familia no puede estar vacío."),
						duration: Duration(seconds: 1, milliseconds: 200),
						backgroundColor: Theme.of(context).colorScheme.error,
					)
				);
			}
		}

		showModalBottomSheet(context: context,
		isScrollControlled: true,
		builder: (context) {
			return SingleChildScrollView(
				child: Padding(
					padding: EdgeInsets.only(
						bottom: MediaQuery.of(context).viewInsets.bottom + MediaQuery.of(context).size.height * 0.02,
						left: 20,
						right: 20,
						top: 20,
					),
					child: Column(
						mainAxisSize: .min,
						crossAxisAlignment: .stretch,
						children: [
							Text("Nombre de la familia: ", style: Theme.of(context).textTheme.headlineSmall,),
							SizedBox(height: 10),
							TextField(controller: _familyNameController),
							SizedBox(height: 20),
							ElevatedButton(onPressed: () => onCreateBtnPressed(), child: Text("Crear familia")),
							SizedBox(height: 10),
						],
					),
				)
			);
		});
	}

	void _joinFamily(BuildContext context) {

		void onJoinBtnPressed() {
			Navigator.of(context).pop();
			
			if (_invitationCodeController.text.isEmpty || _invitationCodeController.text.length != 6) {
			ScaffoldMessenger.of(context).showSnackBar(
				SnackBar(
					content: Text("El código de invitación no es válido."),
					duration: Duration(seconds: 1, milliseconds: 200),
					backgroundColor: Theme.of(context).colorScheme.error,
				)
			);

			return;
			}
		}

		showModalBottomSheet(context: context,
		isScrollControlled: true,
		builder: (context) {
			return SingleChildScrollView(
				child: Padding(
					padding: EdgeInsets.only(
						bottom: MediaQuery.of(context).viewInsets.bottom + MediaQuery.of(context).size.height * 0.02,
						left: 20,
						right: 20,
						top: 20,
					),
					child: Column(
						mainAxisSize: .min,
						crossAxisAlignment: .stretch,
						children: [
							Text("Código de invitación: ", style: Theme.of(context).textTheme.headlineSmall,),
							SizedBox(height: 10),
							TextField(controller: _invitationCodeController),
							SizedBox(height: 20),
							ElevatedButton(onPressed: onJoinBtnPressed, child: Text("Unirse")),
							SizedBox(height: 10),
						],
					),
				)
			);
		});
	}
}