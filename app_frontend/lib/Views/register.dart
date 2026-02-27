import 'dart:ui';

import 'package:flutter/material.dart';

class RegisterView extends StatefulWidget {
	const RegisterView({super.key});

	@override
	State<RegisterView> createState() => _RegisterViewState();
}

class _RegisterViewState extends State<RegisterView> {

	final TextEditingController _userController = TextEditingController();
	final TextEditingController _passwordController = TextEditingController();
	final TextEditingController _passwordConfirmController = TextEditingController();

	@override
	Widget build(BuildContext context) {
		double deviceWidth = MediaQuery.of(context).size.width;
		double deviceHeight = MediaQuery.of(context).size.height;

		return GestureDetector(
			behavior: .opaque,
			onTap: () => FocusScope.of(context).unfocus(),
			child: Scaffold(
				body: Stack(
					children: [
						ImageFiltered(
							imageFilter: ImageFilter.blur(sigmaX: 3, sigmaY: 3),
							child: Image.asset(
								'assets/images/fondo_login_registro.png',
								width: deviceWidth,
								height: deviceHeight,
								fit: BoxFit.cover
							)
						),
						Center(
							child: SizedBox(
								width: deviceWidth * 0.85,
								height: deviceHeight * 0.8,
								child: _buildRegisterCard(() => Navigator.of(context).pushReplacementNamed('/onboarding'))
								),
							)
					],
				)
			),
		);
	}

	Card _buildRegisterCard(Function() onRegisterBtnPressed) {
		return Card(
			child: Padding(
				padding: const EdgeInsets.fromLTRB(20, 12, 20, 12),
				child: Column(
					mainAxisSize: .max,
					mainAxisAlignment: .center,
					crossAxisAlignment: .start,

					children: [
						SizedBox(
							width: .infinity,
							child: Center(child: Text("Registrarse", style: Theme.of(context).textTheme.headlineMedium)),
						),
						SizedBox(height: 40), // TODO: Usar mejor medidas basadas en deviceHeight
						Text("Usuario", style: Theme.of(context).textTheme.titleLarge),
						SizedBox(height: 15),
						TextField(controller: _userController),
						SizedBox(height: 40),
						Text("Contraseña", style: Theme.of(context).textTheme.titleLarge),
						SizedBox(height: 15),
						TextField(controller: _passwordController),
						SizedBox(height: 40),
						Text("Confirmar contraseña", style: Theme.of(context).textTheme.titleLarge),
						SizedBox(height: 15),
						TextField(controller: _passwordConfirmController, obscureText: true),
						SizedBox(height: 40),
						FilledButton(onPressed: onRegisterBtnPressed, child: const Text("Registrarse")),
						SizedBox(height: 15),
						SizedBox(
							width: .infinity,
							child: TextButton(onPressed: () => Navigator.of(context).pop(), child: const Text("Volver"))
						)
					],
				),
			),
		);
	}
}