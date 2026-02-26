import 'package:app_frontend/Views/home.dart';
import 'package:app_frontend/Views/login.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widget_previews.dart';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  @Preview(
    name: 'MainApp'
  )

  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      initialRoute: '/login',

      routes: {
        '/': (context) => const HomeView(),
        '/login': (context) => const LoginView(),
      },
    );
  }
}
