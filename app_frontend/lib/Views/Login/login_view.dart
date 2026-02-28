import 'dart:ui';

import 'package:app_frontend/Repositories/repositories.dart';
import 'package:app_frontend/Views/Login/login_controller.dart';
import 'package:app_frontend/Routes/routes.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

/// Entry point for the login screen.
/// Creates [LoginController] and makes it available to the widget subtree.
class LoginView extends StatelessWidget {
  const LoginView({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (ctx) => LoginController(
        repository: ctx.read<Repositories>().auth,
      ),
      child: const _LoginBody(),
    );
  }
}

/// Scaffold + blurred background. Delegates all logic to [LoginController].
class _LoginBody extends StatelessWidget {
  const _LoginBody();

  @override
  Widget build(BuildContext context) {
    final double deviceWidth = MediaQuery.of(context).size.width;
    final double deviceHeight = MediaQuery.of(context).size.height;

    return GestureDetector(
      // opaque so taps on empty areas still dismiss the keyboard
      behavior: HitTestBehavior.opaque,
      onTap: () => FocusScope.of(context).unfocus(),
      child: Scaffold(
        body: Stack(
          children: [
            // Bottom layer: blurred background image
            ImageFiltered(
              imageFilter: ImageFilter.blur(sigmaX: 3, sigmaY: 3),
              child: Image.asset(
                'assets/images/fondo_login_registro.png',
                width: deviceWidth,
                height: deviceHeight,
                fit: BoxFit.cover,
              ),
            ),
            // Top layer: centered login card
            Center(
              child: SizedBox(
                width: deviceWidth * 0.85,
                height: deviceHeight * 0.65,
                child: const _LoginCard(),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Form fields and action buttons.
/// Rebuilt automatically whenever [LoginController.notifyListeners] fires.
class _LoginCard extends StatelessWidget {
  const _LoginCard();

  @override
  Widget build(BuildContext context) {
    // watch subscribes to the controller: rebuilds this widget on every notifyListeners().
    final controller = context.watch<LoginController>();

    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 12),
        child: Column(
          mainAxisSize: MainAxisSize.max,
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(
              width: double.infinity,
              child: Center(
                child: Text(
                  'Iniciar sesión',
                  style: Theme.of(context).textTheme.headlineMedium,
                ),
              ),
            ),
            const SizedBox(height: 40),
            Text('Usuario', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 15),
            // TextEditingControllers live in LoginController, not here.
            TextField(controller: controller.usernameController),
            const SizedBox(height: 40),
            Text('Contraseña', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 15),
            TextField(
              controller: controller.passwordController,
              obscureText: true,
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
            const SizedBox(height: 40),
            // onPressed: null disables the button (Flutter built-in behaviour).
            FilledButton(
              onPressed: controller.isLoading ? null : () => _onLogin(context),
              child: controller.isLoading
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Ingresar'),
            ),
            const SizedBox(height: 15),
            SizedBox(
              width: double.infinity,
              child: TextButton(
                onPressed: controller.isLoading
                    ? null
                    : () => Navigator.of(context).pushNamed(Routes.register),
                child: const Text('Registrarse'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _onLogin(BuildContext context) async {
    // read (not watch) — we only need to call a method, not trigger rebuilds.
    final controller = context.read<LoginController>();
    final success = await controller.login();

    if (!success || !context.mounted) return;

    // If the user has no family yet, send them to onboarding first.
    // pushReplacementNamed prevents going back to login via the back button.
    final destination = controller.familyId != null ? Routes.home : Routes.onboarding;
    Navigator.of(context).pushReplacementNamed(destination);
  }
}
