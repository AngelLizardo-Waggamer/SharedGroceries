import 'package:flutter/material.dart';

class AppTheme {
  AppTheme._();

  static ThemeData get light => ThemeData(
    useMaterial3: true,
    colorScheme: const ColorScheme(
      brightness: Brightness.light,

      // Azul petróleo principal — protagonista
      primary: Color(0xFF1A6B8A),
      onPrimary: Color(0xFFFFFFFF),
      primaryContainer: Color(
        0xFFCFE5F0,
      ), // Azul muy claro para fondos de énfasis
      onPrimaryContainer: Color(0xFF001F2A),

      // Secundario: Gris azulado para elementos de apoyo
      secondary: Color(0xFF4C626B),
      onSecondary: Color(0xFFFFFFFF),
      secondaryContainer: Color(0xFFCFE6F1),
      onSecondaryContainer: Color(0xFF071E26),

      // Terciario: Un verde azulado sutil (mantiene la frescura sin ser "bosque")
      tertiary: Color(0xFF3E666A),
      onTertiary: Color(0xFFFFFFFF),
      tertiaryContainer: Color(0xFFC1EBEE),
      onTertiaryContainer: Color(0xFF002022),

      // Error: Rojo limpio
      error: Color(0xFFBA1A1A),
      onError: Color(0xFFFFFFFF),
      errorContainer: Color(0xFFFFDAD6),
      onErrorContainer: Color(0xFF410002),

      // Superficies frías (Adiós al crema)
      surface: Color(0xFFF8FAFC), // Blanco con un toque de azul
      onSurface: Color(0xFF191C1E),
      surfaceContainerHighest: Color(
        0xFFE1E3E8,
      ), // Gris azulado claro para superficies
      onSurfaceVariant: Color(0xFF40484C),

      // Contornos neutros/fríos
      outline: Color(0xFF70787D),
      outlineVariant: Color(0xFFC0C8CD),

      inverseSurface: Color(0xFF2E3133),
      onInverseSurface: Color(0xFFF0F1F3),
      inversePrimary: Color(0xFF8ACEE8),
    ),

    // Tipografía con colores más integrados al azul oscuro
    textTheme: const TextTheme(
      displayLarge: TextStyle(
        fontSize: 57,
        fontWeight: FontWeight.w400,
        color: Color(0xFF001F2A),
      ),
      headlineMedium: TextStyle(
        fontSize: 28,
        fontWeight: FontWeight.w600,
        color: Color(0xFF001F2A),
      ),
      headlineSmall: TextStyle(
        fontSize: 24,
        fontWeight: FontWeight.w600,
        color: Color(0xFF001F2A),
      ),
      titleLarge: TextStyle(
        fontSize: 22,
        fontWeight: FontWeight.w600,
        color: Color(0xFF001F2A),
      ),
      bodyLarge: TextStyle(
        fontSize: 18,
        fontWeight: FontWeight.w400,
        height: 1.6,
        color: Color(0xFF191C1E),
      ),
      bodyMedium: TextStyle(
        fontSize: 16,
        fontWeight: FontWeight.w400,
        height: 1.5,
        color: Color(0xFF191C1E),
      ),
      labelLarge: TextStyle(
        fontSize: 16,
        fontWeight: FontWeight.w600,
        color: Color(0xFF1A6B8A),
      ),
    ),

    // Botones con el azul petróleo sólido
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: const Color(0xFF1A6B8A),
        foregroundColor: Colors.white,
        minimumSize: const Size.fromHeight(56),
        shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.all(Radius.circular(16)),
        ),
        textStyle: const TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
      ),
    ),

    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        minimumSize: const Size.fromHeight(56),
        shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.all(Radius.circular(16)),
        ),
        textStyle: const TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
      ),
    ),

    // Cards con tinte azulado sutil en lugar de crema
    cardTheme: const CardThemeData(
      elevation: 0, // Material 3 usa más el color que la sombra
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(20)),
      ),
      color: Color(0xFFFFFFFF),
      surfaceTintColor: Color(0xFFCFE5F0),
    ),

    // AppBar integrada con el tema oscuro/azul
    appBarTheme: const AppBarTheme(
      backgroundColor: Color(0xFF1A6B8A),
      foregroundColor: Color(0xFFFFFFFF),
      elevation: 0,
      centerTitle: true,
      titleTextStyle: TextStyle(
        fontSize: 22,
        fontWeight: FontWeight.w600,
        color: Color(0xFFFFFFFF),
      ),
    ),

    // Inputs con fondo gris azulado claro para mejor contraste
    inputDecorationTheme: const InputDecorationTheme(
      filled: true,
      fillColor: Color(0xFFF0F4F7), // Fondo frío muy suave
      contentPadding: EdgeInsets.symmetric(horizontal: 20, vertical: 18),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(14)),
        borderSide: BorderSide(color: Color(0xFFC0C8CD), width: 1.2),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(14)),
        borderSide: BorderSide(color: Color(0xFFC0C8CD), width: 1.2),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(14)),
        borderSide: BorderSide(color: Color(0xFF1A6B8A), width: 2.0),
      ),
      labelStyle: TextStyle(fontSize: 16, color: Color(0xFF40484C)),
    ),

    // Fondo de pantalla: Blanco azulado sutil
    scaffoldBackgroundColor: const Color(0xFFF4F7F9),

    // Iconos en azul petróleo
    iconTheme: const IconThemeData(size: 28, color: Color(0xFF1A6B8A)),
  );
}
