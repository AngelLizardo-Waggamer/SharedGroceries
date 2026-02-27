import 'dart:ui';

import 'package:flutter/material.dart';

class LoginView extends StatefulWidget {
	const LoginView({super.key});

	@override
	State<LoginView> createState() => _LoginViewState();
}

class _LoginViewState extends State<LoginView> {

	final TextEditingController _userController = TextEditingController();
	final TextEditingController _passwordController = TextEditingController();

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
								height: deviceHeight * 0.65,
								child: _buildLoginCard(() => Navigator.of(context).pushReplacementNamed('/home'), () => Navigator.of(context).pushNamed('/register'))
								),
							)
					],
				)
			),
		);

	}

	Card _buildLoginCard(Function() onLoginBtnPressed, Function() onRegisterBtnPressed) {
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
							child: Center(child: Text("Iniciar sesión", style: Theme.of(context).textTheme.headlineMedium)),
						),
						SizedBox(height: 40), // TODO: Usar mejor medidas basadas en deviceHeight
						Text("Usuario", style: Theme.of(context).textTheme.titleLarge),
						SizedBox(height: 15),
						TextField(controller: _userController),
						SizedBox(height: 40),
						Text("Contraseña", style: Theme.of(context).textTheme.titleLarge),
						SizedBox(height: 15),
						TextField(controller: _passwordController, obscureText: true),
						SizedBox(height: 40),
						FilledButton(onPressed: onLoginBtnPressed, child: const Text("Ingresar")),
						SizedBox(height: 15),
						SizedBox(
							width: .infinity,
							child: TextButton(onPressed: onRegisterBtnPressed, child: const Text("Registrarse"))
						)
					],
				)
			),
		);
	}
}